using System.Data.Common;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using SutureHealth.AspNetCore.Areas.Patient.Models;
using SutureHealth.AspNetCore.Mvc.Attributes;
using SutureHealth.Linq;
using SutureHealth.Patients.Services;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.Patients;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;
using Microsoft.IdentityModel.Tokens;

namespace SutureHealth.AspNetCore.Areas.Controllers.Patient
{
    [Area("Patient")]
    [Route("Patient")]
    public class PatientController : Controller
    {
        protected ILogger<PatientController> Logger { get; }
        protected IApplicationService SecurityService { get; }
        protected IPatientServicesProvider PatientService { get; }

        public PatientController
        (
            ILogger<PatientController> logger,
            IApplicationService securityService,
            IPatientServicesProvider patientService
        )
        {
            Logger = logger;
            SecurityService = securityService;
            PatientService = patientService;
        }

        protected PatientModel InitializePatientModel(Application.Organization organization, PatientModel model = null)
        {
            model = model ?? new PatientModel() { SocialSecurityNumberType = PatientModel.SocialSecurityNumberStyle.Full };

            model.CurrentUser = CurrentUser;
            model.OrganizationName = !string.IsNullOrWhiteSpace(organization.OtherDesignation) ? organization.OtherDesignation : organization.Name;
            model.State = model?.State ?? organization.StateOrProvince;
            model.MedicaidState = model?.MedicaidState ?? organization.StateOrProvince;
            model.RequireClientHeader = SecurityService.ShowLegacyNavBar(CurrentUser.IsUserSender(), CurrentUser.MemberId);

            return model;
        }

        [HttpGet("", Name = "PatientIndex")]
        [HttpGet("Create", Name = "PatientCreate")]
        public async Task<IActionResult> Index() =>
            RedirectToRoute("OrganizationPatientCreate", new
            {
                organizationId = CurrentUser.PrimaryOrganizationId
            });

        [HttpGet("/Patient/CancelCreate", Name = "CancelCreate")]
        public async Task<IActionResult> CancelCreate()
        {
            if (CurrentUser.IsApplicationAdministrator())
            {
                return RedirectToRoute("AdminPatientSearch");
            }
            return RedirectToAction("Index", "Send", new { area = "Request", contentOnly = true });
        }

        [HttpGet("/Organization/{organizationId:int}/Patient/Create", Name = "OrganizationPatientCreate")]
        [RequireAuthorizedOrganization]
        public IActionResult Create(Application.Organization organization)
        {
            return View("Index", InitializePatientModel(organization));
        }
        
