using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Notifications.Services;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Notification")]
    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    public class NotificationController : Controller
    {
        public INotificationService NotificationService { get; init; }

        public NotificationController
        (
            INotificationService notificationService
        )
        {
            NotificationService = notificationService;
        }

        [HttpGet]
        [Route("Detail", Name = "AdminNotificationDetail")]
        public async Task<IActionResult> Detail(int notificationId)
        {
            var notification = await NotificationService.GetNotifications().Where(n => n.NotificationId == notificationId).FirstOrDefaultAsync();

            if (notification == null)
            {
                return NotFound();
            }

            return Ok(notification.SourceText);
        }
    }
}
