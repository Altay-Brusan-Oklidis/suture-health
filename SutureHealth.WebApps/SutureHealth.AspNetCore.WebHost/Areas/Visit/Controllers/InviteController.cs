using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SutureHealth.Notifications.Services;
using SutureHealth.Visits.Services;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.WebHost.Areas.Visit.Models;

namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Controllers
{
    public class InviteController : BaseVisitController
    {
        public InviteController
        (
            IVisitServicesProvider visitService,
            IApplicationService securityService,
            INotificationService notificationService,
            ILogger<InviteController> logger,
            IConfiguration configuration
        ) : base(securityService, visitService, notificationService, logger, configuration) { }

        [HttpGet]
        [AllowAnonymous]
        [Route("{publicId}/Invite", Name = "VisitInviteGet")]
        public async Task<IActionResult> Invite([FromRoute] string publicId)
        {
            var result = await CheckForVideoVisitAsync(publicId);
            if (result.ErrorAction != null)
            {
                return result.ErrorAction;
            }
            
            return View(new InviteViewModel()
            {
                PublicId = publicId,
                HostName = result.Visit.HostName,
                HostSupportPhoneNumber = result.Visit.HostSupportPhoneNumber,
                HostSupportPhoneNumberFormatted = result.Visit.HostSupportPhoneNumberFormatted()
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("{publicId}/Invite", Name = "VisitInvitePost")]
        public async Task<IActionResult> InvitePost([FromRoute] string publicId, string participantName, bool? virtualVisitConsented)
        {
            var result = await CheckForVideoVisitAsync(publicId);
            if (result.ErrorAction != null)
            {
                return result.ErrorAction;
            }

            await SendHostMessageAsync(result.Visit,
                "Patient Joined Video Visit",
                $"Your patient, { (!string.IsNullOrWhiteSpace(participantName) ? participantName : string.Empty) } { result.Visit.ParticipantPhoneNumberFormatted() }, just joined your video visit.\n\nClick the link to join now.\n" +
                    $"{Url.RouteUrl("VisitHostJoin", new { publicId = publicId }, "https")}");

            return RedirectToRoute("VisitRoom", new { publicId, name = participantName });
        }

        [HttpPost]
        [Route("{publicId}/Invite/Send", Name = "VisitResendInvite")]
        public async Task<IActionResult> ResendInvite(string publicId)
        {
            var result = await CheckForVideoVisitAsync(publicId);
            
            if (result.VisitFound)
            {
                await SendPatientMessageAsync(result.Visit);
            }

            return Ok();
        }
    }
}
