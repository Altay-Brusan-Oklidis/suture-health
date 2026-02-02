using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using SutureHealth.Application.Services;
using SutureHealth.Documents.Services;
using SutureHealth.Linq;
using SutureHealth.Patients;
using SutureHealth.Patients.Services;
using SutureHealth.Requests;
using SutureHealth.Requests.Services;
using SutureHealth.Storage;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Configuration;
using SutureHealth.DataStream;
using Slack.Webhooks;
using Slack.Webhooks.Blocks;
using Slack.Webhooks.Elements;
using Kendo.Mvc.Extensions;
using Newtonsoft.Json;
using SutureHealth.Requests.JsonConverters;
using Amazon.SimpleSystemsManagement.Model;
using System.Globalization;

namespace SutureHealth.Hchb.Services
{
    public class HchbServiceProvider: IHchbServiceProvider
    {
        private string BucketName { get; set; }
        private string OruChannelHost { get; set; }
        private string SlackChannel { get; set; }
        private string AlertPatientMatchSlackChannel { get; set; }
        private string WebhookUrl { get; set; }
        private string WebhookUrlToAlertPatient { get; set; }
        protected readonly string DataStream;
        
        protected HchbWebDbContext HchbWebDbContext { get; set; }        
        protected PatientDbContext PatientContext { get; set; }
        protected RequestDbContext RequestDbContext { get; set; }
        protected IPatientServicesProvider PatientServices { get; }
        protected IRequestServicesProvider RequestServices { get; }
        protected IBinaryStorageService StorageService { get; }
        protected IDocumentServicesProvider DocumentService { get; }
        protected IApplicationService ApplicationService { get; }
        protected IApplicationService SecurityService { get; }
        protected ISlackClient SlackClient, SlackAlertPatientMatchClient;
        
        protected HchbServiceProvider() { }

        public HchbServiceProvider(
            HchbWebDbContext hchbWebDbContext,
            PatientDbContext patientContext,
            RequestDbContext requestDbContext,
            IPatientServicesProvider patientServices,
            IRequestServicesProvider requestServicesProvider,
            IBinaryStorageService storageService,
            IDocumentServicesProvider documentService,
            IApplicationService applicationService,
            IConfiguration configuration
        )
        {
            HchbWebDbContext = hchbWebDbContext;
            PatientContext = patientContext;
            RequestDbContext = requestDbContext;
            PatientServices = patientServices;
            RequestServices = requestServicesProvider;
            StorageService = storageService;
            DocumentService = documentService;
            ApplicationService = applicationService;


            BucketName = configuration["SutureHealth:HomecareHomebase:BucketName"];
            OruChannelHost = configuration["SutureHealth:HomecareHomebase:OruChannelHost"];
            DataStream = configuration["SutureHealth:HomecareHomebase:KinesisName"];

            WebhookUrl = configuration["SutureHealth:HomecareHomebase:WebhookUrl"];
            WebhookUrlToAlertPatient = configuration["SutureHealth:HomecareHomebase:WebhookUrlToAlertPatientMatch"];
            SlackChannel = configuration["SutureHealth:HomecareHomebase:SlackChannel"];
            AlertPatientMatchSlackChannel = configuration["SutureHealth:HomecareHomebase:AlertPatientMatchSlackChannel"];

            SlackClient = new SlackClient(WebhookUrl);
            SlackAlertPatientMatchClient = new SlackClient(WebhookUrlToAlertPatient);
        }

