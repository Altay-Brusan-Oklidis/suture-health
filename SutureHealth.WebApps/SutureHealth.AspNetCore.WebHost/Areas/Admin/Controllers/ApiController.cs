using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Newtonsoft.Json;
using SutureHealth.AspNetCore.Areas.Admin.Models.Api;
using SutureHealth.Requests.Services;
using SutureHealth.Application.Services;
using Microsoft.EntityFrameworkCore;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/API")]
    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    public class ApiController : Controller
    {
        protected IRequestServicesProvider RequestService { get; }
        protected IApplicationService SecurityService { get; }

        public ApiController
        (
            IRequestServicesProvider requestService,
            IApplicationService securityService
        )
        {
            RequestService = requestService;
            SecurityService = securityService;
        }

        [HttpGet]
        [Route("Search", Name = "AdminApiSearch")]
        public IActionResult Search()
        {
            return View(new SearchModel()
            {
                CurrentUser = CurrentUser
            });
        }

        [HttpPost]
        [Route("Search/Grid", Name = "AdminApiRequestGrid")]
        public async Task<IActionResult> RequestGrid([FromBody] RequestGridSearch search)
        {
            var requests = search.OrganizationId.HasValue ?
                                (await RequestService.GetTransmittedRequestsAsync(search.OrganizationId.Value)).Where(r => r.Status == search.Status.GetValueOrDefault(Requests.WorkflowStatus.Submitted)) :
                                 await RequestService.GetTransmittedRequestsAsync(search.Status.GetValueOrDefault(Requests.WorkflowStatus.Submitted));
            var organizations = await SecurityService.GetOrganizationsByIdAsync(requests.Select(r => r.OrganizationId).Distinct().ToArray()).ToDictionaryAsync(o => o.OrganizationId);

            return Json(requests.Select(request =>
            {
                var senderOrganization = organizations.GetValueOrDefault(request.OrganizationId);
                var signer = request.Signers.FirstOrDefault();
                var patient = request.Patients.FirstOrDefault();

                return new RequestGridListItem()
                {
                    RequestId = request.TransmittedRequestId,
                    DateSubmitted = request.SubmittedAt.UtcToSutureDateTime().ToString("g"),
                    SendingOrganization = senderOrganization?.Name ?? string.Empty,
                    SigningOrganization = signer?.Organization?.Name ?? string.Empty,
                    SignerName = (new string[] { signer?.FirstName, signer?.LastName }).All(n => !string.IsNullOrWhiteSpace(n)) ?
                                    $"{signer.FirstName} {signer.LastName} {signer.ProfessionalSuffix}" :
                                    signer?.NPI.ToString(),
                    PatientName = $"{patient?.FirstName} {patient?.LastName}",
                    Status = request.Status.ToString(),
                    RequestJson = request == null ? "{}" : JsonConvert.SerializeObject(request, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    PatientMatchJson = patient?.MatchLogs == null ? "[]" : JsonConvert.SerializeObject(patient.MatchLogs, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                };
            }));
        }

        [HttpPost]
        [Route("Search/Submitter", Name = "AdminApiSubmittingOrganizationSearch")]
        public async Task<IActionResult> SubmittingOrganizationSearch([DataSourceRequest] DataSourceRequest request)
        {
            var search = request.Filters.OfType<FilterDescriptor>().FirstOrDefault(f => f.Member == "Name")?.Value.ToString();
            return Json(new DataSourceResult
            {
                Data = await SecurityService.GetOrganizationsByName(search)
                                            .Select(o => new OrganizationSearchListItem()
                                            {
                                                OrganizationId = o.OrganizationId,
                                                Name = o.Name
                                            })
                                            .ToArrayAsync()
            });
        }
    }
}
