using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SutureHealth.Notifications.Services;
using SutureHealth.Visits.Services;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.WebHost.Areas.Visit.Models;
using Microsoft.EntityFrameworkCore;

namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Controllers
{
    public class VisitController : BaseVisitController
    {
        public VisitController
        (
            IVisitServicesProvider visitService,
            IApplicationService applicationService,
            INotificationService notificationService,
            ILogger<VisitController> logger,
            IConfiguration configuration
        ) : base(applicationService, visitService, notificationService, logger, configuration) { }

        [HttpGet]
        [Route("", Name = "Visit")]
        public async Task<ActionResult> Index(bool contentOnly = false)
        {
            var visits = (await VisitService.GetVisitsByMemberIdAsync(CurrentUser.Id))
                                            .Where(x => x.CreatedAt.Date == DateTimeOffset.UtcNow.Date)
                                            .OrderByDescending(v => v.VideoVisitId);
            var user = await ApplicationService.GetMemberByIdAsync(CurrentUser.Id);
            var userSettings = await ApplicationService.GetMemberSettings(CurrentUser.MemberId).ToArrayAsync();
            var userPhoneNumber = user.Contacts?.FirstOrDefault(c => c.Type == ContactType.Mobile)?.Value;

            if (string.IsNullOrEmpty(userPhoneNumber))
            {
                ViewBag.requiredPhone = true;
            }

            ViewBag.TelemedicineRobocallEnabled = userSettings.FirstOrDefault(s => string.Equals("TelemedicineRobocall", s.Key, StringComparison.OrdinalIgnoreCase))?.ItemBool ?? false;
            ViewBag.ExpireTime = DateTime.UtcNow.Date.AddMinutes(userSettings.FirstOrDefault(s => string.Equals("ExpireTimeUtcInMinutes", s.Key, StringComparison.OrdinalIgnoreCase))?.ItemInt ?? 180);

            return View(new IndexViewModel()
            {
                Visits = visits.Select(v =>
                {
                    var lastPatientSession = v.Sessions.OrderByDescending(s => s.VideoVisitSessionId).FirstOrDefault(s => !s.UserId.HasValue);

                    return new IndexViewModel.Visit()
                    {
                        PublicId = v.PublicId,
                        Active = v.Active,
                        CreatedAt = v.CreatedAt,
                        LastPatientSessionCreatedAt = lastPatientSession?.CreatedAt,
                        LastPatientSessionPhoneNumber = lastPatientSession?.PhoneNumber,
                        LastPatientName = v.Sessions?.Where(x => x.UserId == null && x.Name != null)?.Select(x => x.Name)?.LastOrDefault(),
                        JoinedMinutes = lastPatientSession != null && lastPatientSession.LastConnectedHeartbeat.HasValue ? Math.Ceiling((lastPatientSession.LastConnectedHeartbeat - lastPatientSession.CreatedAt).Value.TotalMinutes) : 0d,  // NULL CHECKS
                        PatientHasJoined = lastPatientSession != null && lastPatientSession.LastConnectedHeartbeat.HasValue,
                        PatientCurrentlyJoined = lastPatientSession != null && lastPatientSession.LastConnectedHeartbeat.HasValue && lastPatientSession.LastConnectedHeartbeat > DateTimeOffset.UtcNow.AddSeconds(-60),
                        ParticipantPhoneNumberFormatted = v.ParticipantPhoneNumberFormatted()
                    };
                }),
                CurrentUser = CurrentUser,
                RequireClientHeader = ApplicationService.ShowLegacyNavBar(CurrentUser.IsUserSender(),
                    CurrentUser.MemberId, contentOnly)
            });
        }

        [HttpPost]
        [Route("", Name = "VisitPost")]
        public async Task<ActionResult> IndexPost(string participantPhoneNumber, bool joinNow)
        {
            var user = await ApplicationService.GetMemberByIdAsync(CurrentUser.Id);
            var userSettings = await ApplicationService.GetMemberSettings(CurrentUser.MemberId).ToArrayAsync();
            var organization = await ApplicationService.GetOrganizationByIdAsync(CurrentUser.PrimaryOrganizationId);
            var activeVisits = await VisitService.GetActiveVisitsByMemberIdAsync(CurrentUser.Id);
            var videoVisit = new SutureHealth.Visits.Core.Visit
            {
                HostUserId = CurrentUser.Id,
                Active = true,
                CreatedAt = DateTimeOffset.UtcNow,
                HostName = user.SigningName,
                ParticipantPhoneNumber = Regex.Replace(participantPhoneNumber, "\\D", string.Empty),
                HostIpAddress = HttpContext.Connection.RemoteIpAddress.ToString()
            };

            int defaultExpireTimeUtcInMinutes = userSettings.FirstOrDefault(s => string.Equals("ExpireTimeUtcInMinutes", s.Key, StringComparison.OrdinalIgnoreCase))?.ItemInt ?? 180;
            var expireAt = new DateTimeOffset(videoVisit.CreatedAt.Year, videoVisit.CreatedAt.Month, videoVisit.CreatedAt.Day, defaultExpireTimeUtcInMinutes / 60, defaultExpireTimeUtcInMinutes % 60, 0, videoVisit.CreatedAt.Offset);
            if (expireAt < DateTimeOffset.UtcNow)
            {
                expireAt = expireAt.AddDays(1);
            }

            videoVisit.ExpiresAt = expireAt;
            videoVisit.PublicId = await VisitService.GenerateUniquePublicId();
            videoVisit.HostSupportPhoneNumber = organization.Contacts.FirstOrDefault(c => c.Type == ContactType.Phone)?.Value;

            if (string.IsNullOrEmpty(videoVisit.HostName))
            {
                videoVisit.HostName = $"{user.FirstName} {user.LastName}" + (!string.IsNullOrWhiteSpace(user.Suffix) ? $", {user.Suffix}" : string.Empty) + (!string.IsNullOrWhiteSpace(user.ProfessionalSuffix) ? $" {user.ProfessionalSuffix}" : string.Empty);
            }

            await VisitService.AddVisitAsync(videoVisit);
            if (userSettings.FirstOrDefault(s => string.Equals("TelemedicineRobocall", s.Key, StringComparison.OrdinalIgnoreCase))?.ItemBool ?? false)
            {
                await PatientCallKickoff(videoVisit);
            }
            else
            {
                await SendPatientMessageAsync(videoVisit);
            }

            if (joinNow)
            {
                Log.LogInformation("Joining now - " + videoVisit.VideoVisitId);
                return RedirectToRoute("VisitRoom", new { publicId = videoVisit.PublicId });
            }
            else
            {
                Log.LogInformation("Text when they join - " + videoVisit.VideoVisitId);
                return RedirectToRoute("Visit");
            }
        }

        [HttpPost]
        [Route("ChangeRoboCallEnabled", Name = "VisitChangeRoboCallEnabled")]
        public async Task<IActionResult> ChangeRoboCallEnabled(bool enable)
        {
            var setting = await ApplicationService.GetMemberSettings(CurrentUser.Id).FirstOrDefaultAsync(s => s.Key == "TelemedicineRobocall");

            if (setting != null && setting.ItemBool != enable)
            {
                await ApplicationService.SetMemberSettingAsync(setting.SettingId, "TelemedicineRobocall", enable, null, null, Application.ItemType.Boolean);
            }
            else if (setting == null)
            {
                await ApplicationService.AddMemberSettingAsync(CurrentUser.Id, "TelemedicineRobocall", enable, null, null, Application.ItemType.Boolean);
            }
            
            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Error", Name = "VisitError")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
