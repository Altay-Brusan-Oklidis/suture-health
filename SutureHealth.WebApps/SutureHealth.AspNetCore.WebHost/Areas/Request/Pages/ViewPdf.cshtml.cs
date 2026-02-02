using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Requests.Services;

namespace SutureHealth.AspNetCore.Areas.Request.Pages
{
    public class ViewPdfModel : BasePageModel
    {
        public string PdfUrl { get; set; }
        public string PdfDataHandlerUrl { get; set; }

        IRequestServicesProvider RequestService { get; }
        IApplicationService ApplicationService { get; }

        public ViewPdfModel
        (
            IRequestServicesProvider requestService,
            IApplicationService applicationService
        )
        {
            RequestService = requestService;
            ApplicationService = applicationService;
        }

        public async Task<IActionResult> OnGet(int requestId)
        {
            var request = await RequestService.GetServiceableRequests().FirstOrDefaultAsync(r => r.SutureSignRequestId == requestId);
            if (request != null)
            {
                if (!CurrentUser.IsApplicationAdministrator())
                {
                    var userOrganizationIds = await ApplicationService.GetOrganizationMembersByMemberId(CurrentUser.Id)
                                                                      .Select(om => om.OrganizationId)
                                                                      .ToArrayAsync();
                    if (!(new int[] { request.SignerOrganizationId, request.SubmitterOrganizationId }).Join(userOrganizationIds, id => id, id => id, (rid, uid) => uid).Any())
                    {
                        return Unauthorized();
                    }

                    await RequestService.MarkRequestViewedAsync(CurrentUser.Id, requestId);
                }

                PdfUrl = Url.RouteUrl("DownloadRequest", new { requestId = requestId });
                PdfDataHandlerUrl = Url.Page("/ViewPdf", "Pdf", new { area = "Request", requestId = requestId });

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnGetPdf(int requestId)
        {
            var pdf = (await RequestService.GetServiceableRequestPdfByIdAsync(requestId)).Select(kvp => kvp.Value).FirstOrDefault();
            if (pdf != null)
            {
                if (!CurrentUser.IsApplicationAdministrator())
                {
                    var userOrganizationIds = await ApplicationService.GetOrganizationMembersByMemberId(CurrentUser.Id).Select(om => om.OrganizationId)
                                                                                                                       .ToArrayAsync();
                    var request = await RequestService.GetServiceableRequests().FirstAsync(r => r.SutureSignRequestId == requestId);
                    if (!(new int[] { request.SignerOrganizationId, request.SubmitterOrganizationId }).Join(userOrganizationIds, id => id, id => id, (rid, uid) => uid).Any())
                    {
                        return Unauthorized();
                    }
                }

                return File(pdf, "application/pdf");
            }

            return NotFound();
        }
    }
}