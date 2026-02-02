using SutureHealth.Notifications.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SutureHealth.Notifications
{
    public interface IEmailNotificationProvider : INotificationProvider
    {
        string CreateDestination(IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc);
    }

    public static class EmailServicesExtensions
    {
        public static async Task SendEmail(this INotificationService notificationServices, IEnumerable<string> to, string subject, string content)
        {
            if (notificationServices.GetNotificationProviderByType(Channel.Email) is IEmailNotificationProvider provider)
            {
                // Limit to 15 recipients per email
                var batches = to.Select((to, i) => new { To = to, Group = i / 15 })
                                .GroupBy(to => to.Group);

                foreach (var batch in batches)
                {
                    var destinationUri = provider.CreateDestination(batch.Select(b => b.To), null, null);

                    await notificationServices.CreateNotificationAsync(Channel.Email, subject, destinationUri, sourceText: content);
                }
            }
            else
            {
                throw new NotSupportedException($"Could not find a NotificationProvider for sending Emails.");
            }
        }
    }
}
