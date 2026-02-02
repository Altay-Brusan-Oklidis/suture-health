using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutureHealth.AspNetCore.Areas.Request.Models.Sign;
using SutureHealth.Requests.Services;
using SutureHealth.AspNetCore.WebHost;
using SutureHealth.Reporting.Services;
using SutureHealth.Application.Services;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Request.Controllers
{
    [Area("Request")]
    [Route("Request/Sign")]
    public class SignController : Controller
    {
        [HttpGet]
        [Route("", Name = "SignIndex")]
        public IActionResult Index
        (
            [FromServices] IApplicationService applicationService,
            [FromServices] IConfiguration configuration
        )
        {
            bool? nextEnabled = false;
            var inboxPreference = applicationService.GetHierarchicalSetting("InboxPreference", CurrentUser.MemberId)?.ItemInt;

            if (inboxPreference == 1)
                nextEnabled = true;

            //var nextEnabled = applicationService.GetHierarchicalSetting("Next.Enabled", CurrentUser.MemberId)?.ItemBool;

            // Users who log in with Duo SSO SAML2 can use only the new inbox
            if (User.Identity.AuthenticationType == "AuthenticationTypes.Federation")
                nextEnabled = true;
#if DEBUG
            return Ok($"DEBUG: Signer inbox (Next.Enabled: {nextEnabled.GetValueOrDefault()})");
#else
            if (!string.IsNullOrWhiteSpace(configuration["SutureHealth:NextBaseUri"]) && nextEnabled.GetValueOrDefault())
            {
                return Redirect($"{configuration["SutureHealth:NextBaseUri"]}/request/sign");
            }
            else
            {
                return Redirect("~/UserArea/ModifyRequest.aspx");
            }
#endif
        }

        [EnableCors(Startup.CorsLegacyPolicy)]
        [HttpGet("Badge", Name = "SignerInboxCount")]
        [IgnoreAntiforgeryToken]
        [IgnoreLoginHandler]
        [Route("/revenue/badge")]
        public async Task<IActionResult> Badge
        (
            [FromServices] IRequestServicesProvider requestService
        ) =>
            Json(new BadgeJsonModel()
            {
                PendingDocumentCount = await requestService.GetInboxBadgeCountAsync(CurrentUser.Id)
            });

        // NOTE: This method can be deprecated once legacy Inbox is shutdown
        [EnableCors(Startup.CorsLegacyPolicy)]
        [HttpPost("/Request/{requestId:int}/Help", Name = "RequestSignSendHelpRequest")]
        public async Task<IActionResult> SendHelpRequest
        (
            [FromServices] IInboxServicesProvider inboxService,
            //[FromServices] IDeliveryService deliveryService,
            //[FromServices] IRequestServicesProvider requestService,
            [FromServices] IApplicationService applicationService,
            [FromRoute] int requestId,
            [FromBody] SendHelpRequest helpRequest
        )
        {
            var request = inboxService.GetRequests(CurrentUser.MemberId, CurrentUser.IsUserPhysician(),
                    CurrentUser.CanSign,
                    new[] { "Template", "Template.TemplateType" })
                // .Include(r => r.Template).ThenInclude(t => t.TemplateType)
                .FirstOrDefault(r => r.SutureSignRequestId == requestId);

            if (request == null || helpRequest == null ||
                !(await applicationService.GetSubordinatesForMemberId(request.SignerMemberId).AnyAsync(r => r.SubordinateMemberId == helpRequest.AssistantMemberId)))
            {
                return BadRequest();
            }

            /*
             * Additional dev is needed before this endpoint can offer correct functionality:
             * - The endpoint is active because legacy currently calls it.
             * - Disable the generation of the 526 task on the legacy side or don't MarkRequestAssistanceRequested here.
             * - The email itself needs to be validated.
            await Task.WhenAll
            (
                deliveryService.SendRequestForAssistanceEmailAsync(request.SignerMemberId, helpRequest.AssistantMemberId, request.Template.TemplateType.Name)
                requestService.MarkRequestAssistanceRequestedAsync(CurrentUser.Id, helpRequest.AssistantMemberId, request.SutureSignRequestId)
            );
            */

            return Ok();
        }
    }
}