        [HttpGet("/Organization/{organizationId:int}/Patient/{patientId:int}", Name = "OrganizationPatientEdit")]
        [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
        [RequireAuthorizedOrganization]
        public async Task<IActionResult> Edit(Application.Organization organization, int patientId)
        {
            var patient = await PatientService.GetByIdAsync(patientId);

            if (patient == null)
            {
                return NotFound();
            }

            var address = patient.Addresses.FirstOrDefault();
            var ssnType = patient.Identifiers.Where(id => id.Type == KnownTypes.SocialSecurityNumber || id.Type == KnownTypes.SocialSecuritySerial)
                                             .OrderBy(id => id.Type == KnownTypes.SocialSecuritySerial)
                                             .Select(id => id.Type)
                                             .FirstOrDefault();
            var mrn = patient.OrganizationKeys.FirstOrDefault(ok => ok.OrganizationId == organization.OrganizationId)?.MedicalRecordNumber;
            var homePhone = patient?.Phones.FirstOrDefault(p => p.Type == ContactType.HomePhone);
            var workPhone = patient?.Phones.FirstOrDefault(p => p.Type == ContactType.WorkPhone);
            var otherPhone = patient?.Phones.FirstOrDefault(p => p.Type == ContactType.OtherPhone);
            var mobilePhone = patient?.Phones.FirstOrDefault(p => p.Type == ContactType.Mobile);
            string primaryPhone = string.Empty;
            if(homePhone != null && homePhone.Value != null) 
            {
                if (homePhone.IsPrimary.HasValue && homePhone.IsPrimary.Value == true) 
                {
                    primaryPhone = "Home Phone";
                }
            }
            if (workPhone != null && workPhone.Value != null)
            {
                if (workPhone.IsPrimary.HasValue && workPhone.IsPrimary.Value == true)
                {
                    primaryPhone = "Work Phone";
                }
            }
            if (otherPhone != null && otherPhone.Value != null)
            {
                if (otherPhone.IsPrimary.HasValue && otherPhone.IsPrimary.Value == true)
                {
                    primaryPhone = "Other Phone";
                }
            }

            if (mobilePhone != null && mobilePhone.Value != null)
            {
                if (mobilePhone.IsPrimary.HasValue && mobilePhone.IsPrimary.Value == true)
                {
                    primaryPhone = "Mobile Phone";
                }
            }

            var model = new PatientModel()
            {
                CurrentUser = CurrentUser,
                PatientId = patientId,
                OrganizationId = organization.OrganizationId,
                OrganizationName = organization.Name,
                FirstName = patient.FirstName,
                MiddleName = patient.MiddleName,
                LastName = patient.LastName,
                Suffix = patient.Suffix,
                DateOfBirth = patient.Birthdate,
                Gender = patient.Gender,
                GenderValue = patient.Gender.ToString(),
                Address1 = address?.Line1,
                Address2 = address?.Line2,
                City = address?.City,
                State = address?.StateOrProvince,
                ZipCode = address?.PostalCode,
                HomePhone = homePhone?.Value,
                WorkPhone = workPhone?.Value,
                OtherPhone = otherPhone?.Value,
                MobilePhone = mobilePhone?.Value,
                PrimaryPhone = primaryPhone,
                SocialSecurityNumber = ssnType != null ? patient.Identifiers.FirstOrDefault(id => id.Type == ssnType).Value : null,
                SocialSecurityNumberType = ssnType switch
                {
                    KnownTypes.SocialSecurityNumber => PatientModel.SocialSecurityNumberStyle.Full,
                    KnownTypes.SocialSecuritySerial => PatientModel.SocialSecurityNumberStyle.Last4,
                    _ => PatientModel.SocialSecurityNumberStyle.Unavailable
                },
                HasUnavailablePatientRecordNumber = string.IsNullOrWhiteSpace(mrn),
                PatientRecordNumber = mrn,
                HasMedicaid = patient.Identifiers.Any(id => id.Type == KnownTypes.Medicaid),
                MedicaidNumber = patient.Identifiers.FirstOrDefault(id => id.Type == KnownTypes.MedicaidNumber)?.Value,
                MedicaidState = patient.Identifiers.FirstOrDefault(id => id.Type == KnownTypes.MedicaidState)?.Value,
                HasMedicare = patient.Identifiers.Any(id => id.Type == KnownTypes.Medicare || id.Type == KnownTypes.MedicareAdvantage),
                MedicareMBI = patient.Identifiers.FirstOrDefault(id => id.Type == KnownTypes.MedicareBeneficiaryNumber)?.Value,
                HasMedicareAdvantage = patient.Identifiers.Any(id => id.Type == KnownTypes.MedicareAdvantage),
                HasPrivateInsurance = patient.Identifiers.Any(id => id.Type == KnownTypes.PrivateInsurance),
                HasSelfPay = patient.Identifiers.Any(id => id.Type == KnownTypes.SelfPay),
                HasUnavailablePayerMix = !patient.Identifiers.Any(id => id.Type == KnownTypes.Medicaid || id.Type == KnownTypes.Medicare ||
                                                                        id.Type == KnownTypes.MedicareAdvantage || id.Type == KnownTypes.PrivateInsurance ||
                                                                        id.Type == KnownTypes.SelfPay)
            };

            return View("Index", InitializePatientModel(organization, model));
        }

        [HttpPost("/Organization/{organizationId:int}/Patient/Create", Name = "OrganizationPatientSaveNew")]
        [RequireAuthorizedOrganization]
        public async Task<IActionResult> SaveNew(Application.Organization organization, [FromForm] PatientModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.PatientRecordNumber) && PatientService.GetByExternalIdentifier(organization.OrganizationId, model.PatientRecordNumber).Any())
            {
                ModelState.AddModelError("PatientRecordNumber", "DUPLICATE");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", InitializePatientModel(organization, model));
            }

