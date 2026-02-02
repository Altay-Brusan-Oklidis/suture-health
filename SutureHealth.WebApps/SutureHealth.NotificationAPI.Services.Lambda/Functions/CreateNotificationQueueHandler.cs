using System;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;

namespace SutureHealth.Notifications.Services.Lambda
{
    public partial class Function
    {
        public async Task CreateNotificationQueueHandler(SQSEvent model)
        {
            foreach (var record in model.Records)
            {
                if (Guid.TryParse(record.Body, out Guid uniqueNotificationId))
                {
                    await NotificationService.SendNotification(uniqueNotificationId);
                }
            }
        }
    }
}