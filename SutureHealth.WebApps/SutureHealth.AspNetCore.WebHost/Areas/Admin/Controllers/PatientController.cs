using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using SutureHealth.AspNetCore.Areas.Admin.Models;
using SutureHealth.AspNetCore.Areas.Admin.Models.Patient;
using SutureHealth.AspNetCore.Mvc.Extensions;
using SutureHealth.Patients.Services;
using SutureHealth.Application.Services;
using Humanizer;
using SutureHealth.Linq;
using Microsoft.EntityFrameworkCore;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Patient")]
    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    [Authorize(AuthorizationPolicies.ShowPatientsInfo)]
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

        [HttpGet("matches/{matchPatientLogId:int?}")]
        public async Task<IActionResult> GetPatientMatchLog(int? matchPatientLogId = null)
        {
            MatchLogModel model = null;
            if (matchPatientLogId.HasValue)
            {
                var matchLog = await PatientService.GetMatchLogByIdAsync(matchPatientLogId.Value);
                var identifiers = new List<Identifier>();

                if (!string.IsNullOrWhiteSpace(matchLog.FacilityMRN))
                    identifiers.Add(KnownTypes.UniqueSutureIdentifier, matchLog.FacilityMRN);
                if (!string.IsNullOrWhiteSpace(matchLog.MedicareNumber))
                    identifiers.Add(KnownTypes.MedicareBeneficiaryNumber, matchLog.MedicareNumber);
                if (!string.IsNullOrWhiteSpace(matchLog.MedicaidNumber))
                {
                    identifiers.Add(KnownTypes.MedicaidNumber, matchLog.MedicaidNumber);
                    identifiers.Add(KnownTypes.MedicaidState, matchLog.MedicaidState);
                }
                if (!string.IsNullOrWhiteSpace(matchLog.SocialSecurityNumber))
                    identifiers.Add(KnownTypes.SocialSecurityNumber, matchLog.SocialSecurityNumber);
                if (!string.IsNullOrWhiteSpace(matchLog.SocialSecuritySerial))
                    identifiers.Add(KnownTypes.SocialSecuritySerial, matchLog.SocialSecuritySerial);


                var matches = await PatientService.MatchAsync(new Patients.PatientMatchingRequest
                {
                    AddressLine1 = matchLog.Address1,
                    AddressLine2 = matchLog.Address2,
                    Birthdate = matchLog.Birthdate,
                    City = matchLog.City,
                    FirstName = matchLog.FirstName,
                    Gender = matchLog.Gender,
                    LastName = matchLog.LastName,
                    MemberId = CurrentUser.Id,
                    MiddleName = matchLog.MiddleName,
                    OrganizationId = matchLog.FacilityId,
                    PostalCode = matchLog.Zip,
                    StateOrProvince = matchLog.State,
                    Ids = identifiers.ToList<IIdentifier>(),
                    Ruleset = Patients.MatchingRuleset.DocumentProcessing,
                    LogMatches = false, 
                    ManualReviewEnabled = true
                });


                var goldenTickets = matches.MatchResults.Where(r => r.Rules.OfType<GoldenTicketRuleResult<SutureHealth.Patients.Patient>>().Any());
                var scoredResults = matches.MatchResults.Where(r => r.Score > -20).Take(30);
                model = new MatchLogModel
                {
                    MatchPatientLog = matchLog,
                    Matches = goldenTickets.Union(scoredResults).OrderByDescending(mr => mr.Score),
                    RequireKendo = true,
                    SocialSecurityNumber = string.IsNullOrWhiteSpace(matchLog.SocialSecurityNumber) ? matchLog.SocialSecuritySerial : matchLog.SocialSecurityNumber,
                    SocialSecurityNumberType = !string.IsNullOrWhiteSpace(matchLog.SocialSecurityNumber) ? SocialSecurityNumberStyle.Full :
                                               !string.IsNullOrWhiteSpace(matchLog.SocialSecuritySerial) ? SocialSecurityNumberStyle.Last4 : SocialSecurityNumberStyle.Unavailable,
                    CurrentUser = CurrentUser
                };
            }
            else
            {
                model = new MatchLogModel
                {
                    MatchPatientLog = new Patients.MatchLog
                    {
                        Birthdate = DateTime.Now.Date,
                        Gender = Gender.Male,
                        Outcomes = Array.Empty<Patients.MatchOutcome>()
                    },
                    RequireKendo = true,
                    SocialSecurityNumberType = SocialSecurityNumberStyle.Unavailable,
                    CurrentUser = CurrentUser
                };
            }

            return View("MatchLog", model);
        }

        [HttpPost("matches")]
        public async Task<IActionResult> PostPatchMatch([Bind(Prefix = nameof(MatchLogModel.MatchPatientLog))] Patients.MatchLog matchLog)
        {
            var identifiers = new List<Identifier>();

            if (!string.IsNullOrWhiteSpace(matchLog.FacilityMRN))
                identifiers.Add(KnownTypes.UniqueSutureIdentifier, matchLog.FacilityMRN);
            if (!string.IsNullOrWhiteSpace(matchLog.MedicareNumber))
                identifiers.Add(KnownTypes.MedicareBeneficiaryNumber, matchLog.MedicareNumber);
            if (!string.IsNullOrWhiteSpace(matchLog.MedicaidNumber))
            {
                identifiers.Add(KnownTypes.MedicaidNumber, matchLog.MedicaidNumber);
                identifiers.Add(KnownTypes.MedicaidState, matchLog.MedicaidState);
            }

            var socialSecurityMask = Request.Form["SocialSecurityNumber"].FirstOrDefault();
            var socialSecurityType = Request.Form["SocialSecurityNumberType"].FirstOrDefault();

            if (socialSecurityType.HasValue() && socialSecurityType != SocialSecurityNumberStyle.Unavailable.ToString() && !string.IsNullOrEmpty(socialSecurityMask))
            {
                if (socialSecurityType == SocialSecurityNumberStyle.Full.ToString())
                    identifiers.Add(KnownTypes.SocialSecurityNumber, socialSecurityMask);
                if (socialSecurityType == SocialSecurityNumberStyle.Last4.ToString())
                    identifiers.Add(KnownTypes.SocialSecuritySerial, matchLog.SocialSecuritySerial);
            }

            var stopwatch = new System.Diagnostics.Stopwatch();
            try
            {
                stopwatch.Start();
                var model = await PatientService.MatchAsync(new Patients.PatientMatchingRequest
                {
                    AddressLine1 = matchLog.Address1,
                    AddressLine2 = matchLog.Address2,
                    Birthdate = matchLog.Birthdate,
                    City = matchLog.City,
                    FirstName = matchLog.FirstName,
                    Gender = matchLog.Gender,
                    LastName = matchLog.LastName,
                    MemberId = CurrentUser.Id,
                    MiddleName = matchLog.MiddleName,
                    OrganizationId = matchLog.FacilityId,
                    PostalCode = matchLog.Zip,
                    StateOrProvince = matchLog.State,
                    Ids = identifiers.ToList<IIdentifier>(),
                    Ruleset = Patients.MatchingRuleset.DocumentProcessing,
                    LogMatches = false,
                    ManualReviewEnabled = true
                });

                var goldenTickets = model.MatchResults.Where(r => r.Rules.OfType<GoldenTicketRuleResult<SutureHealth.Patients.Patient>>().Any());
                var scoredResults = model.MatchResults.Where(r => r.Score > -20).Take(30);
                return PartialView("_PatientMatchResults", goldenTickets.Union(scoredResults).OrderByDescending(mr => mr.Score));
            }
            finally
            {
                stopwatch.Stop();
                Logger.LogDebug($"Patient match took {stopwatch.Elapsed.Humanize()} to complete.");
            }
        }

        [HttpGet]
        [Route("Search", Name = "AdminPatientSearch")]
        public IActionResult Search()
        {
            return View(new SearchModel()
            {
                CurrentUser = CurrentUser,
                AddDialog = new AddEntityToOrganizationDialogModel()
                {
                    DialogName = "AddPatientDialog",
                    DialogTitle = "Add Patient",
                    ConfirmButtonLabel = "Add Patient",
                    ConfirmFunctionName = "AddPatientDialogConfirm",
                    OrganizationFieldName = "AddPatientOrganization",
                    OrganizationDataSourceUrl = Url.RouteUrl("AdminPatientOrganizationsDataSource")
                }
            });
        }

        [HttpPost]
        [Route("Search", Name = "AdminPatientPatientsDataSource")]
        public async Task<IActionResult> PatientsDataSource([DataSourceRequest] DataSourceRequest request)
        {
            var query = PatientService.QueryPatients();

            request.Filters.Transform("DateOfBirth", "Birthdate");

            if (request.Filters.GetFilterDescriptor("OrganizationIds") is FilterDescriptor organizationFilter)
            {
                request.Filters.Remove("OrganizationIds");
                if (int.TryParse(organizationFilter.Value.ToString(), out var filterValue))
                {
                    query = query.Where(p => p.OrganizationKeys.Any(k => k.OrganizationId == filterValue));
                }
            }
            if (request.Filters.GetFilterDescriptor("PatientRecordNumbers") is FilterDescriptor recordNumberFilter)
            {
                request.Filters.Remove("PatientRecordNumbers");
                query = query.Where(p => p.OrganizationKeys.Any(k => k.MedicalRecordNumber.Contains(recordNumberFilter.Value.ToString())));
            }
            if (request.Filters.GetFilterDescriptor("SocialSecurityNumber") is FilterDescriptor socialSecurityNumberFilter)
            {
                var searchValue = System.Text.RegularExpressions.Regex.Replace(socialSecurityNumberFilter.Value.ToString(), @"[^0-9]+", string.Empty);

                request.Filters.Remove("SocialSecurityNumber");
                query = query.Where(p => p.Identifiers.Any(id => (id.Type == KnownTypes.SocialSecurityNumber || id.Type == KnownTypes.SocialSecuritySerial) && id.Value.Contains(searchValue)));
            }

            request.Sorts.Transform("DateOfBirth", sd => sd.Member = "Birthdate");
            request.Sorts.Transform("DateCreatedTicks", sd => sd.Member = "CreatedAt");
            request.Sorts.Transform("DateModifiedTicks", sd => sd.Member = "UpdatedAt");

            return Json(await query.ToDataSourceResultAsync(request, p => new PatientListItem()
            {
                PatientId = p.PatientId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DateOfBirth = p.Birthdate,
                DateOfBirthDisplay = p.Birthdate.ToString("d"),
                SocialSecurityNumber = p.Identifiers.Where(id => id.Type == KnownTypes.SocialSecurityNumber || id.Type == KnownTypes.SocialSecuritySerial)
                                                    .OrderBy(id => id.Type == KnownTypes.SocialSecuritySerial)
                                                    .FirstOrDefault()?.Value ?? string.Empty,
                OrganizationIds = string.Join("<br />", p.OrganizationKeys.Select(k => k.OrganizationId)),
                PatientRecordNumbers = string.Join("<br />", p.OrganizationKeys.Where(k => !string.IsNullOrWhiteSpace(k.MedicalRecordNumber))
                                                                               .Select(k => $"[{k.OrganizationId}]: {k.MedicalRecordNumber}")),
                DateCreated = p.CreatedAt?.ToString("d") ?? string.Empty,
                DateCreatedTicks = p.CreatedAt?.Ticks ?? 0,
                DateModified = p.UpdatedAt?.ToString("d") ?? string.Empty,
                DateModifiedTicks = p.UpdatedAt?.Ticks ?? 0,
                IsActive = p.IsActive,
                DetailUrl = Url.RouteUrl("AdminPatientPatientDetail", new { patientId = p.PatientId })
            }));
        }

        [HttpGet]
        [Route("{patientId:int}/SearchDetail", Name = "AdminPatientPatientDetail")]
        public async Task<IActionResult> PatientDetail(int patientId)
        {
            var patient = await PatientService.GetByIdAsync(patientId);
            var organizations = (from ok in patient.OrganizationKeys
                                 join o in SecurityService.GetOrganizations() on ok.OrganizationId equals o.OrganizationId
                                 select o).ToDictionary(o => o.OrganizationId);
            if (patient == null)
            {
                return NotFound();
            }

            return PartialView("_PatientDetail", new PatientDetailModel()
            {
                GridName = $"PatientDetail-{patientId}",
                Organizations = patient.OrganizationKeys.Select(ok => new PatientDetailModel.PatientDetailListItem()
                {
                    OrganizationId = ok.OrganizationId,
                    Name = organizations.GetValueOrDefault(ok.OrganizationId)?.Name ?? "Unknown",
                    IsActive = ok.IsActive,
                    RecordNumber = ok.MedicalRecordNumber,
                    EditPatientActionUrl = Url.RouteUrl("OrganizationPatientEdit", new { organizationId = ok.OrganizationId, patientId = patientId })
                }).OrderBy(o => o.Name),
                AddOrganizationDialog = new AddEntityToOrganizationDialogModel()
                {
                    DialogName = $"AddOrganization_{patientId}",
                    DialogTitle = "Add Patient to Organization",
                    OrganizationFieldName = $"AddPatientOrganization-{patientId}",
                    ConfirmButtonLabel = "Edit Patient",
                    OrganizationDataSourceUrl = Url.RouteUrl("AdminPatientAddOrganizationDataSource", new { patientId = patientId }),
                    ConfirmFunctionName = $"AddPatientDialogConfirm_{patientId}"
                }
            });
        }

        [HttpPost]
        [Route("Search/Organizations", Name = "AdminPatientOrganizationsDataSource")]
        [Route("{patientId:int}/Search/Organizations", Name = "AdminPatientAddOrganizationDataSource")]
        public async Task<IActionResult> OrganizationsDataSource([DataSourceRequest] DataSourceRequest request, [FromRoute] int? patientId = null)
        {
            var searchValue = request?.Filters?.OfType<FilterDescriptor>().FirstOrDefault()?.Value.ToString() ?? string.Empty;
            var organizations = await SecurityService.GetOrganizationsByName(searchValue).ToArrayAsync();
            return Json(new DataSourceResult()
            {
                Total = organizations.Length,
                Data = organizations.Select(o => patientId.HasValue ?
                                                 new PatientDetailModel.OrganizationListItem()
                                                 {
                                                     OrganizationId = o.OrganizationId,
                                                     Name = $"{o.Name}{(!string.IsNullOrWhiteSpace(o.OtherDesignation) && !string.Equals(o.Name, o.OtherDesignation, StringComparison.InvariantCultureIgnoreCase) ? $" ({o.OtherDesignation})" : string.Empty)}",
                                                     EditPatientUrl = Url.RouteUrl("OrganizationPatientEdit", new { organizationId = o.OrganizationId, patientId = patientId.Value })
                                                 } :
                                                 new SearchModel.OrganizationListItem()
                                                 {
                                                     OrganizationId = o.OrganizationId,
                                                     Name = $"{o.Name}{(!string.IsNullOrWhiteSpace(o.OtherDesignation) && !string.Equals(o.Name, o.OtherDesignation, StringComparison.InvariantCultureIgnoreCase) ? $" ({o.OtherDesignation})" : string.Empty)}",
                                                     CreatePatientUrl = Url.RouteUrl("OrganizationPatientCreate", new { organizationId = o.OrganizationId })
                                                 } as OrganizationSearchListItem)
            });
        }
    }
}
