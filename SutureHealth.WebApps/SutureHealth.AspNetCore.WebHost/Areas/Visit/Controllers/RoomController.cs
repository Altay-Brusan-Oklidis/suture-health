using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Twilio.Jwt.AccessToken;
using SutureHealth.Notifications.Services;
using SutureHealth.Visits.Core;
using SutureHealth.Visits.Services;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.WebHost.Areas.Visit.Models;

namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Controllers
{
    public class RoomController : BaseVisitController
    {
        public RoomController
        (
            IVisitServicesProvider visitService,
            IApplicationService securityService,
            INotificationService notificationService,
            IConfiguration configuration,
            ILogger<RoomController> logger
        ) : base(securityService, visitService, notificationService, logger, configuration) { }

        [HttpGet]
        [HttpPost]
        [AllowAnonymous]
        [Route("{publicId}/Room", Name = "VisitRoom")]
        public async Task<IActionResult> Room([FromRoute] string publicId, [FromQuery] string name)
        {
            var result = await CheckForVideoVisitAsync(publicId);
            if (result.ErrorAction != null)
            {
                return result.ErrorAction;
            }

            var videoVisit = result.Visit;
            var formattedParticipantNumber = videoVisit.ParticipantPhoneNumberFormatted();
            var session = new Session
            {
                CreatedAt = DateTimeOffset.UtcNow,
                VideoVisitId = videoVisit.VideoVisitId,
                IpAddress = HttpContext.Connection.RemoteIpAddress.ToString()
            };

            var model = new RoomViewModel()
            {
                UserId = CurrentUser?.Id,
                VideoVisitId = videoVisit.VideoVisitId,
                VisitPublicId = videoVisit.PublicId
            };

            if (CurrentUser != null)  // user is physician
            {
                session.Name = videoVisit.HostName;
                session.UserId = CurrentUser.Id;
                model.OtherParticipantName = videoVisit.Sessions.Where(s => s.UserId == null && !string.IsNullOrWhiteSpace(s.Name)).OrderByDescending(s => s.VideoVisitSessionId).FirstOrDefault()?.Name ?? formattedParticipantNumber;
            }
            else // user is patient
            {
                session.Name = name;
                session.PhoneNumber = videoVisit.ParticipantPhoneNumber;
                model.OtherParticipantName = videoVisit.HostName;

                var minuteAgo = DateTimeOffset.UtcNow.AddMinutes(-1);
                if (await VisitService.IsMemberInSessionAsync(videoVisit.HostUserId))
                {
                    ViewBag.PhysicianOnAnotherCall = true;
                }
            }

            await VisitService.AddSessionAsync(videoVisit.VideoVisitId, session);

            // Create a video grant for the token
            var grant = new VideoGrant();
            grant.Room = $"{videoVisit.PublicId}: {videoVisit.HostUserId} -> {videoVisit.PublicId}";
            var grants = new HashSet<IGrant> { grant };

            model.VideoVisitSessionId = session.VideoVisitSessionId;
            model.Name = session.Name ?? formattedParticipantNumber;

            // Robb - need to use a unique identity to avoid the same name between physician and patient
            var token = new Token(TwilioAccountSid,
                                  TwilioApiKey,
                                  TwilioApiSecret,
                                  identity: $"{session.VideoVisitId}-{Guid.NewGuid()}-{session.UserId}", grants: grants);
            model.AccessToken = token.ToJwt();
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("RoomNotFoundError", Name = "VisitRoomNotFoundError")]
        public ActionResult RoomNotFoundError()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{publicId}/Room/Expired", Name = "VisitExpiredError")]
        public async Task<IActionResult> ExpiredError(string publicId)
        {
            var visit = await VisitService.GetVisitByPublicIdAsync(publicId);
            var model = new ExpiredErrorModel
            {
                HostName = visit.HostName,
                HostSupportPhoneNumber = visit.HostSupportPhoneNumberFormatted(),
                IsAuthenticated = CurrentUser != null
            };

            return View(model);
        }

        [HttpGet]
        [Route("{publicId}/Room/HostJoin", Name = "VisitHostJoin")]
        public ActionResult HostJoin(string publicId)
        {
            return RedirectToRoute("VisitRoom", new { publicId });
        }

        [HttpGet]
        [Route("{publicId}/Room/Close", Name = "VisitRoomClose")]
        public async Task<IActionResult> RoomClose(string publicId)
        {
            var visit = await VisitService.GetVisitByPublicIdAsync(publicId);
            if (visit == null)
            {
                return RedirectToRoute("VisitRoomNotFoundError");
            }

            if (visit.Active && visit.HostUserId == CurrentUser.Id)
            {
                await VisitService.CloseRoomAsync(visit.VideoVisitId);
            }
            else
            {
                return BadRequest();
            }

            return RedirectToRoute("Visit");
        }

        [HttpPost]
        [Route("{publicId}/Room/Deactivate", Name = "VisitDeactivate")]
        public async Task<IActionResult> Deactivate(string publicId)
        {
            var visit = await VisitService.GetVisitByPublicIdAsync(publicId);
            if (visit == null)
            {
                return RedirectToRoute("VisitRoomNotFoundError");
            }

            if (visit.Active && visit.HostUserId == CurrentUser.Id)
            {
                await VisitService.DeactivateVisitAsync(visit.VideoVisitId);
            }
            else
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        [Route("{publicId}/Room/Reactivate", Name = "VisitReactivate")]
        public async Task<IActionResult> Reactivate(string publicId)
        {
            var visit = (await VisitService.GetVisitsByMemberIdAsync(CurrentUser.Id)).SingleOrDefault(v => v.PublicId == publicId);
            if (visit == null)
            {
                return RedirectToRoute("VisitRoomNotFoundError");
            }

            if (!visit.Active)
            {
                await VisitService.ActivateVisitAsync(visit.VideoVisitId);
            }
            else
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("{publicId}/Session/{sessionId:long}/Heartbeat", Name = "VisitSessionHeartbeat")]
        public async Task<IActionResult> Heartbeat(long sessionId, bool connected, long? bandwidth)
        {
            await VisitService.InvokeHeartbeatAsync(sessionId, connected, bandwidth);
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("{publicId}/Session/{sessionId:long}/Feedback", Name = "VisitFeedback")]
        public async Task<IActionResult> Feedback(string publicId, long sessionId, string roomSid, string userSid, string comments, string logRocketId, int rating = -1)
        {
            var visit = await VisitService.GetVisitByPublicIdAsync(publicId);
            var feedback = new Feedback
            {
                VideoVisitSessionIdFromFeedback = sessionId,
                TwilioRoomSid = roomSid,
                TwilioUserSid = userSid,
                CreatedAt = DateTimeOffset.UtcNow,
                LogRocketId = logRocketId,
                Rating = rating,
                Comments = comments
            };

            if (visit != null)
            {
                feedback.VideoVisitId = visit.VideoVisitId;
            }

            await VisitService.AddFeedbackAsync(feedback);
            return Ok();
        }
    }
}
