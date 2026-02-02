using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Notifications.Services;
using SutureHealth.Application.Services;
using SutureHealth.Visits.Services;

namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Controllers
{
    [Controller]
    public class RecentsController : BaseVisitController
    {
        public RecentsController
        (
            IVisitServicesProvider visitService,
            IApplicationService securityService,
            INotificationService notificationService,
            ILogger<RecentsController> logger,
            IConfiguration configuration
        ) : base(securityService, visitService, notificationService, logger, configuration) { }

        [HttpGet("Recents")]
        public async Task<IActionResult> Recents()
        {
            await VisitService.CloseExpiredVisitsAsync();
            return View(new BaseViewModel
            {
                CurrentUser = CurrentUser,
                RequireClientHeader = ApplicationService.ShowLegacyNavBar(CurrentUser.IsUserSender(), CurrentUser.MemberId),
                RequireKendo = true
            });
        }

        [HttpPost("Recents")]
        public async Task<IActionResult> GetUserVisitsData([DataSourceRequest] DataSourceRequest request)
            => Json((await VisitService.GetVisitsByMemberIdAsync(CurrentUser.Id))
                                            .OrderByDescending(x => x.CreatedAt)
                                            .Select(visit => new SutureHealth.AspNetCore.WebHost.Areas.Visit.Models.Visit
                                            {
                                                VideoVisitId = visit.VideoVisitId,
                                                CreatedAt = visit.CreatedAt.UtcDateTime,
                                                PublicId = visit.PublicId,
                                                Active = visit.Active,
                                                HostUserId = visit.HostUserId,
                                                HostName = visit.HostName,
                                                HostSupportPhoneNumber = visit.HostSupportPhoneNumber,
                                                HostIpAddress = visit.HostIpAddress,
                                                ParticipantPhoneNumber = visit.ParticipantPhoneNumberFormatted(),
                                                TotalMinutes = visit.TotalMinutes.HasValue ? Math.Max(0, visit.TotalMinutes.Value) : 0,
                                                BillableMinutes = visit.BillableMinutes.HasValue ? Math.Max(0, visit.BillableMinutes.Value) : 0,
                                                PatientName = visit.Sessions?.Where(x => x.UserId == null && x.Name != null)?.Select(x => x.Name)?.LastOrDefault()
                                            }).ToDataSourceResult(request));
    }
}