        public async Task<string> ProcessAdtMessage(string message, int logId = -1)
        {
            Adt adtMessage = new(message);
            SlackMessage messagContent = adtMessage.ToNotification();

            try
            {
                // Get Patient's facility Id from branch code
                int facilityId = GetFacilityIdFromBranchCode(adtMessage.BranchCode);

                if (facilityId < 0)
                {
                    await SendNotificationAsync("Patient's facility does not exist." , messagContent);

                    return "Patient's facility does not exist.";
                }

                // Get matched Patient
                PatientMatchingResponse response = null;
                HchbPatientWeb hchbPatient = null;

                // Process ADT Message according to it's Type
                switch (adtMessage.Type)
                {
                    case ADT.A01:
                        (response, hchbPatient) = await MatchPatient(adtMessage, facilityId, false, RequestSource.HCHB);
                        return await ProcessA01Message(adtMessage, response, logId, hchbPatient);

                    case ADT.A03:
                        (response, hchbPatient) = await MatchPatient(adtMessage, facilityId, false, RequestSource.HCHB);
                        return await ProcessA03Message(adtMessage, response, logId, hchbPatient);

                    case ADT.A04:
                    case ADT.A08:
                        (response, hchbPatient) = await MatchPatient(adtMessage, facilityId, true, RequestSource.HCHB);
                        return await ProcessA04Message(adtMessage, response, hchbPatient, facilityId, logId);

                    case ADT.A11:
                        (response, hchbPatient)= await MatchPatient(adtMessage, facilityId, false, RequestSource.HCHB);
                        return await ProcessA11Message(adtMessage, response, logId, hchbPatient);

                    default:
                        return Messages.INVALID_TYPE;
                }
            }
            catch (Exception ex)
            {
                messagContent = adtMessage.ToNotification();
                await SendNotificationAsync(ex.ToString(), messagContent);
                return ex.ToString();
            }            
        }