            var identifiers = new List<Identifier>();
            if (model.SocialSecurityNumberType != PatientModel.SocialSecurityNumberStyle.Unavailable && !string.IsNullOrWhiteSpace(model.SocialSecurityNumber))
            {
                identifiers.Add(new Identifier
                {
                    Type = (model.SocialSecurityNumberType == PatientModel.SocialSecurityNumberStyle.Full) ? KnownTypes.SocialSecurityNumber : KnownTypes.SocialSecuritySerial,
                    Value = model.SocialSecurityNumber
                });
            }

            if (model.HasMedicaid && !string.IsNullOrWhiteSpace(model.MedicaidNumber))
            {
                identifiers.Add(new Identifier
                {
                    Type = KnownTypes.MedicaidNumber,
                    Value = model.MedicaidNumber
                });
            }

            if (model.HasMedicaid && !string.IsNullOrWhiteSpace(model.MedicaidState))
            {
                identifiers.Add(new Identifier
                {
                    Type = KnownTypes.MedicaidState,
                    Value = model.MedicaidState
                });
            }

            if (model.HasMedicare && !string.IsNullOrWhiteSpace(model.MedicareMBI))
            {
                identifiers.Add(new Identifier
                {
                    Type = KnownTypes.MedicareBeneficiaryNumber,
                    Value = model.MedicareMBI.Replace("-", "")
                });
            }

            if (!string.IsNullOrWhiteSpace(model.PatientRecordNumber))
            {
                identifiers.Add(new Identifier
                {
                    Type = KnownTypes.UniqueExternalIdentifier,
                    Value = model.PatientRecordNumber
                });
            }

            if (model.HasSelfPay)
            {
                identifiers.Add(new Identifier
                {
                    Type = KnownTypes.SelfPay,
                    Value = string.Empty
                });
            }
            if (model.HasPrivateInsurance)
            {
                identifiers.Add(new Identifier
                {
                    Type = KnownTypes.PrivateInsurance,
                    Value = string.Empty
                });
            }
            if (model.HasMedicareAdvantage)
            {
                identifiers.Add(new Identifier
                {
                    Type = KnownTypes.MedicareAdvantage,
                    Value = string.Empty
                });
            }

            var phones = new List<PatientPhone>();

            if (!string.IsNullOrEmpty(model.HomePhone))
            {
                if (!string.IsNullOrEmpty(model.PrimaryPhone) && model.PrimaryPhone.EqualsIgnoreCase("HomePhone"))
                {
                    phones.Add(new() { Value = model.HomePhone.ToFormattedPhoneNumber(), Type = ContactType.HomePhone, IsPrimary = true });
                }
                else
                {
                    phones.Add(new() { Value = model.HomePhone.ToFormattedPhoneNumber(), Type = ContactType.HomePhone });
                }
            }
            if (!string.IsNullOrEmpty(model.WorkPhone))
            {
                if (!string.IsNullOrEmpty(model.PrimaryPhone) && model.PrimaryPhone.EqualsIgnoreCase("WorkPhone"))
                {
                    phones.Add(new() { Value = model.WorkPhone.ToFormattedPhoneNumber(), Type = ContactType.WorkPhone, IsPrimary = true });
                }
                else
                {
                    phones.Add(new() { Value = model.WorkPhone.ToFormattedPhoneNumber(), Type = ContactType.WorkPhone });
                }
            }
            if (!string.IsNullOrEmpty(model.MobilePhone))
            {
                if (!string.IsNullOrEmpty(model.PrimaryPhone) && model.PrimaryPhone.EqualsIgnoreCase("MobilePhone"))
                {
                    phones.Add(new() { Value = model.MobilePhone.ToFormattedPhoneNumber(), Type = ContactType.Mobile, IsPrimary = true });
                }
                else
                {
                    phones.Add(new() { Value = model.MobilePhone.ToFormattedPhoneNumber(), Type = ContactType.Mobile });
                }
            }
            if (!string.IsNullOrEmpty(model.OtherPhone))
            {
                if (!string.IsNullOrEmpty(model.PrimaryPhone) && model.PrimaryPhone.EqualsIgnoreCase("OtherPhone"))
                {
                    phones.Add(new() { Value = model.OtherPhone.ToFormattedPhoneNumber(), Type = ContactType.OtherPhone, IsPrimary = true });
                }
                else
                {
                    phones.Add(new() { Value = model.OtherPhone.ToFormattedPhoneNumber(), Type = ContactType.OtherPhone });
                }
            }

