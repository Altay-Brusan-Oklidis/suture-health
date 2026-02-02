using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.Notifications.Services;
using SutureHealth.Visits.Services;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.WebHost.Areas.Visit.Models;
using Microsoft.EntityFrameworkCore;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Controllers
{
    [Route("Visit")]
    [Area("Visit")]
    public class BaseVisitController : Controller
    {
        protected IApplicationService ApplicationService { get; set; }
        protected ILogger Log { get; set; }
        protected INotificationService NotificationService { get; set; }
        protected IVisitServicesProvider VisitService { get; private set; }

        protected string TwilioAccountSid { get; set; }
        protected string TwilioApiKey { get; set; }
        protected string TwilioApiSecret { get; set; }

        public BaseVisitController
        (
            IApplicationService applicationService,
            IVisitServicesProvider visitService,
            INotificationService notificationService,
            ILogger logger,
            IConfiguration configuration
        )
        {
            ApplicationService = applicationService;
            Log = logger;
            NotificationService = notificationService;
            VisitService = visitService;

            TwilioAccountSid = configuration["SutureHealth:NotificationServicesProvider:Providers:Twilio:AccountSid"];
            TwilioApiKey = configuration["SutureHealth:NotificationServicesProvider:Providers:Twilio:ApiKey"];
            TwilioApiSecret = configuration["SutureHealth:NotificationServicesProvider:Providers:Twilio:ApiSecret"];
        }

        protected async Task<CheckForVideoVisitResponse> CheckForVideoVisitAsync(string publicId = null)
        {
            var response = new CheckForVideoVisitResponse()
            {
                Visit = await VisitService.GetVisitByPublicIdAsync(publicId)
            };

            if (response.VisitFound && !response.Visit.Active)
            {
                response.ErrorAction = RedirectToRoute("VisitExpiredError", new { publicId });
            }
            if (!response.VisitFound)
            {
                response.ErrorAction = RedirectToRoute("VisitRoomNotFoundError");
            }

            return response;
        }

        protected async System.Threading.Tasks.Task SendHostMessageAsync(SutureHealth.Visits.Core.Visit videoVisit, string subject, string message)
        {
            try
            {
                var user = await ApplicationService.GetMemberByIdAsync(videoVisit.HostUserId);
                var hostPhoneNumber = user.Contacts?.FirstOrDefault(c => c.Type == ContactType.Mobile)?.Value;

                if (string.IsNullOrEmpty(hostPhoneNumber) == false)
                {
                    hostPhoneNumber = Regex.Replace(hostPhoneNumber, "\\D", string.Empty);
                    await NotificationService.CreateNotificationAsync
                        (
                            SutureHealth.Notifications.Channel.Sms,
                            subject,
                            $"tel:{hostPhoneNumber}",
                            null,
                            null,
                            null,
                            message,
                            new Uri("https://www.google.com"),
                            videoVisit.VideoVisitId.ToString()
                        );
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Exception occurred in SendHostMessage()");
            }
        }

        protected async System.Threading.Tasks.Task SendPatientMessageAsync(SutureHealth.Visits.Core.Visit videoVisit, string subject = null, string message = null)
        {
            var hostingOrganizationMember = await ApplicationService.GetOrganizationMembersByMemberId(videoVisit.HostUserId)
                                                                    .Where(om => om.IsActive)
                                                                    .OrderByDescending(om => om.IsPrimary)
                                                                    .FirstAsync();
            var hostingOrganization = hostingOrganizationMember.Organization;
            try
            {
                var link = Url.RouteUrl("VisitInviteGet", new { publicId = videoVisit.PublicId }, "https");
                if (string.IsNullOrEmpty(message))
                {
                    message = $"{videoVisit.HostName} from {hostingOrganization.Name} is waiting for you.\n\nClick the link to join the video call now.\n{link}";
                }

                if (string.IsNullOrEmpty(subject))
                {
                    subject = "Video Visit Invite";
                }

                await NotificationService.CreateNotificationAsync
                (
                    SutureHealth.Notifications.Channel.Sms,
                    subject,
                    $"tel:{videoVisit.ParticipantPhoneNumber}",
                    null,
                    null,
                    null,
                    message,
                    new Uri(Url.RouteUrl("VisitNotificationCallBack", new { publicId = videoVisit.PublicId }, "https")),
                    videoVisit.VideoVisitId.ToString()
                );
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Exception occurred in SendPatientMessage()");
            }
        }

        protected async System.Threading.Tasks.Task PatientCallKickoff(SutureHealth.Visits.Core.Visit videoVisit, int minutesDelay = 0)
        {
            await NotificationService.CreateNotificationAsync
            (
                SutureHealth.Notifications.Channel.TextToSpeech,
                "Patient Call Kickoff",
                $"tel:{videoVisit.ParticipantPhoneNumber}",
                null,
                minutesDelay > 0 ? DateTime.UtcNow.AddMinutes(minutesDelay) : null,
                new Uri(Url.RouteUrl("VisitPatientCall", new { publicId = videoVisit.PublicId }, "https")),
                null,
                new Uri(Url.RouteUrl("VisitPatientCallStatusCallback", new { publicId = videoVisit.PublicId }, "https")),
                Guid.NewGuid().ToString()
            );
        }
    }

    public static class VisitExtensions
    {
        public static string ParticipantPhoneNumberFormatted(this SutureHealth.Visits.Core.Visit visit)
            => long.Parse(visit.ParticipantPhoneNumber).ToString("(###) ###-####");

        public static string HostSupportPhoneNumberFormatted(this SutureHealth.Visits.Core.Visit visit)
            => long.Parse(visit.HostSupportPhoneNumber).ToString("(###) ###-####");
    }
}
