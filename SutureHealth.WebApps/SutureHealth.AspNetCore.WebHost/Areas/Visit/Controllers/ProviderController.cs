using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Twilio;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using SutureHealth.Notifications.Services;
using SutureHealth.Visits.Services;
using SutureHealth.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Controllers
{
    [AllowAnonymous]
    public class ProviderController : BaseVisitController
    {
        public ProviderController
        (
            IApplicationService securityService,
            IVisitServicesProvider visitService,
            INotificationService notificationService,
            ILogger<ProviderController> logger,
            IConfiguration configuration
        ) : base(securityService, visitService, notificationService, logger, configuration) { }

        [HttpPost]
        [Route("{publicId}/Provider/NotificationCallBack", Name = "VisitNotificationCallBack")]
        public async Task<IActionResult> NotificationCallBack(string publicId, [FromBody] SutureHealth.Notifications.NotificationStatus notification)
        {
            var visit = await VisitService.GetVisitByPublicIdAsync(publicId);

            Log.LogInformation($"NotificationCallBack: Notification Callback  for publicId : {publicId}");

            if (visit == null)
            {
                Log.LogWarning($"NotificationCallback: Did not find visit for publicId: {publicId}");
                return Ok();
            }

            if (notification != null)
            {
                if (notification.Success.HasValue)
                {
                    var messagePairs = new Dictionary<string, string> {
                        {"is not a valid phone number", "Invalid phone number." },
                        {"The message has failed", "Unknown error occurred." },
                        {"violates a blacklist rule", "Recipient is blocking texts from Suture Health." }
                    };

                    Log.LogInformation($"NotificationCallBack: Get success response for publicId : {publicId}");

                    await VisitService.LogStatusAsync(publicId, notification.Success.Value, !notification.Success.Value ?
                                                                                                messagePairs.Where(mp => string.Equals(mp.Key, notification.Message, StringComparison.OrdinalIgnoreCase)).Select(mp => mp.Value).DefaultIfEmpty(notification.Message).First() :
                                                                                                null);
                }
            }
            else
            {
                Log.LogWarning($"NotificationCallBack: Could not find input stream for publicId : {publicId}");
            }

            return Ok();
        }

        [HttpPost]
        [Route("{publicId}/Provider/PatientCallStatusCallback", Name = "VisitPatientCallStatusCallback")]
        public async Task<IActionResult> PatientCallStatusCallback(string publicId, [FromBody] SutureHealth.Notifications.NotificationStatus notification)
        {
            var videoVisit = await VisitService.GetVisitByPublicIdAsync(publicId);
            if (videoVisit == null)
            {
                Log.LogWarning($"PatientCallStatusCallback: Did not find visit for publicId: {publicId}");

                return Ok();
            }

            if (notification != null)
            {
                if (notification.Success.HasValue && !notification.Success.Value)
                {
                    Log.LogInformation($"VideoVisit {videoVisit.VideoVisitId} - Call failed, sending text.  Call error = {notification.Message}");

                    await SendHostMessageAsync(videoVisit,
                        "Patient Notification Error",
                        $"{ videoVisit.ParticipantPhoneNumberFormatted() } did not answer our call, so we sent the invitation via text."
                        );
                    await SendPatientMessageAsync(videoVisit);

                    await VisitService.LogStatusAsync(publicId, null, (videoVisit.PatientNotificationError ?? string.Empty) + "; " + notification.Message);
                }
            }
            else
            {
                Log.LogInformation($"PatientCallStatusCallback: Could not find input stream for publicId : {publicId}");
            }

            return Ok();
        }

        [HttpGet]
        [Route("{publicId}/Provider/PatientCall", Name = "VisitPatientCall")]
        public async Task<IActionResult> PatientCall(string publicId)
        {
            var visit = await VisitService.GetVisitByPublicIdAsync(publicId);
            var hostingOrganizationMember = await ApplicationService.GetOrganizationMembersByMemberId(visit.HostUserId)
                                                                    .Where(om => om.IsActive)
                                                                    .OrderByDescending(om => om.IsPrimary)
                                                                    .FirstAsync();
            var hostingOrganization = hostingOrganizationMember.Organization;
            var hostingMember = hostingOrganizationMember.Member;
            string hostName;

            if (string.IsNullOrEmpty(hostingMember.ProfessionalSuffix))
            {
                hostName = hostingMember.SigningName;
            }
            else
            {
                hostName = visit.HostName.Replace(hostingMember.ProfessionalSuffix, string.Join(' ', hostingMember.ProfessionalSuffix.ToCharArray()));
            }

            TwilioClient.Init(TwilioApiKey, TwilioApiSecret, TwilioAccountSid);

            var response = new VoiceResponse();
            var gather =
                new Gather(
                        numDigits: 1,
                        action: new Uri(Url.RouteUrl("VisitPatientCallResponse", new { publicId = visit.PublicId }, "https")),
                        input: new List<Gather.InputEnum> { Gather.InputEnum.Dtmf },
                        hints: "0, 1, 2, 3, 4, 5"
                    )
                    .Say($"{hostName} from {hostingOrganization.Name} has invited you to a video visit. If you are ready now, press 0. If you can join in 10 minutes, press 1. If you can join in 20 minutes, press 2. If you can join in 30 minutes, press 3. If you cannot join in the next 30 minutes, press 4.  If your phone does not support video or you would like to use a different phone, press 5.")
                    ;
            response.Append(gather);
            response.Append(gather);

            // If the user doesn't enter input, loop
            response.Redirect(new Uri(Url.RouteUrl("VisitPatientCallNoResponse", new { publicId = visit.PublicId }, "https")));
            
            return Content(response.ToString(), "text/xml");
        }

        [HttpPost]
        [Route("{publicId}/Provider/PatientCallNoResponse", Name = "VisitPatientCallNoResponse")]
        public async Task<IActionResult> PatientCallNoResponse(string publicId)
        {
            var visit = await VisitService.GetVisitByPublicIdAsync(publicId);
            var hostUser = await ApplicationService.GetMemberByIdAsync(visit.HostUserId);
            string hostName;

            if (string.IsNullOrEmpty(hostUser.ProfessionalSuffix))
            {
                hostName = hostUser.SigningName;
            }
            else
            {
                hostName = visit.HostName.Replace(hostUser.ProfessionalSuffix, string.Join(' ', hostUser.ProfessionalSuffix.ToCharArray()));
            }

            var response = new VoiceResponse();
            response.Say($"Sorry, we did not get anything. You will receive a text message shortly at this number. Please click the link in that message to meet with { hostName }");
            await SendPatientMessageAsync(visit);
            await SendHostMessageAsync(visit, "No Response", $"{ visit.ParticipantPhoneNumberFormatted() } did not answer our call, so we sent the invitation via text.");

            return Content(response.ToString(), "text/xml");
        }

        [HttpPost]
        [Route("{publicId}/Provider/PatientCallResponse", Name = "VisitPatientCallResponse")]
        public async Task<ActionResult> PatientCallResponse(string digits, string publicId, string originalNumber = null)
        {
            var visit = await VisitService.GetVisitByPublicIdAsync(publicId);
            var hostUser = await ApplicationService.GetMemberByIdAsync(visit.HostUserId);
            string hostName;

            if (string.IsNullOrEmpty(hostUser.ProfessionalSuffix))
            {
                hostName = hostUser.SigningName;
            }
            else
            {
                hostName = visit.HostName.Replace(hostUser.ProfessionalSuffix, string.Join(' ', hostUser.ProfessionalSuffix.ToCharArray()));
            }

            var response = new VoiceResponse();

            // If the user entered digits, process their request
            if (!string.IsNullOrEmpty(digits))
            {
                switch (digits)
                {
                    case "0":
                        response.Say($"Thank you, you will receive a text message shortly. Please click the link in that message to meet with {hostName}");
                        if (originalNumber != null)
                        {
                            await SendHostMessageAsync(visit, "Appointment Confirmed", $"{originalNumber} has confirmed their appointment and will be joining shortly from a new number, {visit.ParticipantPhoneNumberFormatted() }, which has been updated in the system.");
                        }
                        else
                        {
                            await SendHostMessageAsync(visit, "Appointment Confirmed", $"{visit.ParticipantPhoneNumberFormatted()} has confirmed their appointment and will be joining shortly.");
                        }
                        await SendPatientMessageAsync(visit);
                        break;
                    case "1":
                    case "2":
                    case "3":
                        var minutes = (int.Parse(digits) * 10);
                        response.Say("Thank you, we will call again at the time you selected.");
                        await PatientCallKickoff(visit, minutes - 3);
                        if (originalNumber != null)
                        {
                            await SendHostMessageAsync(visit, "Appointment Confirmed for {minutes} minutes", $"{originalNumber} has confirmed they will be able to join in {minutes} minutes from a new number, {visit.ParticipantPhoneNumberFormatted() }, which has been updated in the system.");
                        }
                        else
                        {
                            await SendHostMessageAsync(visit, $"Appointment Confirmed for {minutes} minutes", $"{visit.ParticipantPhoneNumberFormatted()} has confirmed they will be able to join in {minutes} minutes.");
                        }
                        break;
                    case "4":
                        var phoneText = "";
                        if (visit.HostSupportPhoneNumber != null)
                        {
                            phoneText = "at " + string.Join(", ", visit.HostSupportPhoneNumber.ToCharArray());
                        }

                        response.Say($"Thank you. Please contact our office { phoneText } to schedule your visit.");
                        await SendPatientMessageAsync(visit, "Reschedule Appointment", $"Please call {visit.HostSupportPhoneNumberFormatted()} to schedule your visit with {visit.HostName}.");
                        await SendHostMessageAsync(visit, "Appointment Reschedule Requested", $"{visit.ParticipantPhoneNumberFormatted()} has been asked to call your office to reschedule.");
                        // Close room
                        await VisitService.DeactivateVisitAsync(visit.VideoVisitId);
                        break;
                    case "5":
                        var gather = new Gather(
                                numDigits: 10,
                                action: new Uri(Url.RouteUrl("VisitChangeNumberResponse", new { publicId = visit.PublicId }, "https")),
                                input: new List<Gather.InputEnum> { Gather.InputEnum.Dtmf },
                                timeout: 5
                            )
                            .Say($"Please enter a 10-digit number including area code.");

                        response
                            .Append(gather)
                            .Append(gather)
                            .Redirect(new Uri(Url.RouteUrl("VisitPatientCallNoResponse", new { publicId = visit.PublicId }, "https")));
                        break;
                    default:
                        response.Say("Sorry, I don't understand that choice.").Pause();
                        response.Redirect(new Uri(Url.RouteUrl("VisitPatientCall", new { publicId = visit.PublicId }, "https")));
                        break;
                }
            }
            else
            {
                // If no input was sent, redirect to the /voice route
                response.Redirect(new Uri(Url.RouteUrl("VisitPatientCall", new { publicId = visit.PublicId }, "https")));
            }

            return Content(response.ToString(), "text/xml");
        }

        [HttpPost]
        [Route("{publicId}/Provider/ChangeNumberResponse", Name = "VisitChangeNumberResponse")]
        public async Task<ActionResult> ChangeNumberResponse(string publicId, string digits = "")
        {
            var visit = await VisitService.GetVisitByPublicIdAsync(publicId);
            var response = new VoiceResponse();

            if (digits.Length == 10)
            {
                response.Append(new Gather(
                        numDigits: 1,
                        action: new Uri(Url.RouteUrl("VisitChangeNumberConfirmationResponse", new { publicId = visit.PublicId, newNumber = digits }, "https")),
                        input: new List<Gather.InputEnum> { Gather.InputEnum.Dtmf })
                    .Say($"The number you entered was { string.Join(", ", digits.ToCharArray()) }. To confirm the number, press 1. To re-enter the number, press 2.")
                    );
            }
            else if (digits.Length > 0)
            {
                response.Append(new Gather(
                        numDigits: 10,
                        action: new Uri(Url.RouteUrl("VisitChangeNumberResponse", new { publicId = visit.PublicId }, "https")),
                        input: new List<Gather.InputEnum> { Gather.InputEnum.Dtmf })
                    .Say($"Please re-enter the number, making sure to include area code so that it is exactly 10 digits.")
                    );
            }
            // If no input was sent, redirect to no reponse
            response.Append(new Redirect(new Uri(Url.RouteUrl("VisitPatientCallNoResponse", new { publicId = visit.PublicId }, "https"))));

            return Content(response.ToString(), "text/xml");
        }

        [HttpPost]
        [Route("{publicId}/Provider/ChangeNumberConfirmationResponse", Name = "VisitChangeNumberConfirmationResponse")]
        public async Task<ActionResult> ChangeNumberConfirmationResponse(string digits, string newNumber, string publicId)
        {
            var visit = await VisitService.GetVisitByPublicIdAsync(publicId);
            var response = new VoiceResponse();

            if (digits == "1")
            {
                response.Append(
                    new Gather(
                            numDigits: 1,
                            action: new Uri(Url.RouteUrl("VisitPatientCallResponse", new { publicId = visit.PublicId, originalNumber = visit.ParticipantPhoneNumberFormatted() }, "https")),
                            input: new List<Gather.InputEnum> { Gather.InputEnum.Dtmf },
                            hints: "0, 1, 2, 3, 4"
                        )
                        .Say($"Thank you!  If you are ready now, press 0, if you can join in 10 minutes, press 1. If you can join in 20 minutes, press 2. If you can join in 30 minutes, press 3. If you cannot join in the next 30 minutes, press 4."
                        ));

                await VisitService.SetVisitParticipantPhoneNumberAsync(visit.VideoVisitId, newNumber);
            }
            else
            {
                // If no input was sent, redirect to the /voice route
                response.Append(new Gather(
                                    numDigits: 10,
                                    action: new Uri(Url.RouteUrl("VisitChangeNumberResponse", new { publicId = visit.PublicId }, "https")),
                                    input: new List<Gather.InputEnum> { Gather.InputEnum.Dtmf },
                                    timeout: 5
                                ))
                                .Say($"Please enter a 10-digit number including area code.");
            }

            return Content(response.ToString(), "text/xml");
        }
    }
}