            var matches = await PatientService.MatchAsync(new Patients.PatientMatchingRequest
            {
                Birthdate = model.DateOfBirth.Value,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                Suffix = model.Suffix,
                Gender = model.Gender.GetValueOrDefault(Gender.Unknown),
                MemberId = CurrentUser.Id,
                OrganizationId = organization.OrganizationId,
                AddressLine1 = model.Address1,
                AddressLine2 = model.Address2,
                City = model.City,
                StateOrProvince = model.State,
                PostalCode = model.ZipCode,
                Ids = identifiers.Cast<IIdentifier>().ToList(),
                Phones = phones,
                ManualReviewEnabled = true
            });

            if (model.ForceAddSimilarPatient)
                matches.MatchLevel = MatchLevel.NonMatch;

            int? patientId = null;
            switch (matches.MatchLevel)
            {
                case MatchLevel.NonMatch:
                    patientId = (await PatientService.CreateAsync(model.ToPatient(), organization.OrganizationId, CurrentUser.Id)).PatientId;
                    break;
                case MatchLevel.Match:

                    var matchPatient = matches.TopMatch;

                    var newPatient = model.ToPatient();
                    newPatient.PatientId = matchPatient.PatientId;

                    // Don't replace optional fields if we already have more detail than submitted.
                    if (string.IsNullOrWhiteSpace(newPatient.MiddleName) &&
                        !string.IsNullOrWhiteSpace(matchPatient.MiddleName))
                    {
                        newPatient.MiddleName = matchPatient.MiddleName;
                    }

                    if (string.IsNullOrWhiteSpace(newPatient.Suffix) && !string.IsNullOrWhiteSpace(matchPatient.Suffix))
                    {
                        newPatient.Suffix = matchPatient.Suffix;
                    }


                    var newPatientHasFullSocial = newPatient.HasFullSocial();
                    var matchPatientHasFullSocial = matchPatient.HasFullSocial();

                    var newPatientHasLast4Social = newPatient.HasLast4Social();
                    var matchPatientHasLast4Social = matchPatient.HasLast4Social();

                    var newIdentifiers = new List<PatientIdentifier>();

                    void AddIdentifier(Patients.Patient p, string t) =>
                        newIdentifiers.AddRange(p.Identifiers.Where(identifier => identifier.Type.Equals(t)));

                    if (newPatientHasFullSocial)
                    {
                        AddIdentifier(newPatient, KnownTypes.SocialSecurityNumber);
                    }
                    else if (matchPatientHasFullSocial)
                    {
                        AddIdentifier(matchPatient, KnownTypes.SocialSecurityNumber);
                    }
                    else if (newPatientHasLast4Social)
                    {
                        AddIdentifier(newPatient, KnownTypes.SocialSecuritySerial);
                    }
                    else if (matchPatientHasLast4Social)
                    {
                        AddIdentifier(matchPatient, KnownTypes.SocialSecuritySerial);
                    }

                    newIdentifiers.AddRange(newPatient.Identifiers.Where(identifier =>
                        identifier.Type is not (KnownTypes.SocialSecurityNumber or KnownTypes.SocialSecuritySerial)));
                    newIdentifiers.AddRange(matchPatient.Identifiers.Where(identifier =>
                        identifier.Type is not (KnownTypes.SocialSecurityNumber or KnownTypes.SocialSecuritySerial)));

                    newPatient.Identifiers = newIdentifiers;

                    await PatientService.UpdateAsync(newPatient, organization.OrganizationId, CurrentUser.Id);
                    patientId = matchPatient.PatientId;
                    break;
                case Linq.MatchLevel.Similar:
                case Linq.MatchLevel.SimilarHighRisk:
                    model.SimilarPatientDialog = ProcessMatchResults(model, matches, CurrentUser.IsApplicationAdministrator() || matches.MatchLevel == Linq.MatchLevel.Similar);
                    break;
                default:
                    throw new ApplicationException("Invalid Matching Scenario");
            }