        public async Task<string> ProcessMdmMessage(string message, int logId = -1)
        {
            Mdm mdmMessage = new(message);
            SlackMessage mdmNotification = null;

            try
            {
                string validationResult = mdmMessage.Patient.Validate();
                if (validationResult != "success")
                {
                    SlackMessage messageContent = mdmMessage.ToNotification();
                    await SendNotificationAsync(validationResult, messageContent);
                    return validationResult;
                }

                mdmMessage.Patient.ManualReviewEnabled = false;
                mdmMessage.Patient.OrganizationId = GetFacilityIdFromBranchCode(mdmMessage.Sender.BranchCode);
                mdmMessage.Patient.RequestSource = RequestSource.HCHB;
                mdmMessage.Patient.SourceDescription = mdmMessage.Rawfilename;

                // Get matched patient
                var (response, hchbPatient) = await MatchMDMPatient(mdmMessage);

                switch (response.MatchLevel)
                {
                    case MatchLevel.Match:
                        // get Sender information                
                        int senderFacilityId = GetFacilityIdFromBranchCode(mdmMessage.Sender.BranchCode);
                        if (senderFacilityId < 0)
                        {
                            mdmNotification = mdmMessage.ToNotification();
                            await SendNotificationAsync(Messages.SENDER_NOTEXIST_ERROR, mdmNotification); 
                            return Messages.SENDER_NOTEXIST_ERROR;
                        }
                            
                        var sender = await ApplicationService.GetMemberByNPIAsync(mdmMessage.Sender.Npi);

                        HchbTemplate parentTemplate = GetHchbTemplate(
                            mdmMessage.Transaction.AdmissionType, 
                            mdmMessage.Transaction.ObservationId, 
                            mdmMessage.Transaction.ObservationText, 
                            mdmMessage.Transaction.PatientType
                        );

                        if (parentTemplate == null || parentTemplate.TemplateId == 0)
                        {
                            SlackMessage messageContent = mdmMessage.ToNotification();
                            await SendNotificationAsync(Messages.TEMPLATE_NOTEXIST_ERROR, messageContent);
                            return Messages.TEMPLATE_NOTEXIST_ERROR;
                        }                       
                        
                        mdmMessage.Transaction.TemplateId = parentTemplate.TemplateId;

                        // create document to sign
                        byte[] content = await GetObjectBytesAsync(BucketName, mdmMessage.Transaction.FileName);
                        var storageKey = await StorageService.SaveToBinaryStorageAsync(BinaryStorageType.Templates, content);
                        var templateId = 0;
                        
                        templateId = await DocumentService.CreateRequestTemplateAsync(
                            parentTemplate.TemplateId, 
                            senderFacilityId, 
                            sender.MemberId, 
                            storageKey
                        );
                        
                        // get Signer information
                        var signer = await ApplicationService.GetMemberByNPIAsync(mdmMessage.Signer.Npi);
                        var signerId = signer.MemberId;
                        if (signer == null)
                        {                            
                            mdmNotification = mdmMessage.ToNotification();
                            await SendNotificationAsync(Messages.SIGNER_NOTEXIST_ERROR, mdmNotification);
                            return Messages.SIGNER_NOTEXIST_ERROR;
                        }
                        mdmMessage.Transaction.SignerId = signerId;

                        int signerFacilityId = GetSignerPrimaryFacility(signer.MemberId);
                        if (signerFacilityId < 0)
                        {
                            mdmNotification = mdmMessage.ToNotification();
                            await SendNotificationAsync(Messages.SIGNER_FACILITY_NOTEXIST_ERROR, mdmNotification);
                            return Messages.SIGNER_FACILITY_NOTEXIST_ERROR;
                        }
                        mdmMessage.Transaction.SignerFacilityId = signerFacilityId;

                        var oldHchbTransaction = HchbWebDbContext.HchbTransactions
                            .OrderByDescending(x => x.SendDate)
                            .FirstOrDefault(x => x.OrderNumber == mdmMessage.Transaction.OrderNumber);

                        ICDCode code = null;
                        if (hchbPatient == null)
                        {
                            hchbPatient = new HchbPatientWeb()
                            {
                                PatientId = response.TopMatch.PatientId,
                                HchbPatientId = mdmMessage.Transaction.HchbPatientId,
                                EpisodeId = mdmMessage.Transaction.EpisodeId,
                                IcdCode = null,
                                Status = mdmMessage.Status
                            };
                            HchbWebDbContext.HchbPatients.Add(hchbPatient);
                        }
                        else if (hchbPatient.IcdCode != null)
                        {
                            code = HchbWebDbContext.ICDCodes.SingleOrDefault(x => 
                                x.CodeType == 10 && x.IcdCode == hchbPatient.IcdCode);
                        }

                        int? icdCodeId = code?.Id;

                        if (oldHchbTransaction == null || oldHchbTransaction.Status == 2)
                        {
                            int requestId;

                            // send request for sign
                            if (mdmMessage.Transaction.ObservationId == "1" && mdmMessage.Transaction.ObservationText == "485")
                            {
                                if (mdmMessage.Transaction.AdmissionType == "RECERTIFICATION")
                                {
                                    requestId = await RequestServices.CreateRequestAsync(sender.MemberId,
                                                                            senderFacilityId,
                                                                            response.TopMatch.PatientId,
                                                                            templateId,
                                                                            signerId,
                                                                            signerFacilityId,
                                                                            null,
                                                                            null,
                                                                            icdCodeId,
                                                                            null,
                                                                            mdmMessage.Transaction.AdmitDate,
                                                                            false);
                                } 
                                else
                                {
                                    requestId = await RequestServices.CreateRequestAsync(sender.MemberId,
                                                                            senderFacilityId,
                                                                            response.TopMatch.PatientId,
                                                                            templateId,
                                                                            signerId,
                                                                            signerFacilityId,
                                                                            null,
                                                                            null,
                                                                            icdCodeId,
                                                                            mdmMessage.Transaction.AdmitDate,
                                                                            null,
                                                                            false);
                                }                                
                            }
                            else
                            {
                                requestId = await RequestServices.CreateRequestAsync(sender.MemberId,
                                                                            senderFacilityId,
                                                                            response.TopMatch.PatientId,
                                                                            templateId,
                                                                            signerId,
                                                                            signerFacilityId,
                                                                            null,
                                                                            null,
                                                                            icdCodeId,
                                                                            mdmMessage.Transaction.OrderDate,
                                                                            null,
                                                                            false);
                            }

                            mdmMessage.Transaction.RequestId = requestId;

                            HchbWebDbContext.HchbTransactions.Add(mdmMessage.Transaction);
                            HchbWebDbContext.SaveChanges();

                            LogProcessMessage(logId);
                            return Messages.DOCUMENT_SEND;
                        } 
                        else
                        {
                            if (oldHchbTransaction.SignerId != signerId || 
                                oldHchbTransaction.SignerFacilityId != signerFacilityId)
                            {
                                await RequestDbContext.ForwardRequestAsync(oldHchbTransaction.RequestId, 
                                    signerId, 
                                    signerFacilityId, 
                                    null, 
                                    null, 
                                    0);

                                oldHchbTransaction.SignerId = signerId;
                                oldHchbTransaction.SignerFacilityId = signerFacilityId;
                                HchbWebDbContext.SaveChanges();
                            }
                            
                            LogProcessMessage(logId);
                            
                            return Messages.DOCUMENT_SEND;
                        }

                    default:
                        mdmNotification = mdmMessage.ToNotification();
                        await SendNotificationAsync(
                            "No Patient corresponds to the medical document (MDM) linked with it.", 
                            mdmNotification
                        );
                        return Messages.PATIENT_NONMATCH;
                }
            }
            catch (Exception ex)
            {
                mdmNotification = mdmMessage.ToNotification();
                await SendNotificationAsync(ex.ToString(), mdmNotification);
                return ex.ToString();
            }            
        }

