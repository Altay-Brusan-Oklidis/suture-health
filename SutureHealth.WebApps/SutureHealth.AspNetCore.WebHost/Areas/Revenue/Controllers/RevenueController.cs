using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SutureHealth.AspNetCore.Areas.Revenue.Models;
using SutureHealth.Revenue;
using SutureHealth.Revenue.Services;
using SutureHealth.Application.Services;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Revenue.Controllers
{
    [Area("Revenue")]
    [Route("Revenue")]
    public class RevenueController : Controller
    {
        private const string STATEMENT_DATE_FORMAT = "MMMM yyyy";

        protected IRevenueService ReportingService { get; }
        protected IApplicationService SecurityService { get; }

        public RevenueController
        (
            IRevenueService reportingService,
            IApplicationService securityService
        )
        {
            ReportingService = reportingService;
            SecurityService = securityService;
        }

        [HttpGet]
        [Route("", Name = "RevenueIndex")]
        public async Task<IActionResult> Index(bool contentOnly = false)
        {
            return View(new IndexViewModel()
            {
                CurrentUser = CurrentUser,
                RequireKendo = true,
                RequireUI = true,
                Statements = Enumerable.Range(0, 12).Select(i =>
                {
                    var value = DateTime.Now.AddMonths(-i).ToString(STATEMENT_DATE_FORMAT);
                    return new SelectListItem(value, value);
                }),
                Providers = (await ReportingService.GetMembersForRevenueAsync(CurrentUser.Id))
                                                   .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
                                                   .Select(m => new SelectListItem($"{m.LastName}, {m.FirstName}{(string.IsNullOrWhiteSpace(m.Suffix) ? string.Empty : $" {m.Suffix}")}", m.MemberId.ToString())),
                Offices = await SecurityService.GetOrganizationMembersByMemberId(CurrentUser.Id)
                                                .Select(om => om.Organization)
                                                .OrderBy(o => o.OtherDesignation).ThenBy(o => o.StateOrProvince)
                                                .Select(o => new SelectListItem($"{o.OtherDesignation}, {o.StateOrProvince}", o.OrganizationId.ToString(), o.OrganizationId == CurrentUser.CurrentOrganizationId))
                                                .ToArrayAsync(),
                RequireClientHeader = !contentOnly
            });
        }

        [HttpGet]
        [Route("Metrics", Name = "RevenueMetrics")]
        public async Task<IActionResult> Metrics(int facilityId, int signerId)
        {
            var metrics = await ReportingService.GetRevenueMetricsReportAsync(new RevenueMetricsReportRequest()
            {
                MemberId = CurrentUser.Id,
                SignerMemberId = signerId,
                SignerOrganizationId = facilityId
            });

            return PartialView("_Metrics", new MetricsViewModel()
            {
                IsPaidUser = CurrentUser.IsPayingClient,
                AllTimeRevenue = metrics.AllTimeRevenue,
                AllTimeRVU = metrics.AllTimeRVU,
                YearToDateRevenue = metrics.LastYearRevenue,
                YearToDateRVU = metrics.LastYearRVU,
                Last30DaysRevenue = metrics.Last30DaysRevenue,
                Last30DaysRVU = metrics.Last30DaysRVU
            });
        }

        [HttpPost]
        [Route("Grid", Name = "RevenueGrid")]
        public async Task<IActionResult> Grid([FromBody] GridPostModel request)
        {
            var reportRequest = new RevenueReportRequest()
            {
                MemberId = CurrentUser.Id,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortExpression = request.SortExpr,
                SortDirection = request.SortDir,
                PatientId = request.PatientId,
                SignerMemberId = request.Signer,
                SignerOrganizationId = request.SignerFacilityId,
                EffectiveStartDate = !string.IsNullOrWhiteSpace(request.EffectiveStartDate) ? DateTime.Parse(request.EffectiveStartDate) : null,
                EffectiveEndDate = !string.IsNullOrWhiteSpace(request.EffectiveEndDate) ? DateTime.Parse(request.EffectiveEndDate) : null
            };
            var data = null as RevenueReportResponse;
            var model = new GridJsonModel();

            #region Bind Request Dates
            if (!string.IsNullOrEmpty(request.Statement) && request.Statement != "0")
            {
                DateTime filterDate;
                if (DateTime.TryParseExact(request.Statement, STATEMENT_DATE_FORMAT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out filterDate))
                {
                    reportRequest.ServiceStartDate = new DateTime(filterDate.Year, filterDate.Month, 1);
                    reportRequest.ServiceEndDate = new DateTime(filterDate.Year, filterDate.Month, DateTime.DaysInMonth(filterDate.Year, filterDate.Month)).AddDays(1);
                }
            }
            else
            {
                //start DG var userMonthLimit = CurrentUser.IsPaid == true ? 11 : 1;
                var userMonthLimit = 11;
                //end DG
                var today = DateTime.Now;
                var priviousMonth = DateTime.Now.AddMonths(-userMonthLimit);
                reportRequest.ServiceEndDate = today.AddDays(1);
                reportRequest.ServiceStartDate = new DateTime(priviousMonth.Year, priviousMonth.Month, 1);
            }

            if (!string.IsNullOrEmpty(request.ServiceStartDate) && Convert.ToDateTime(request.ServiceStartDate) >= reportRequest.ServiceStartDate)
            {
                reportRequest.ServiceStartDate = Convert.ToDateTime(request.ServiceStartDate);
            }

            if (!string.IsNullOrEmpty(request.ServiceEndDate) && Convert.ToDateTime(request.ServiceEndDate) <= reportRequest.ServiceEndDate)
            {
                reportRequest.ServiceEndDate = Convert.ToDateTime(request.ServiceEndDate);
            }
            #endregion
            #region Bind Data Model
            data = await ReportingService.GetRevenueReportAsync(reportRequest);
            model.Requests = data.Items.Select(r => new GridJsonModel.Request()
            {
                BillingCode = r.BillingCode,
                DiagnosisCode = r.DiagnosisCode,
                DocumentType = r.DocumentType,
                EffectiveDate = r.EffectiveDate,
                FormId = r.FormId,
                ProviderSuffix = r.ProviderSuffix,
                Last4SSN = r.Last4SSN,
                Medicaid = Convert.ToInt32(r.Medicaid),
                Medicare = Convert.ToInt32(r.Medicare),
                MedicareAdvantage = Convert.ToInt32(r.MedicareAdvantage),
                PatientDOB = r.PatientDOB,
                PatientFirstName = r.PatientFirstName,
                PatientLastName = r.PatientLastName,
                PatientSuffix = r.PatientSuffix,
                PlaceOfService = r.PlaceOfService,
                Practice = r.Practice,
                PracticeNPI = r.PracticeNPI,
                PrivateInsurance = Convert.ToInt32(r.PrivateInsurance),
                ProviderCredential = r.ProviderCredential,
                ProviderFirstName = r.ProviderFirstName,
                ProviderLastName = r.ProviderLastName,
                ProviderNPI = r.ProviderNPI,
                ReferringProvider = r.ReferringProvider,
                ReferringProviderMedicare = r.ReferringProviderMedicare,
                ReferringProviderNPI = r.ReferringProviderNPI,
                Revenue = r.Revenue,
                RVU = r.RVU,
                SelfPay = Convert.ToInt32(r.SelfPay),
                ServiceDate = r.ServiceDate,
                Total = r.Total,
                PatientDisplayName = ($"{r.PatientLastName}, {r.PatientFirstName} {r.PatientSuffix}").Trim(),
                ProviderDisplayName = ($"{r.ProviderLastName}, {r.ProviderFirstName} {r.ProviderSuffix}").Trim(),
                PdfUrl = Url.Page("/ViewPdf", new { area = "Request", requestId = r.FormId }),
                Payer = string.Join(", ", (new KeyValuePair<string, bool>[] {
                    new KeyValuePair<string, bool>("Medicaid", r.Medicaid),
                    new KeyValuePair<string, bool>("Medicare", r.Medicare),
                    new KeyValuePair<string, bool>("MedicareAdvantage", r.MedicareAdvantage),
                    new KeyValuePair<string, bool>("PrivateInsurance", r.PrivateInsurance),
                    new KeyValuePair<string, bool>("SelfPay", r.SelfPay)
                }).Where(kvp => kvp.Value).Select(kvp => kvp.Key).DefaultIfEmpty("unavailable"))
            });
            if (model.Requests.Any())
            {
                model.CurrentReportRevenue = data.Revenue.ToString("n0");
                model.CurrentReportRVU = $"({data.RVU} RVU)";
                model.TotalRecords = model.Requests.First().Total;
            }
            else
            {
                model.CurrentReportRevenue = "0";
                model.CurrentReportRVU = $"(0 RVU)";
            }
            #endregion

            return Json(model);
        }
    }
}