            return patientId.HasValue ?
                (CurrentUser.IsApplicationAdministrator() ?
                    RedirectToRoute("AdminPatientSearch") :
                    RedirectToRoute("SendIndexWithPatient", new { organizationId = organization.OrganizationId, patientId = patientId.Value })) :
                View("Index", InitializePatientModel(organization, model));
        }

        [HttpPost("/Organization/{organizationId:int}/Patient/{patientId:int}", Name = "OrganizationPatientEdit")]
        [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
        [RequireAuthorizedOrganization]
        public async Task<IActionResult> Save(Application.Organization organization, int patientId, [FromForm] PatientModel model)
        {
            var patient = await PatientService.GetByIdAsync(patientId);
            var modifiedPatient = model.ToPatient();

            if (patient == null)
            {
                return NotFound();
            }

            modifiedPatient.PatientId = patientId;
            await PatientService.UpdateAsync(modifiedPatient, organization.OrganizationId, CurrentUser.Id);

            return RedirectToRoute("AdminPatientSearch");
        }

        [HttpPost]
        [Route("Validation/MRN", Name = "OrganizationPatientValidateMrn")]
        public async Task<IActionResult> ValidateMrn(int? patientId, int organizationId, string patientRecordNumber)
            => Json(string.IsNullOrWhiteSpace(patientRecordNumber) || !PatientService.GetByExternalIdentifier(organizationId, patientRecordNumber).Any(p => p.PatientId != patientId));

        protected PatientModel.SimilarPatientModel ProcessMatchResults(PatientModel patientModel, Patients.PatientMatchingResponse matches, bool allowOverride = false)
        {
            var discrepancies = new List<PatientModel.SimilarPatientModel.SimilarField>();
            var bestMatch = matches.MatchResults.ElementAt(0);

            if (bestMatch != null)
            {
                if (bestMatch.Rules.Any(r => r.Rule.Description.Contains("LastName") && r.Score <= 0)) discrepancies.Add(PatientModel.SimilarPatientModel.SimilarField.LastName);
                if (bestMatch.Rules.Any(r => r.Rule.Description.Contains("FirstName") && r.Score <= 0)) discrepancies.Add(PatientModel.SimilarPatientModel.SimilarField.FirstName);
                if (bestMatch.Rules.Any(r => r.Rule.Description.Contains("DateOfBirth") && r.Score <= 0)) discrepancies.Add(PatientModel.SimilarPatientModel.SimilarField.Birthdate);
                if (bestMatch.Rules.Any(r => r.Rule.Description.Contains("Gender") && r.Score <= 0)) discrepancies.Add(PatientModel.SimilarPatientModel.SimilarField.Gender);
                if (bestMatch.Rules.Any(r => r.Rule.Description.Contains("PostalCode") && r.Score <= 0)) discrepancies.Add(PatientModel.SimilarPatientModel.SimilarField.PostalCode);
                if (bestMatch.Rules.Any(r => r.Rule.Description.Contains("ssn") && r.Score <= 0)) discrepancies.Add(PatientModel.SimilarPatientModel.SimilarField.SocialSecurityNumber);
                if (bestMatch.Rules.Any(r => r.Rule.Description.Contains(KnownTypes.MedicareBeneficiaryNumber) && r.Score <= 0)) discrepancies.Add(PatientModel.SimilarPatientModel.SimilarField.MedicareMBI);
                if (bestMatch.Rules.Any(r => r.Rule.Description.Contains(KnownTypes.MedicaidNumber) && r.Score <= 0)) discrepancies.Add(PatientModel.SimilarPatientModel.SimilarField.MedicaidNumber);
            }

            return new PatientModel.SimilarPatientModel
            {
                AllowAdd = allowOverride,
                Similarities = discrepancies
            };
        }
    }
}