        public async Task<string> ProcessOruMessage(string message)
        {
            try
            {
                OruStream stream = JsonConvert.DeserializeObject<OruStream>(message,new OruStreamConverter());
                HchbTransaction transaction = HchbWebDbContext.HchbTransactions.SingleOrDefault(t => 
                    t.RequestId == stream.RequestId);
                string result = "";

                if (transaction != null)
                {
                    Oru oru = new()
                    {
                        RequestId = stream.RequestId,
                        FirstName = stream.FirstName,
                        LastName = stream.LastName,
                        Gender = stream.Gender,
                        BirthDate = stream.BirthDate,
                        RejectReason = stream.RejectReason,
                        ResultDate = stream.ResultDate,
                        ResultStatus = stream.ResultStatus,
                        OrderNumber = transaction.OrderNumber,
                        RequestedDateTime = transaction.OrderDate?.ToString("yyyyMMddHHmmss") ,
                        ObservationId = transaction.ObservationId,
                        ObservationText = transaction.ObservationText,
                        PatientId = transaction.HchbPatientId,
                        EpisodeId = transaction.EpisodeId
                    };

                    using var httpClient = new HttpClient();
                    HttpResponseMessage response;

                    string json = JsonConvert.SerializeObject(oru);
                    StringContent content = new(json, Encoding.UTF8, "application/json");
                    response = await httpClient.PostAsync(OruChannelHost, content);

                    result = response.ToString();
                    if (!response.IsSuccessStatusCode)
                    {
                        SlackMessage oruNotification = oru.ToNotification();
                        await SendNotificationAsync(response.ToString(), oruNotification);
                    }

                    transaction.Status = stream.ResultStatus == "F" ? 1 : 2;
                    HchbWebDbContext.SaveChanges();
                }
                else
                {
                    result = Messages.NOT_VALID_HCHB_DOCUMENT_ERROR;
                }

                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        private async Task<(PatientMatchingResponse, HchbPatientWeb)> MatchMDMPatient (Mdm mdmMessage)
        {
            var existingHchbPatient = HchbWebDbContext.HchbPatients
                .SingleOrDefault(p => p.HchbPatientId == mdmMessage.Transaction.HchbPatientId &&
                                      p.EpisodeId == mdmMessage.Transaction.EpisodeId);

            if (existingHchbPatient == null)
            {
                PatientMatchingResponse response = await PatientServices.MatchAsync(mdmMessage.Patient);
                return (response, null);
            }

            var patientId = existingHchbPatient.PatientId;
            var existingPatient = PatientContext.Patients.SingleOrDefault(p => p.PatientId == patientId);

            var match = new MatchingResult<Patient>
            {
                Match = existingPatient,
                Rules = Array.Empty<MatchingRuleResult<Patient>>(),
                Score = 1000
            };

            return (new PatientMatchingResponse
            {
                MatchLevel = MatchLevel.Match,
                TopMatch = existingPatient,
                MatchResults = new[] { match }
            }, existingHchbPatient);
        }

        private async Task<(PatientMatchingResponse, HchbPatientWeb)> MatchPatient(
            Adt adtMessage, 
            int facilityId, 
            bool manualReview, 
            RequestSource source
        )
        {
            var existingHchbPatient = HchbWebDbContext.HchbPatients
                .SingleOrDefault(p => p.HchbPatientId == adtMessage.HchbPatient.HchbPatientId &&
                                      p.EpisodeId == adtMessage.HchbPatient.EpisodeId);


            if (existingHchbPatient != null)
            {
                var patientId = existingHchbPatient.PatientId;
                var existingPatient = PatientContext.Patients.SingleOrDefault(p => p.PatientId == patientId);

                var match = new MatchingResult<Patient>
                {
                    Match = existingPatient,
                    Rules = Array.Empty<MatchingRuleResult<Patient>>(),
                    Score = 1000
                };

                return (new PatientMatchingResponse
                {
                    MatchLevel = MatchLevel.Match,
                    TopMatch = existingPatient,
                    MatchResults = new[] { match }
                }, existingHchbPatient);
            }
            
            var identifiers = new List<IIdentifier>(
                adtMessage.Patient.Identifiers.Select(
                    identifier => new Patients.PatientIdentifier
                    {
                        Type = identifier.Type,
                        Value = identifier.Value
                    }
                )
            );

            PatientMatchingRequest request = new()
            {
                Birthdate = adtMessage.Patient.Birthdate,
                FirstName = adtMessage.Patient.FirstName,
                MiddleName = adtMessage.Patient.MiddleName,
                LastName = adtMessage.Patient.LastName,
                Gender = adtMessage.Patient.Gender,
                AddressLine1 = adtMessage.Patient.Addresses.First().Line1,
                City = adtMessage.Patient.Addresses.First().City,
                StateOrProvince = adtMessage.Patient.Addresses.First().StateOrProvince,
                PostalCode = adtMessage.Patient.Addresses.First().PostalCode,
                Ids = identifiers,
                Phones = adtMessage.Patient.Phones,
                OrganizationId = facilityId,
                ManualReviewEnabled = manualReview,
                RequestSource = source,
                SourceDescription = adtMessage.MessageControlId
            };

            string validationResult = request.Validate();
            if (validationResult != "success")
            {
                SlackMessage messageContent = adtMessage.ToNotification();
                await SendNotificationAsync(validationResult, messageContent);
                throw new InvalidDataException(validationResult);
            }

            return (await PatientServices.MatchAsync(request), null);
        }

        private async Task<string> ProcessA01Message(
            Adt adtMessage, 
            PatientMatchingResponse response, 
            int logId, 
            HchbPatientWeb hchbPatient
        )
        {
            if (response.MatchLevel == MatchLevel.Match) {
                if (hchbPatient == null)
                {
                    adtMessage.HchbPatient.PatientId = response.TopMatch.PatientId;
                    adtMessage.HchbPatient.Status = Status.CURRENT;
                    HchbWebDbContext.HchbPatients.Add(adtMessage.HchbPatient);
                }
                else
                {
                    hchbPatient.Status = Status.CURRENT;
                    HchbWebDbContext.HchbPatients.Update(hchbPatient);
                }

                HchbWebDbContext.SaveChanges();
                if (logId > 0)
                {
                    LogProcessMessage(logId);
                }
                return Messages.ADMIT_SUCCESS;
            }
            SlackMessage adtNotification = adtMessage.ToNotification();
            await SendNotificationAsync("There is no matched Patient for admit request.", adtNotification);

            return Messages.PATIENT_NONMATCH;
        }

        private async Task<string> ProcessA03Message(
            Adt adtMessage, 
            PatientMatchingResponse response, 
            int logId, 
            HchbPatientWeb hchbPatient
        )
        {
            if (response.MatchLevel == MatchLevel.Match)
            {
                if (hchbPatient == null)
                {
                    adtMessage.HchbPatient.PatientId = response.TopMatch.PatientId;
                    adtMessage.HchbPatient.Status = Status.DISCHARGED;
                    HchbWebDbContext.HchbPatients.Add(adtMessage.HchbPatient);
                }
                else
                {
                    hchbPatient.Status = Status.DISCHARGED;
                    HchbWebDbContext.HchbPatients.Update(hchbPatient);
                }                
                HchbWebDbContext.SaveChanges();
                if (logId > 0)
                {
                    LogProcessMessage(logId);
                }
                return Messages.DISCHARGE_SUCCESS;
            }
            
            SlackMessage adtNotification = adtMessage.ToNotification();
            await SendNotificationAsync("There is no matched Patient for discharge request.", adtNotification);
            return Messages.PATIENT_NONMATCH;
        }

        private async Task<string> ProcessA04Message(
            Adt adtMessage, 
            PatientMatchingResponse response, 
            HchbPatientWeb existingHchbPatient, 
            int facilityId, 
            int logId
        )
        {
            switch (response.MatchLevel)
            {
                case MatchLevel.NonMatch:
                case MatchLevel.Similar:
                    Patient patient;
                    try
                    {
                        patient = await PatientServices.CreateAsync(adtMessage.Patient, facilityId, 0);
                        adtMessage.HchbPatient.PatientId = patient.PatientId;
                        adtMessage.HchbPatient.Status = Status.PENDDING;
                        HchbWebDbContext.HchbPatients.Add(adtMessage.HchbPatient);
                        if (logId > 0)
                        {
                            LogProcessMessage(logId);
                            await ProcessMessageLog(adtMessage.HchbPatient.HchbPatientId, adtMessage.HchbPatient.EpisodeId);
                        }
                        HchbWebDbContext.SaveChanges();
                        return Messages.CREATE_SUCCESS;
                    }
                    catch (Exception ex)
                    {
                        await SendNotificationAsync($"Unable to create Patient\n{ex.Message}", 
                            adtMessage.ToNotification());
                        return Messages.ADT_ERROR;
                    }

                case MatchLevel.Match:
                    var matchPatient = response.TopMatch;

                    adtMessage.Patient.PatientId = matchPatient.PatientId;
                    adtMessage.HchbPatient.PatientId = matchPatient.PatientId;

                    if (existingHchbPatient != null)
                    {
                        if (adtMessage.HchbPatient.IcdCode != null && 
                            existingHchbPatient.IcdCode != adtMessage.HchbPatient.IcdCode)
                        {
                            existingHchbPatient.IcdCode = adtMessage.HchbPatient.IcdCode;
                        }
                        HchbWebDbContext.HchbPatients.Update(existingHchbPatient);
                    }
                    else
                    {
                        adtMessage.HchbPatient.Status = Status.PENDDING;
                        HchbWebDbContext.HchbPatients.Add(adtMessage.HchbPatient);
                    }
                    HchbWebDbContext.SaveChanges();
                    
                    if (string.IsNullOrWhiteSpace(adtMessage.Patient.MiddleName) &&
                        !string.IsNullOrWhiteSpace(matchPatient.MiddleName))
                    {
                        adtMessage.Patient.MiddleName = matchPatient.MiddleName;
                    }

                    if (string.IsNullOrWhiteSpace(adtMessage.Patient.Suffix) && !string.IsNullOrWhiteSpace(matchPatient.Suffix))
                    {
                        adtMessage.Patient.Suffix = matchPatient.Suffix;
                    }

                    var newPatientHasFullSocial = adtMessage.Patient.HasFullSocial();
                    var matchPatientHasFullSocial = matchPatient.HasFullSocial();

                    var newPatientHasLast4Social = adtMessage.Patient.HasLast4Social();
                    var matchPatientHasLast4Social = matchPatient.HasLast4Social();

                    var newIdentifiers = new List<Patients.PatientIdentifier>();

                    void AddIdentifier(Patients.Patient p, string t) =>
                        newIdentifiers.AddRange(p.Identifiers.Where(identifier => identifier.Type.Equals(t)));

                    if (newPatientHasFullSocial)
                    {
                        AddIdentifier(adtMessage.Patient, KnownTypes.SocialSecurityNumber);
                    }
                    else if (matchPatientHasFullSocial)
                    {
                        AddIdentifier(matchPatient, KnownTypes.SocialSecurityNumber);
                    }
                    else if (newPatientHasLast4Social)
                    {
                        AddIdentifier(adtMessage.Patient, KnownTypes.SocialSecuritySerial);
                    }
                    else if (matchPatientHasLast4Social)
                    {
                        AddIdentifier(matchPatient, KnownTypes.SocialSecuritySerial);
                    }

                    newIdentifiers.AddRange(adtMessage.Patient.Identifiers.Where(identifier =>
                        identifier.Type is not (KnownTypes.SocialSecurityNumber or KnownTypes.SocialSecuritySerial)));
                    newIdentifiers.AddRange(matchPatient.Identifiers.Where(identifier =>
                        identifier.Type is not (KnownTypes.SocialSecurityNumber or KnownTypes.SocialSecuritySerial)));

                    adtMessage.Patient.Identifiers = newIdentifiers;

                    await PatientServices.UpdateAsync(adtMessage.Patient, facilityId, 0);
                    if (logId > 0)
                    {
                        LogProcessMessage(logId);
                        await ProcessMessageLog(adtMessage.HchbPatient.HchbPatientId, adtMessage.HchbPatient.EpisodeId);
                    }
                    return Messages.UPDATE_SUCCESS;

                default:

                    string branchName = GetFacilityNameFromBranchCode(adtMessage.BranchCode);
                    SlackMessage adtNotification = adtMessage.ToPatientMatchNotification(branchName);
                    await SendSimilarHighRiskPatientMatchNotificationAsync(
                        "A similar Patient with the same information already exists when attempting to create a new Patient.",
                        adtNotification
                    );
                    return Messages.PATIENT_EXIST;
            }
        }

        private async Task<string> ProcessA11Message(
            Adt adtMessage, 
            PatientMatchingResponse response, 
            int logId, 
            HchbPatientWeb hchbPatient
        )
        {
            if (response.MatchLevel == MatchLevel.Match)
            {
                if (hchbPatient == null)
                {
                    adtMessage.HchbPatient.PatientId = response.TopMatch.PatientId;
                    adtMessage.HchbPatient.Status = Status.NON_ADMIT;
                    HchbWebDbContext.HchbPatients.Add(adtMessage.HchbPatient);
                }
                else
                {
                    hchbPatient.Status = Status.NON_ADMIT;
                    HchbWebDbContext.HchbPatients.Update(hchbPatient);
                }
                        
                if (logId > 0)
                {
                    LogProcessMessage(logId);
                }
                return Messages.CANCLE_SUCCESS;
            }

            SlackMessage adtNotification = adtMessage.ToNotification();
            await SendNotificationAsync("There is no matched Patient for cancel admission request.", adtNotification);

            return Messages.PATIENT_NONMATCH;
        }

        private static async Task<byte[]> GetObjectBytesAsync(string BucketName, string objectKey)
        {
            using var client = new AmazonS3Client();
            var request = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = objectKey
            };
            using var response = await client.GetObjectAsync(request);
            using var responseStream = response.ResponseStream;
            using var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public int LogMessage(string type, string message)
        {
            HL7MessageLog messagelog = new (message, type);
         
            var log = HchbWebDbContext.Logs.SingleOrDefault(l => l.MessageControlId == messagelog.MessageControlId);            
            if (log == null)
            {
                var result = HchbWebDbContext.Logs.Add(messagelog);
                HchbWebDbContext.SaveChanges();
                return result.Entity.Id;
            }

            if (log.HchbPatientId != messagelog.HchbPatientId)
                log.HchbPatientId = messagelog.HchbPatientId;
            if (log.EpisodeId != messagelog.EpisodeId)
                log.EpisodeId = messagelog.EpisodeId;
            HchbWebDbContext.SaveChanges();
            return log.Id;
        }

        public void LogReason(int id, string reason)
        {
            var log = HchbWebDbContext.Logs.SingleOrDefault(l => l.Id == id)
                ?? throw new DoesNotExistException("LogReason - Log: " + id + " is not exist.");
            log.Reason = reason.Substring(0, 1000);
            HchbWebDbContext.SaveChanges();
        }

        public bool? IsProcessedMessage(int id)
        {
            var log = HchbWebDbContext.Logs.SingleOrDefault(l => l.Id == id)
                ?? throw new DoesNotExistException("IsProcessedMessage - Log: " + id + " is not exist.");
            return log.IsProcessed;
        }

        public void LogProcessMessage(int id)
        {
            var log = HchbWebDbContext.Logs.SingleOrDefault(l => l.Id == id)
                ?? throw new DoesNotExistException("LogProcessMessage - Log: " + id + " is not exist.");
            log.IsProcessed = true;
            log.ProcessedDate = DateTime.Now;
            log.Reason = "Send document successfully.";
            HchbWebDbContext.SaveChanges();
        }

        public async Task ProcessMessageLog(string hchbId, string episodeId)
        {
            var messages = HchbWebDbContext.Logs.Where((message) =>
                message.SubType != "A04" && message.SubType != "A08" && message.HchbPatientId == hchbId
                && message.EpisodeId == episodeId && message.IsProcessed == false);
            foreach (var message in messages)
            {
                await KinesisService.PutJsonToStreamAsync(DataStream, message.Type, message.Message);
            }
        }

        public Task<bool> SendNotificationAsync(string error, SlackMessage message)
        {                        
            message.Channel = SlackChannel;
            
            var section = new Section
            {
                Text = new TextObject
                {
                    Text = $":point_right: *Details*: _{error}_",
                    Type = TextObject.TextType.Markdown

                }
            };

            message.Blocks.Insert(1, section);
            
            return SlackClient.PostAsync(message);
        }

        public Task<bool> SendNotificationAsync(string error, string message)
        {
            var section = new Section
            {
                Text = new TextObject
                {
                    Text = $":point_right: *Details*: _{error}_",
                    Type = TextObject.TextType.Markdown

                }
            };

            SlackMessage _message = new()
            {
                Channel = SlackChannel,
                Blocks = new List<Block>() {section },
                Attachments= new List<SlackAttachment> 
                {  
                    new SlackAttachment() 
                    { 
                        Text = message
                    } 
                }
            };

            return SlackClient.PostAsync(_message);
        }

        public Task<bool> SendSimilarHighRiskPatientMatchNotificationAsync(string error, SlackMessage message)
        {
            message.Channel = AlertPatientMatchSlackChannel;

            var section = new Section
            {
                Text = new TextObject
                {
                    Text = $":point_right: *Details*: _{error}_",
                    Type = TextObject.TextType.Markdown

                }
            };

            message.Blocks.Insert(1, section);

            return SlackAlertPatientMatchClient.PostAsync(message);
        }

        public Task<bool> SendSimilarHighRiskPatientMatchNotificationAsync(string error, string message)
        {
            var section = new Section
            {
                Text = new TextObject
                {
                    Text = $":point_right: *Details*: _{error}_",
                    Type = TextObject.TextType.Markdown

                }
            };

            SlackMessage _message = new()
            {
                Channel = AlertPatientMatchSlackChannel,
                Blocks = new List<Block>() { section },
                Attachments = new List<SlackAttachment>
                {
                    new SlackAttachment()
                    {
                        Text = message
                    }
                }
            };

            return SlackAlertPatientMatchClient.PostAsync(_message);
        }

        public void CreateHchbPatient(int patientId, string messageControlId)
        {
            var log = HchbWebDbContext.Logs.SingleOrDefault(l => l.MessageControlId == messageControlId)
                ?? throw new DoesNotExistException("CreateHchbPatient - Log: " + messageControlId + " is not exist");
            var newPatient = new HchbPatientWeb()
            {
                HchbPatientId = log.HchbPatientId,
                EpisodeId = log.EpisodeId,
                PatientId = patientId,
                Status = log.Status,
                IcdCode = log.ICDCode
            };
            HchbWebDbContext.HchbPatients.Add(newPatient);
            HchbWebDbContext.SaveChanges();
        }

        public int GetFacilityIdFromBranchCode(string code)
        {
            var branch = HchbWebDbContext.Branches.SingleOrDefault(b => b.BranchCode == code)
                ?? throw new DoesNotExistException("GetFacilityIdFromBranchCode - Branch: " + code + " is not exist");
            return branch.FacilityId;
        }

        public string GetFacilityNameFromBranchCode(string code)
        {
            var branch = HchbWebDbContext.Branches.SingleOrDefault(b => b.BranchCode == code)
                ?? throw new DoesNotExistException("GetFacilityIdFromBranchCode - Branch: " + code + " is not exist");
            return branch.BranchName;
        }
                

        public HchbTemplate GetHchbTemplate(string admissionType, string observationId, string observationText, string patientType)
        {
            return HchbWebDbContext.Templates.SingleOrDefault(t =>
                t.AdmissionType == admissionType && t.ObservationId == observationId
                && t.ObservationText == observationText && t.PatientType == patientType);
        }

        public int GetSignerPrimaryFacility(int userId)
        {
            var facility = HchbWebDbContext.UserFacilities.SingleOrDefault(f => f.UserId == userId && f.Primary && f.Active)
                ?? throw new DoesNotExistException("GetSignerPrimaryFacility - Primary facility of User: "
                + userId + " is not exist");
            return facility.FacilityId;
        }
    }
}
