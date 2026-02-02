using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kendo.Mvc.UI;
using SutureHealth.Linq;
using SutureHealth.AspNetCore.Areas.Admin.Models.Review;
using SutureHealth.Requests.Services;
using SutureHealth.Application.Services;
using SutureHealth.Patients.Services;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;
using Kendo.Mvc.Extensions;
using SutureHealth.Requests;
using SutureHealth.Patients;
using Microsoft.EntityFrameworkCore;
using SutureHealth.AspNetCore.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SutureHealth.AspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Review")]
    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    public class ReviewController : Controller
    {
        protected IRequestServicesProvider RequestService { get; }
        protected IApplicationService SecurityService { get; }
        protected IPatientServicesProvider PatientService { get; }

        public ReviewController
        (
            IRequestServicesProvider requestService,
            IApplicationService securityService,
            IPatientServicesProvider patientService
        )
        {
            RequestService = requestService;
            SecurityService = securityService;
            PatientService = patientService;
        }

        [HttpGet]
        [Route("", Name = "AdminReviewIndex")]
        public IActionResult Index()
        {
            bool showDialog = TempData.ContainsKey("ShowDialog") && (bool)TempData["ShowDialog"];
            TempData.Remove("ShowDialog");
            return View(new IndexModel()
            {
                CurrentUser = CurrentUser,
                ShowDialog = showDialog,
            });
        }

        [HttpGet]
        [Route("Requests", Name = "AdminReviewRequestDataSource")]
        public async Task<IActionResult> RequestDataSource([DataSourceRequest] DataSourceRequest request)
        {
            var query = PatientService.GetAllMatchLogs()
                                      .AsNoTracking()
                                      .Where(m => (m.NeedsReview == true &&
                                                   m.ManuallyMatched == false &&
                                                   m.MatchedPatient == false)
                                                   ||
                                                   (m.NeedsReview == true &&
                                                     m.ManuallyMatched == false &&
                                                     m.MatchedPatient == true))
                                      .Include(m => m.Organization);

            var list = query.ToList().Distinct(new MatchLogEqualityComparer());

            return Json(await list.ToDataSourceResultAsync(request,
                       l =>
                       {
                           var raw = new ReviewGridListItem()
                           {
                               DateSubmitted = l.CreateDate,
                               FirstName = l.FirstName,
                               LastName = l.LastName,
                               AddressStreetLine1 = l.Address1,
                               AddressCity = l.City,
                               AddressState = l.State,
                               AddressZipCode = l.Zip,
                               Birthdate = l.Birthdate,
                               SocialSecurityNumber = l.SocialSecurityNumber.ToFormmatedSSNForGrid() ?? l.SocialSecuritySerial,
                               MedicareNumber = l.MedicareNumber,
                               SubmittedFacilityName = l.Organization.Name,
                               PatientMatchUrl = Url.RouteUrl("AdminReviewResolveMatch", new { requestId = l.MatchPatientLogID }),
                               IsPatientMatched = l.MatchedPatient.Value,
                               MatchPatientLogID = l.MatchPatientLogID,
                               Source = l.Source,
                           };
                           if (l.MedicaidNumber != null)
                           {
                               raw.MedicaidNumber = l.MedicaidNumber + " (" + l.MedicaidState + ")";
                           }
                           return raw;
                       }
                ));

        }

        [HttpGet]
        [Route("Request/{requestId:long}", Name = "AdminReviewResolveMatch")]
        public async Task<IActionResult> ResolveMatch(long requestId)
        {
            var log = await PatientService.GetMatchLogByIdAsync((int)requestId);

            var matchRequest = log.ToPatientMatchingRequest();

            matchRequest.ManualReviewEnabled = false;
            var potentialMatches = await PatientService.MatchAsync(matchRequest);
            foreach (var result in potentialMatches.MatchResults)
            {
                Patients.Patient patient = result.Match;
                var completePatientInfo = await PatientService.GetByIdAsync(patient.PatientId);
                result.Match = completePatientInfo;
            }


            var sendingUser = await SecurityService.GetMemberByIdAsync(matchRequest.MemberId);
            var organization = await SecurityService.GetOrganizationByIdAsync(log.FacilityId);

            bool isAutoCreate = IsAutoCreateRequest(potentialMatches);
            bool isAutoMerge = IsAutoMergeRequest(potentialMatches);
            isAutoMerge = true;
            if (isAutoMerge || isAutoCreate)
            {
                TempData["ShowDialog"] = true;
                //await AutoResolve(log, potentialMatches, organization,(int)requestId,isAutoMerge,isAutoCreate);
                return RedirectToAction("Index");
            }

            string ssnOrssn4 = log.SocialSecurityNumber ?? log.SocialSecuritySerial;

            List<PatientMatchModel.PatientInfo> patientInfoList =
            ConvertPotentialMatchesToPatientInfoList(potentialMatches, log, organization);

            return View(new PatientMatchModel()
            {
                SubmittedDate = log.CreateDate.ToLocalTime().ToString("g"),
                Patient = new PatientMatchModel.PatientInfo()
                {
                    FirstName = log.FirstName,
                    MiddleName = log.MiddleName.IsNullOrWhiteSpace() ? "" : log.MiddleName,
                    LastName = log.LastName,
                    Suffix = log.Suffix,
                    DOB = log.Birthdate,

                    HomePhone = new Patients.PatientPhone() { Value = log.HomePhone, IsPrimary = log.PrimaryPhone == ContactType.HomePhone.ToString() },
                    WorkPhone = new Patients.PatientPhone() { Value = log.WorkPhone, IsPrimary = log.PrimaryPhone == ContactType.WorkPhone.ToString() },
                    Mobile = new Patients.PatientPhone() { Value = log.Mobile, IsPrimary = log.PrimaryPhone == ContactType.Mobile.ToString() },
                    OtherPhone = new Patients.PatientPhone() { Value = log.OtherPhone, IsPrimary = log.PrimaryPhone == ContactType.OtherPhone.ToString() },

                    Address = new Requests.PatientAddress()
                    {
                        City = log.City,
                        StateOrProvince = log.State,
                        Line1 = log.Address1,
                        Line2 = log.Address2,
                        PostalCode = log.Zip,
                        ParentId = organization.ParentId.HasValue == true ? (long)organization.ParentId.Value : 0
                    },
                    Gender = log.Gender,
                    SSN = ssnOrssn4,
                    MedicareMBI = log.MedicareNumber,
                    MedicaidNumber = log.MedicaidNumber,
                    MedicaidState = log.MedicaidState,
                    FacilityMRN = log.FacilityMRN,
                    OrganizationId = organization.OrganizationId,
                    OrganizatioName = organization.Name,
                    IsSelfPaid = log.IsSelfPay.HasValue ? log.IsSelfPay.Value : false,
                    IsPrivateInsuranceAvailable = log.IsPrivateInsurance.HasValue ? log.IsPrivateInsurance.Value : false,

                },
                SendingOrg = new PatientMatchModel.SendingOrganizationInfo()
                {
                    Name = organization.Name,
                    Phone = organization.Contacts.Where(x => x.Type == ContactType.Phone).FirstOrDefault()?.Value?.ToFormattedPhoneNumber()
                },
                SendingMemberName = sendingUser?.ShortName,
                PatientMatches = patientInfoList.OrderByDescending(m => m.MatchScore).ToList(),
                RequestId = requestId,
                CurrentUser = CurrentUser,
                Source = log.Source,
                IsAutoMerge = isAutoMerge,
                IsAutoCreate = isAutoCreate
            });
        }


        [HttpPost]
        [Route("Request/{requestId:long}/AssociateMerge", Name = "AdminReviewAssociateMerge")]
        public async Task<IActionResult> AssociateMerge([FromRoute] long requestId, [FromBody] AssociateMergeRequest mergeOptions)
        {
            var log = await PatientService.GetMatchLogByIdAsync((int)requestId);
            Application.Organization organization = await SecurityService.GetOrganizationByIdAsync(log.FacilityId);
            var patient = CreatePatientFromLogAndMergeOptions(log, mergeOptions);

            try
            {
                await UpdatePatient(patient, organization, mergeOptions, (int)requestId);
            }
            catch (Exception)
            {

                throw;
            }


            return Json(new { success = true });
        }

        [HttpPost]
        [Route("Request/{requestId:long}/CreatePatient", Name = "AdminReviewCreatePatient")]
        public async Task<IActionResult> CreatePatient([FromRoute] long requestId)
        {
            var matchlogPatient = await PatientService.GetMatchLogByIdAsync((int)requestId);
            List<Patients.PatientAddress> patientAddresses = new List<Patients.PatientAddress>();
            patientAddresses.Add(
                new Patients.PatientAddress()
                {
                    City = matchlogPatient.City ?? string.Empty,
                    Line1 = matchlogPatient.Address1 ?? string.Empty,
                    Line2 = matchlogPatient.Address2 ?? string.Empty,
                    PostalCode = matchlogPatient.Zip,
                    StateOrProvince = matchlogPatient.State ?? string.Empty
                });

            var patientIdentifier = new List<Patients.PatientIdentifier>();
            if (!matchlogPatient.SocialSecurityNumber.IsNullOrWhiteSpace())
            {
                patientIdentifier.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.SocialSecurityNumber,
                    Value = matchlogPatient.SocialSecurityNumber
                });
                patientIdentifier.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.SocialSecuritySerial,
                    Value = matchlogPatient.SocialSecurityNumber.GetLast(4)
                });
            }
            else if (!matchlogPatient.SocialSecuritySerial.IsNullOrWhiteSpace())
            {
                patientIdentifier.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.SocialSecuritySerial,
                    Value = matchlogPatient.SocialSecuritySerial
                });
            }

            if (!matchlogPatient.MedicareNumber.IsNullOrWhiteSpace())
            {
                patientIdentifier.Add(KnownTypes.Medicare, string.Empty);
                patientIdentifier.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.MedicareBeneficiaryNumber,
                    Value = matchlogPatient.MedicareNumber
                });
            }

            if (matchlogPatient.IsSelfPay == true)
            {
                patientIdentifier.Add(KnownTypes.SelfPay, string.Empty);
            }

            if (matchlogPatient.IsMedicareAdvantage == true)
            {
                patientIdentifier.Add(KnownTypes.MedicareAdvantage, string.Empty);
            }
            if (matchlogPatient.IsPrivateInsurance == true)
            {
                patientIdentifier.Add(KnownTypes.PrivateInsurance, string.Empty);
            }

            if (!matchlogPatient.MedicaidNumber.IsNullOrWhiteSpace() && !matchlogPatient.MedicaidState.IsNullOrWhiteSpace())
            {
                patientIdentifier.Add(KnownTypes.Medicaid, string.Empty);
                patientIdentifier.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.MedicaidState,
                    Value = matchlogPatient.MedicaidState
                });
                patientIdentifier.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.MedicaidNumber,
                    Value = matchlogPatient.MedicaidNumber
                });
            }

            if (!matchlogPatient.FacilityMRN.IsNullOrWhiteSpace())
            {
                patientIdentifier.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.UniqueExternalIdentifier,
                    Value = matchlogPatient.FacilityMRN
                });
            }

            List<Patients.PatientPhone> phones = new();
            if (!matchlogPatient.HomePhone.IsNullOrEmpty())
            {
                phones.Add(new Patients.PatientPhone()
                {
                    Type = ContactType.HomePhone,
                    Value = matchlogPatient.HomePhone,
                    IsActive = true,
                    IsPrimary = matchlogPatient.PrimaryPhone.EqualsIgnoreCase(ContactType.HomePhone.ToString())
                });
            }
            if (!matchlogPatient.WorkPhone.IsNullOrEmpty())
            {
                phones.Add(new Patients.PatientPhone()
                {
                    Type = ContactType.WorkPhone,
                    Value = matchlogPatient.WorkPhone,
                    IsActive = true,
                    IsPrimary = matchlogPatient.PrimaryPhone.EqualsIgnoreCase(ContactType.WorkPhone.ToString())
                });
            }
            if (!matchlogPatient.Mobile.IsNullOrEmpty())
            {
                phones.Add(new Patients.PatientPhone()
                {
                    Type = ContactType.Mobile,
                    Value = matchlogPatient.Mobile,
                    IsActive = true,
                    IsPrimary = matchlogPatient.PrimaryPhone.EqualsIgnoreCase(ContactType.Mobile.ToString())
                });
            }
            if (!matchlogPatient.OtherPhone.IsNullOrEmpty())
            {
                phones.Add(new Patients.PatientPhone()
                {
                    Type = ContactType.OtherPhone,
                    Value = matchlogPatient.OtherPhone,
                    IsActive = true,
                    IsPrimary = matchlogPatient.PrimaryPhone.EqualsIgnoreCase(ContactType.OtherPhone.ToString())
                });
            }

            Patients.Patient patient = new()
            {
                FirstName = matchlogPatient.FirstName,
                LastName = matchlogPatient.LastName,
                MiddleName = matchlogPatient.MiddleName ?? string.Empty,
                Suffix = matchlogPatient.Suffix ?? string.Empty,
                Gender = matchlogPatient.Gender,
                Birthdate = matchlogPatient.Birthdate,
                Phones = phones,
                Addresses = patientAddresses,
                Identifiers = patientIdentifier
            };

            try
            {
                await PatientService.CreateAsync(patient, matchlogPatient.FacilityId, matchlogPatient.MemberId);
                await PatientService.SetPatientMatchLogFlagsToResolved((int)requestId, CurrentUser.Id);

            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = e.InnerException.Message });

            }

            return Json(new { success = true });

        }

        [HttpPost]
        [Route("Request/DeleteMatch", Name = "AdminReviewDeleteMatch")]
        public async Task<IActionResult> DeleteMatch([FromBody] DeleteMatchRequestItem item)
        {
            await PatientService.TryDisableNeedReviewForMatchLogInstance(item.MatchPatientLogId);
            return Json(new { success = true });
        }

        [HttpGet]
        [Route("Request/{fileName}/DownloadFile", Name = "AdminReviewDownloadFile")]
        public async Task<IActionResult> DownloadFile([FromRoute] string fileName)
        {
            int lastUnderscoreIndex = fileName.LastIndexOf('_');
            int lastDotIndex = fileName.LastIndexOf('.');
            if ((lastUnderscoreIndex != -1) && (lastDotIndex != -1))
            {
                string requestIdString = fileName.Substring(lastUnderscoreIndex + 1, lastDotIndex - lastUnderscoreIndex - 1);
                int requestId;
                if (int.TryParse(requestIdString, out requestId))
                {
                    var fileContent = await RequestService.GetServiceableRequestPdfByIdAsync(new[] { requestId });

                    return File(fileContent[requestId],
                                @"application/octet-stream",
                                $"{fileName}" + ".pdf");

                }
            }

            return NotFound();
        }


        private bool IsAutoCreateRequest(PatientMatchingResponse matchingResponse)
        {
            bool result = false;
            switch (matchingResponse.MatchLevel)
            {
                case MatchLevel.NonMatch:
                    result = true;
                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }
        private bool IsAutoMergeRequest(PatientMatchingResponse matchingResponse)
        {
            if (matchingResponse.MatchLevel == MatchLevel.Match && matchingResponse.MatchResults?.Count() == 1)
            {
                return true;
            }
            return false;
        }

        private List<PatientMatchModel.PatientInfo> ConvertPotentialMatchesToPatientInfoList(PatientMatchingResponse potentialMatches, MatchLog log, Application.Organization organization)
        {
            List<PatientMatchModel.PatientInfo> patientInfoList = new List<PatientMatchModel.PatientInfo>();

            foreach (var outcome in potentialMatches.MatchResults)
            {
                var patient = outcome.Match;

                //organizations.Values
                string patientOrganizationName = string.Empty;

                var patientFacilities = (from ok in patient.OrganizationKeys
                                         join o in SecurityService.GetOrganizations() on ok.OrganizationId equals o.OrganizationId
                                         select new { Text = o?.Name + " \r\n(" + ok?.MedicalRecordNumber + " , " + ok?.CreatedAt?.ToString("d") + ") ", Value = o?.Name }).ToList();

                int matchScorePercentage = NormalizeMatchScore(outcome.Score);

                var patientInfo = new PatientMatchModel.PatientInfo()
                {

                    FirstName = patient.FirstName,
                    IsFirstNameMatch = (bool)patient.FirstName?.Trim().EqualsIgnoreCase(log.FirstName?.Trim()) ? true : false,
                    LastName = patient.LastName,
                    IsLastNameMatch = (bool)patient.LastName?.Trim().EqualsIgnoreCase(log.LastName?.Trim()) ? true : false,
                    MiddleName = patient.MiddleName,
                    IsMiddleNameMatch = !string.IsNullOrEmpty(patient.MiddleName) &&
                                         (bool)patient.MiddleName?.Trim().EqualsIgnoreCase(log.MiddleName?.Trim()) ||
                                          (bool)log.MiddleName.IsNullOrEmpty() &&
                                          (bool)patient.MiddleName.IsNullOrEmpty() ? true : false,
                    Suffix = patient.Suffix,
                    IsSuffixMatch = !string.IsNullOrEmpty(patient.Suffix) &&
                                    (bool)patient.Suffix?.Trim().EqualsIgnoreCase(log.Suffix?.Trim()) ||
                                    (bool)log.Suffix.IsNullOrEmpty() && (bool)patient.Suffix.IsNullOrEmpty() ? true : false,


                    DOB = patient.Birthdate,
                    IsDOBMatch = patient.Birthdate.ToString("d") == log.Birthdate.ToString("d") ? true : false,
                    Gender = patient.Gender,

                    HomePhone = patient.Phones.Where(p => p.Type == ContactType.HomePhone).FirstOrDefault(),
                    WorkPhone = patient.Phones.Where(p => p.Type == ContactType.WorkPhone).FirstOrDefault(),
                    Mobile = patient.Phones.Where(p => p.Type == ContactType.Mobile).FirstOrDefault(),
                    OtherPhone = patient.Phones.Where(p => p.Type == ContactType.OtherPhone).FirstOrDefault(),

                    IsGenderMatch = patient.Gender == log.Gender ? true : false,
                    PatientId = patient.PatientId,
                    Address = new Requests.PatientAddress()
                    {
                        City = patient.Addresses.FirstOrDefault()?.City,
                        CountryOrRegion = patient.Addresses.FirstOrDefault()?.CountryOrRegion,
                        County = patient.Addresses.FirstOrDefault()?.County,
                        Line1 = patient.Addresses.FirstOrDefault()?.Line1,
                        Line2 = patient.Addresses.FirstOrDefault()?.Line2,
                        PostalCode = patient.Addresses.FirstOrDefault()?.PostalCode,
                        ParentId = (patient.Addresses.FirstOrDefault() == null ? 0 : patient.Addresses.FirstOrDefault().ParentId),
                        Id = (patient.Addresses.FirstOrDefault() == null ? 0 : (long)patient.Addresses.FirstOrDefault().Id),
                        StateOrProvince = patient.Addresses.FirstOrDefault()?.StateOrProvince
                    },
                    IsCityMatch = (!string.IsNullOrEmpty(patient.Addresses.FirstOrDefault()?.City) &&
                                    (bool)patient.Addresses.FirstOrDefault()?.City?.EqualsIgnoreCase(log.City)) ||
                                    (string.IsNullOrEmpty(patient.Addresses.FirstOrDefault()?.City) &&
                                     string.IsNullOrEmpty(log.City)),

                    IsStateMatch = (!string.IsNullOrEmpty(patient.Addresses.FirstOrDefault()?.StateOrProvince) &&
                                     (bool)patient.Addresses.FirstOrDefault()?.StateOrProvince?.EqualsIgnoreCase(log.State)) ||
                                     (string.IsNullOrEmpty(patient.Addresses.FirstOrDefault()?.StateOrProvince) &&
                                      string.IsNullOrEmpty(log.State)),

                    IsLine1Match = (!string.IsNullOrEmpty(patient.Addresses.FirstOrDefault()?.Line1) &&
                                     (bool)patient.Addresses.FirstOrDefault()?.Line1?.EqualsIgnoreCase(log.Address1)) ||
                                     (string.IsNullOrEmpty(patient.Addresses.FirstOrDefault()?.Line1) &&
                                      string.IsNullOrEmpty(log.Address1)),

                    IsLine2Match = (!string.IsNullOrEmpty(patient.Addresses.FirstOrDefault()?.Line2) &&
                                     (bool)patient.Addresses.FirstOrDefault()?.Line2?.EqualsIgnoreCase(log.Address2)) ||
                                     (string.IsNullOrEmpty(patient.Addresses.FirstOrDefault()?.Line2) &&
                                     string.IsNullOrEmpty(log.Address2)),

                    IsPostalCodeMatch = patient.Addresses.FirstOrDefault()?.PostalCode == log.Zip ? true : false,

                    MatchIndex = potentialMatches.MatchResults.IndexOf(outcome),
                    MatchScore = (int)outcome.Score,
                    MatchScorePercentage = matchScorePercentage,

                    Facilities = new SelectList(from f in patientFacilities
                                                select new SelectListItem() { Text = f.Text, Value = f.Value }, "Value", "Text"),

                    MedicareMBI = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicareBeneficiaryNumber) && !string.IsNullOrWhiteSpace(i.Value))
                                                   .Select(i => i.Value).FirstOrDefault(),
                    IsMedicareMBIMatch = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicareBeneficiaryNumber) && !string.IsNullOrWhiteSpace(i.Value))
                                                   .Select(i => i.Value).FirstOrDefault() == log.MedicareNumber ||
                              patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicareBeneficiaryNumber)).IsNullOrEmpty() && log.MedicareNumber.IsNullOrEmpty() ? true : false,


                    MedicaidNumber = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicaidNumber) && !string.IsNullOrWhiteSpace(i.Value))
                                                       .Select(i => i.Value).FirstOrDefault(),

                    IsMedicaidNumberMatch = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicaidNumber) && !string.IsNullOrWhiteSpace(i.Value))
                                                       .Select(i => i.Value).FirstOrDefault() == log.MedicaidNumber ||
                                   patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicaidNumber)).IsNullOrEmpty() && log.MedicaidNumber.IsNullOrEmpty() ? true : false,

                    MedicaidState = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicaidState) && !string.IsNullOrEmpty(i.Value))
                                                       .Select(i => i.Value).FirstOrDefault(),


                    FacilityMRN = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.UniqueExternalIdentifier) && !string.IsNullOrEmpty(i.Value))
                                                     .Select(i => i.Value).FirstOrDefault(),

                    IsHomePhoneMatch = IsPhoneMatch(patient.Phones, ContactType.HomePhone, log.HomePhone, log.PrimaryPhone),
                    IsWorkPhoneMatch = IsPhoneMatch(patient.Phones, ContactType.WorkPhone, log.WorkPhone, log.PrimaryPhone),
                    IsMobileMatch = IsPhoneMatch(patient.Phones, ContactType.Mobile, log.Mobile, log.PrimaryPhone),
                    IsOtherPhoneMatch = IsPhoneMatch(patient.Phones, ContactType.OtherPhone, log.OtherPhone, log.PrimaryPhone),


                };
                var _selfPayIdentifier = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SelfPay) && !string.IsNullOrEmpty(i.Value)).Select(i => i.Value).FirstOrDefault();
                if (_selfPayIdentifier != null)
                {
                    patientInfo.IsSelfPaid = _selfPayIdentifier.EqualsIgnoreCase("true") ? true : false;
                    patientInfo.IsSelfPaidMatch = patientInfo.IsSelfPaid == log.IsSelfPay;
                }
                else
                {
                    // patient does not have SelfPay Identifier
                    patientInfo.IsSelfPaidMatch = log.IsSelfPay == null || log.IsSelfPay == false;
                }

                var _privateInsuranceIdentifier = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.PrivateInsurance) && !string.IsNullOrEmpty(i.Value)).Select(i => i.Value).FirstOrDefault();
                if (_privateInsuranceIdentifier != null)
                {
                    patientInfo.IsPrivateInsuranceAvailable = _privateInsuranceIdentifier.EqualsIgnoreCase("true") ? true : false;
                    patientInfo.IsPrivateInsuranceMatch = patientInfo.IsPrivateInsuranceAvailable == log.IsPrivateInsurance;
                }
                else
                {
                    // patient does not have privateInsurance
                    patientInfo.IsPrivateInsuranceMatch = log.IsPrivateInsurance == null || log.IsPrivateInsurance == false;

                }


                // match outcome and matchlog have ssn
                if (patient.Identifiers.HasFullSocial() && !string.IsNullOrEmpty(log.SocialSecurityNumber))
                {
                    patientInfo.SSN = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber) && !string.IsNullOrWhiteSpace(i.Value))
                                            .Select(i => i.Value).FirstOrDefault();
                    patientInfo.IsSSNMatch = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber) && !string.IsNullOrWhiteSpace(i.Value))
                                            .Select(i => i.Value).FirstOrDefault() == log.SocialSecurityNumber ||
                                            patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber)).IsNullOrEmpty() && log.SocialSecurityNumber.IsNullOrEmpty() ? true : false;
                }
                // match outcome has ssn but matchlog has only ssn4 
                else if (patient.Identifiers.HasFullSocial() && string.IsNullOrEmpty(log.SocialSecurityNumber) && !string.IsNullOrEmpty(log.SocialSecuritySerial))
                {
                    patientInfo.SSN = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber) && !string.IsNullOrWhiteSpace(i.Value))
                                            .Select(i => i.Value).FirstOrDefault();
                    patientInfo.IsSSNMatch = false;
                }
                // match outcome has ssn but matchlog is null
                else if (patient.Identifiers.HasFullSocial() && string.IsNullOrEmpty(log.SocialSecurityNumber) && string.IsNullOrEmpty(log.SocialSecuritySerial))
                {
                    patientInfo.SSN = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber) && !string.IsNullOrWhiteSpace(i.Value))
                                            .Select(i => i.Value).FirstOrDefault();
                    patientInfo.IsSSNMatch = false;
                }
                // match outcome has ssn4 but matchlog has ssn
                else if (patient.Identifiers.HasLast4Social() && !patient.Identifiers.HasFullSocial() && !string.IsNullOrEmpty(log.SocialSecurityNumber))
                {
                    patientInfo.SSN = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial) && !string.IsNullOrWhiteSpace(i.Value))
                                            .Select(i => i.Value).FirstOrDefault();
                    patientInfo.IsSSNMatch = false;
                }
                // match outcome and matchlog have ssn4
                else if (!patient.Identifiers.HasFullSocial() && patient.Identifiers.HasLast4Social()
                    && string.IsNullOrEmpty(log.SocialSecurityNumber) && !string.IsNullOrEmpty(log.SocialSecuritySerial))
                {

                    patientInfo.SSN = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial) && !string.IsNullOrWhiteSpace(i.Value))
                                         .Select(i => i.Value).FirstOrDefault();
                    patientInfo.IsSSNMatch = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial) && !string.IsNullOrWhiteSpace(i.Value))
                                            .Select(i => i.Value).FirstOrDefault() == log.SocialSecuritySerial ||
                              patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial)).IsNullOrEmpty() && log.SocialSecuritySerial.IsNullOrEmpty() ? true : false;
                }
                // match outcome has ssn4 but matchlog is null
                else if (patient.Identifiers.HasLast4Social() && string.IsNullOrEmpty(log.SocialSecurityNumber) && string.IsNullOrEmpty(log.SocialSecuritySerial))
                {
                    patientInfo.SSN = patient.Identifiers.Where(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial) && !string.IsNullOrWhiteSpace(i.Value))
                                            .Select(i => i.Value).FirstOrDefault();
                    patientInfo.IsSSNMatch = false;
                }

                // match outcome and matchlog both has no ssn nor ssn4
                else if (!patient.Identifiers.HasFullSocial() && !patient.Identifiers.HasLast4Social()
                    && string.IsNullOrEmpty(log.SocialSecurityNumber) && string.IsNullOrEmpty(log.SocialSecuritySerial))
                {
                    patientInfo.SSN = null;
                    patientInfo.IsSSNMatch = true;
                }

                patientInfo.IsMedicaidStateMatch = (patientInfo.MedicaidState == log.MedicaidState) &&
                                                 (patientInfo.MedicaidNumber == log.MedicaidNumber);
                patientInfo.DocumentNames = GetDocumentsName(patient.PatientId);
                patientInfo.CreatedAt = patient.CreatedAt;
                patientInfo.UpdatedAt = patient.UpdatedAt;
                patientInfoList.Add(patientInfo);
                patientInfo.IsOrganizatioNameMatch = (patientInfo.Facilities?.Count() == 1) && (organization?.Name == patientInfo.Facilities.FirstOrDefault().Value);

            }
            return patientInfoList;
        }

        private AssociateMergeRequest CreateMergeOptionsForAutoResolve(MatchLog log, PatientMatchingResponse matchingResponse)
        {
            AssociateMergeRequest mergeOptions = null;
            Patients.Patient patient = null;
            if (matchingResponse.MatchResults != null && matchingResponse.MatchResults.Count() == 1)
            {
                // auto-merge
                patient = matchingResponse.MatchResults.FirstOrDefault()?.Match;
                var patientAddress = patient?.Addresses.FirstOrDefault();
                var patientHomePhone = patient?.Phones.Where(p => p.Type == ContactType.HomePhone).FirstOrDefault()?.Value;
                var patientWorkPhone = patient?.Phones.Where(p => p.Type == ContactType.WorkPhone).FirstOrDefault()?.Value;
                var patientMobile = patient?.Phones.Where(p => p.Type == ContactType.Mobile).FirstOrDefault()?.Value;
                var patientOtherPhone = patient?.Phones.Where(p => p.Type == ContactType.OtherPhone).FirstOrDefault()?.Value;
                var patientPrimaryPhone = patient?.Phones.Where(p => p.IsPrimary == true)?.FirstOrDefault()?.Type.ToString();
                // null-value override protection. Applies on optional fields
                // setting match flags to false, enforces CreatePatientFromLogAndMergeOptions
                // to use mergeOptions values instead of log values.
                mergeOptions = new AssociateMergeRequest()
                {
                    FirstName = null,
                    MiddleName = log.MiddleName.IsNullOrEmpty() ? patient.MiddleName : log.MiddleName,
                    IsMiddleNameMatch = false,
                    LastName = null,
                    Suffix = log.Suffix.IsNullOrEmpty() ? patient.Suffix : log.Suffix,
                    IsSuffixMatch = false,
                    DOB = null,
                    Line1 = log.Address1.IsNullOrEmpty() ? patientAddress?.Line1 : log.Address1,
                    IsLine1Match = false,
                    Line2 = log.Address2.IsNullOrEmpty() ? patientAddress?.Line2 : log.Address2,
                    IsLine2Match = false,
                    City = log.City.IsNullOrEmpty() ? patientAddress?.City : log.City,
                    IsCityMatch = false,
                    State = log.State.IsNullOrEmpty() ? patientAddress?.StateOrProvince : log.State,
                    PostalCode = null,
                    Gender = null,
                    OrganizationName = null,
                    SSN = null,
                    MedicareMBI = null,
                    MedicaidNumber = null,
                    FacilityMRN = null,
                    SelectedPatientId = null,

                    HomePhone = log.HomePhone.IsNullOrEmpty() ? patientHomePhone : log.HomePhone,
                    WorkPhone = log.WorkPhone.IsNullOrEmpty() ? patientWorkPhone : log.WorkPhone,
                    Mobile = log.Mobile.IsNullOrEmpty() ? patientMobile : log.Mobile,
                    OtherPhone = log.OtherPhone.IsNullOrEmpty() ? patientOtherPhone : log.OtherPhone,
                    PrimaryPhone = log.PrimaryPhone.IsNullOrEmpty() ? patientPrimaryPhone : log.PrimaryPhone,
                    IsHomePhoneMatch = false,
                    IsWorkPhoneMatch = false,
                    IsMobileMatch = false,
                    IsOtherPhoneMatch = false
                };
                mergeOptions.SelectedPatientId = patient?.PatientId.ToString();
            }
            else
            {
                // auto-create                
                mergeOptions = new AssociateMergeRequest()
                {
                    FirstName = null,
                    MiddleName = null,
                    IsMiddleNameMatch = true,
                    LastName = null,
                    Suffix = null,
                    IsSuffixMatch = true,
                    DOB = null,
                    Line1 = null,
                    IsLine1Match = true,
                    Line2 = null,
                    IsLine2Match = true,
                    City = null,
                    IsCityMatch = true,
                    State = null,
                    PostalCode = null,
                    Gender = null,
                    OrganizationName = null,
                    SSN = null,
                    MedicareMBI = null,
                    MedicaidNumber = null,
                    FacilityMRN = null,
                    SelectedPatientId = null,

                    HomePhone = null,
                    WorkPhone = null,
                    Mobile = null,
                    OtherPhone = null,
                    PrimaryPhone = null,
                    IsHomePhoneMatch = true,
                    IsWorkPhoneMatch = true,
                    IsMobileMatch = true,
                    IsOtherPhoneMatch = true
                };
            }

            if (log.IsSelfPay == true)
            {
                mergeOptions.IsSelfPaid = true;
            }
            else
            {
                mergeOptions.IsSelfPaid = false;
            }

            if (log.IsPrivateInsurance == true)
            {
                mergeOptions.IsPrivateInsuranceAvailable = true;
            }
            else
            {
                mergeOptions.IsPrivateInsuranceAvailable = false;
            }

            return mergeOptions;
        }

        private Patients.Patient CreatePatientFromLogAndMergeOptions(MatchLog log, AssociateMergeRequest mergeOptions)
        {
            Patients.Patient patient = new();
            var address = new Patients.PatientAddress();
            List<Patients.PatientIdentifier> identifiers = new List<Patients.PatientIdentifier>();

            if (!mergeOptions.SelectedPatientId.IsNullOrEmpty())
            {
                patient.PatientId = int.Parse(mergeOptions.SelectedPatientId);
            }

            if (mergeOptions.FirstName.IsNullOrEmpty())
            {
                patient.FirstName = log.FirstName;
            }
            else
            {
                patient.FirstName = mergeOptions.FirstName;
            }

            if (mergeOptions.IsMiddleNameMatch)
            {
                patient.MiddleName = log.MiddleName;
            }
            else
            {
                patient.MiddleName = mergeOptions.MiddleName ?? string.Empty;
            }

            if (mergeOptions.LastName.IsNullOrEmpty())
            {
                patient.LastName = log.LastName;
            }
            else
            {
                patient.LastName = mergeOptions.LastName;
            }

            if (mergeOptions.IsSuffixMatch)
            {
                patient.Suffix = log.Suffix;
            }
            else
            {
                patient.Suffix = mergeOptions.Suffix ?? string.Empty;
            }

            if (mergeOptions.DOB.IsNullOrEmpty())
            {
                patient.Birthdate = log.Birthdate;
            }
            else
            {
                patient.Birthdate = DateTime.Parse(mergeOptions.DOB);
            }

            if (mergeOptions.IsLine1Match)
            {
                address.Line1 = log.Address1;
            }
            else
            {
                address.Line1 = mergeOptions.Line1 ?? string.Empty;
            }

            if (mergeOptions.IsLine2Match)
            {
                address.Line2 = log.Address2;
            }
            else
            {
                address.Line2 = mergeOptions.Line2 ?? string.Empty;
            }

            if (mergeOptions.IsCityMatch)
            {
                address.City = log.City;
            }
            else
            {
                address.City = mergeOptions.City ?? string.Empty;
            }

            if (mergeOptions.State.IsNullOrEmpty())
            {
                address.StateOrProvince = log.State;
            }
            else
            {
                address.StateOrProvince = mergeOptions.State;
            }

            if (mergeOptions.PostalCode.IsNullOrEmpty())
            {
                address.PostalCode = log.Zip;
            }
            else
            {
                address.PostalCode = mergeOptions.PostalCode;
            }

            if (mergeOptions.Gender.IsNullOrEmpty())
            {
                patient.Gender = log.Gender;
            }
            else
            {
                patient.Gender = Enum.Parse<Gender>(mergeOptions.Gender);
            }

            if (mergeOptions.IsHomePhoneMatch)
            {
                if (!string.IsNullOrEmpty(log.HomePhone))
                {
                    patient.Phones.Add(new Patients.PatientPhone()
                    {
                        Type = ContactType.HomePhone,
                        Value = log.HomePhone,
                        IsActive = true,
                        IsPrimary = IsPrimaryPhoneInserted(patient.Phones) ? false : log.PrimaryPhone.EqualsIgnoreCase(ContactType.HomePhone.ToString())
                    });
                }
            }
            else
            {
                patient.Phones.Add(new Patients.PatientPhone()
                {
                    Type = ContactType.HomePhone,
                    Value = mergeOptions.HomePhone,
                    IsActive = true,
                    IsPrimary = mergeOptions.PrimaryPhone.EqualsIgnoreCase(ContactType.HomePhone.ToString())
                });
            }

            if (mergeOptions.IsWorkPhoneMatch)
            {
                if (!string.IsNullOrEmpty(log.WorkPhone))
                {
                    patient.Phones.Add(new Patients.PatientPhone()
                    {
                        Type = ContactType.WorkPhone,
                        Value = log.WorkPhone,
                        IsActive = true,
                        IsPrimary = IsPrimaryPhoneInserted(patient.Phones) ? false : log.PrimaryPhone.EqualsIgnoreCase(ContactType.WorkPhone.ToString())
                    });
                }
            }
            else
            {
                patient.Phones.Add(new Patients.PatientPhone()
                {
                    Type = ContactType.WorkPhone,
                    Value = mergeOptions.WorkPhone,
                    IsActive = true,
                    IsPrimary = mergeOptions.PrimaryPhone.EqualsIgnoreCase(ContactType.WorkPhone.ToString())
                });

            }

            if (mergeOptions.IsMobileMatch)
            {
                if (!string.IsNullOrEmpty(log.Mobile))
                {
                    patient.Phones.Add(new Patients.PatientPhone()
                    {
                        Type = ContactType.Mobile,
                        Value = log.Mobile,
                        IsActive = true,
                        IsPrimary = IsPrimaryPhoneInserted(patient.Phones) ? false : log.PrimaryPhone.EqualsIgnoreCase(ContactType.Mobile.ToString())
                    });
                }
            }
            else
            {
                patient.Phones.Add(new Patients.PatientPhone()
                {
                    Type = ContactType.Mobile,
                    Value = mergeOptions.Mobile,
                    IsActive = true,
                    IsPrimary = mergeOptions.PrimaryPhone.EqualsIgnoreCase(ContactType.Mobile.ToString())
                });
            }

            if (mergeOptions.IsOtherPhoneMatch)
            {
                if (!string.IsNullOrEmpty(log.OtherPhone))
                {
                    patient.Phones.Add(new Patients.PatientPhone()
                    {
                        Type = ContactType.OtherPhone,
                        Value = log.OtherPhone,
                        IsActive = true,
                        IsPrimary = IsPrimaryPhoneInserted(patient.Phones) ? false : log.PrimaryPhone.EqualsIgnoreCase(ContactType.OtherPhone.ToString())
                    });
                }
            }
            else
            {
                patient.Phones.Add(new Patients.PatientPhone()
                {
                    Type = ContactType.OtherPhone,
                    Value = mergeOptions.OtherPhone,
                    IsActive = true,
                    IsPrimary = mergeOptions.PrimaryPhone.EqualsIgnoreCase(ContactType.OtherPhone.ToString())
                });

            }

            if (mergeOptions.IsSSNDownCasted == false)
            {
                if (mergeOptions.SSN.IsNullOrEmpty())
                {
                    if (!log.SocialSecurityNumber.IsNullOrEmpty())
                    {
                        identifiers.Add(KnownTypes.SocialSecurityNumber, log.SocialSecurityNumber);
                        identifiers.Add(KnownTypes.SocialSecuritySerial, log.SocialSecurityNumber.GetLast(4));
                    }
                    else if (!log.SocialSecuritySerial.IsNullOrEmpty())
                    {
                        identifiers.Add(KnownTypes.SocialSecuritySerial, log.SocialSecuritySerial);
                    }
                }
                else
                {
                    // in AssociateMergeRequest SSN is place holder for both SSN and SSN4.
                    if (mergeOptions.SSN.Length == 9)
                    {
                        identifiers.Add(KnownTypes.SocialSecurityNumber, mergeOptions.SSN);
                        identifiers.Add(KnownTypes.SocialSecuritySerial, mergeOptions.SSN.GetLast(4));
                    }
                    else if (mergeOptions.SSN.Length == 4)
                    {
                        identifiers.Add(KnownTypes.SocialSecuritySerial, mergeOptions.SSN);
                    }

                }
            }
            else if (!mergeOptions.SSN.IsNullOrEmpty())
            {
                if (mergeOptions.SSN.Length == 9)
                {
                    identifiers.Add(KnownTypes.SocialSecurityNumber, mergeOptions.SSN);
                    identifiers.Add(KnownTypes.SocialSecuritySerial, mergeOptions.SSN.GetLast(4));
                }
                else if (mergeOptions.SSN.Length == 4)
                {
                    identifiers.Add(KnownTypes.SocialSecuritySerial, mergeOptions.SSN);
                }
            }


            if (mergeOptions.MedicareMBI.IsNullOrEmpty())
            {
                if (!log.MedicareNumber.IsNullOrEmpty())
                {
                    identifiers.Add(new Patients.PatientIdentifier()
                    {
                        Type = KnownTypes.MedicareBeneficiaryNumber,
                        Value = log.MedicareNumber
                    });
                }
            }
            else
            {
                identifiers.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.MedicareBeneficiaryNumber,
                    Value = mergeOptions.MedicareMBI
                });
            }

            if (mergeOptions.MedicaidNumber.IsNullOrEmpty())
            {
                if (!log.MedicaidNumber.IsNullOrEmpty())
                {
                    identifiers.Add(new Patients.PatientIdentifier()
                    {
                        Type = KnownTypes.MedicaidNumber,
                        Value = log.MedicaidNumber
                    });
                }
                if (!log.MedicaidState.IsNullOrEmpty())
                {
                    identifiers.Add(new Patients.PatientIdentifier()
                    {
                        Type = KnownTypes.MedicaidState,
                        Value = log.MedicaidState
                    });
                }

            }
            else
            {
                if (mergeOptions.MedicaidNumber.Any(c => c.Equals('(')) && mergeOptions.MedicaidNumber.Any(c => c.Equals(')')))
                {
                    identifiers.Add(new Patients.PatientIdentifier()
                    {
                        Type = KnownTypes.MedicaidNumber,
                        Value = mergeOptions.MedicaidNumber.Substring(0, mergeOptions.MedicaidNumber.IndexOf("(")).Trim()
                    });
                    identifiers.Add(new Patients.PatientIdentifier()
                    {
                        Type = KnownTypes.MedicaidState,
                        Value = mergeOptions.MedicaidNumber.Substring(mergeOptions.MedicaidNumber.IndexOf("(") + 1, 2).Trim()
                    });
                }
                else
                {
                    identifiers.Add(new Patients.PatientIdentifier()
                    {
                        Type = KnownTypes.MedicaidNumber,
                        Value = mergeOptions.MedicaidNumber.Trim()
                    });
                }
            }

            if (mergeOptions.FacilityMRN.IsNullOrEmpty())
            {
                if (!log.FacilityMRN.IsNullOrEmpty())
                    identifiers.Add(new Patients.PatientIdentifier()
                    {
                        Type = KnownTypes.UniqueExternalIdentifier,
                        Value = log.FacilityMRN
                    });
            }
            else
            {
                identifiers.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.UniqueExternalIdentifier,
                    Value = mergeOptions.FacilityMRN
                });
            }

            if (mergeOptions.IsSelfPaid == true)
            {
                identifiers.Add(KnownTypes.SelfPay, string.Empty);
            }

            if (mergeOptions.IsPrivateInsuranceAvailable == true)
            {
                identifiers.Add(KnownTypes.PrivateInsurance, string.Empty);
            }

            patient.Identifiers = identifiers;
            patient.Addresses = new List<Patients.PatientAddress>() { address };

            return patient;
        }

        private async Task UpdatePatient(Patients.Patient patient, Application.Organization organization, AssociateMergeRequest mergeOptions, int requestId)
        {
            await PatientService.UpdateAsync(patient, organization.OrganizationId, CurrentUser.Id);

            if (mergeOptions.IsSelfPaid == false && mergeOptions.IsPrivateInsuranceAvailable == false)
            {
                await PatientService.ResetPayerMixFlags(patient.PatientId, CurrentUser.Id);
            }

            string ssn = patient.Identifiers.Where(i => i.Type == KnownTypes.SocialSecurityNumber).FirstOrDefault()?.Value;
            string ssn4 = patient.Identifiers.Where(i => i.Type == KnownTypes.SocialSecuritySerial).FirstOrDefault()?.Value;

            await PatientService.UpdatePatientSocialSecurityInfo(patient.PatientId, ssn, ssn4, CurrentUser.Id);
            await PatientService.SetPatientMatchLogFlagsToResolved((int)requestId, CurrentUser.Id);
        }
        
        private async Task AutoResolve(MatchLog log, PatientMatchingResponse matchingResponse, Application.Organization organization, int requestId, bool isAutoMerge,bool isAutoCreate) 
        {
            var mergeOptions = CreateMergeOptionsForAutoResolve(log, matchingResponse);
            var patient = CreatePatientFromLogAndMergeOptions(log, mergeOptions);
            if(isAutoMerge && !isAutoCreate) 
            {
                // auto-merge
                await UpdatePatient(patient, organization, mergeOptions, requestId);
            }
            else if(isAutoCreate && !isAutoMerge)
            {
                // auto-create
                await PatientService.CreateAsync(patient, organization.OrganizationId, CurrentUser.Id);
                await PatientService.SetPatientMatchLogFlagsToResolved((int)requestId, CurrentUser.Id);
            }
        }

        private bool IsPhoneMatch(ICollection<Patients.PatientPhone> phones, ContactType type, string matchlog, string primaryPhone)
        {

            bool result;
            bool isPrimary = false;
            bool? primaryFlag = phones.Where(i => i.Type == type && !string.IsNullOrEmpty(i.Value))
                           .Select(i => i.IsPrimary)
                           .FirstOrDefault();


            if (primaryPhone == type.ToString())
            {
                // current type is primary phone
                isPrimary = primaryFlag.HasValue && primaryFlag.Value == true;
            }
            else
            {
                // current type is not primary phone
                isPrimary = primaryFlag.HasValue && primaryFlag.Value != true;
            }

            result = phones.Where(i => i.Type == type && !string.IsNullOrEmpty(i.Value))
                           .Select(i => i.Value)
                           .FirstOrDefault()?.Trim() == matchlog &&
                     isPrimary
                           ||
                     phones.Where(i => i.Type == type).IsNullOrEmpty() && matchlog.IsNullOrEmpty() ? true : false;
            return result;
        }

        private bool IsPrimaryPhoneInserted(ICollection<Patients.PatientPhone> phones)
        {
            return phones.Where(i => i.IsPrimary == true).Any();
        }

        private int NormalizeMatchScore(float score)
        {
            int result;

            if (score >= 100)
            {
                int percentage = (int)(((score - 100) / (363 - 100)) * 25 + 75);
                result = percentage > 100 ? 100 : percentage;
            }
            else if (score >= 57)
            {
                result = (int)(((score - 57) / (100 - 57)) * 24 + 50);
            }
            else if (score >= 0)
            {
                result = (int)(((score - 0) / (57 - 0)) * 24 + 25);
            }
            else
            {
                result = (int)(((score + 304) / (303)) * 24);
            }
            return result;
        }

        private List<string> GetDocumentsName(int patientId, int count = 10)
        {
            List<string> fileNames = new List<string>();
            var requestList = RequestService.GetServiceableRequests().Where(r => r.PatientId == patientId)
                                                                     .Include(r => r.Signer)
                                                                     .Include(r => r.Patient)
                                                                     .Include(r => r.Template).ThenInclude(t => t.TemplateType)
                                                                     .OrderByDescending(r => r.SubmittedAt)
                                                                     .Take(count);
            foreach (var request in requestList)
            {
                fileNames.Add(request.GetRequestShortFileName(CurrentUser) + ".pdf");
            }
            return fileNames;
        }
    }
}
