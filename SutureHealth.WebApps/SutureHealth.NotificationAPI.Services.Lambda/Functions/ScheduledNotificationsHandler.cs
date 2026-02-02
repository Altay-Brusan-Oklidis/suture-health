using System.Threading.Tasks;

namespace SutureHealth.Notifications.Services.Lambda
{
    public partial class Function
    {
        public async Task ScheduledNotificationsHandler(Amazon.Lambda.CloudWatchEvents.ScheduledEvents.ScheduledEvent scheduledEvent)
        {
            await NotificationService.SendPendingNotifications();
        }
    }
}