using SutureHealth.Notifications.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Notifications
{
    public interface ITelephoneNumberNotificationProvider  : INotificationProvider
    { }

    public static class TelephoneNumberNotificationProviderExtensions
    {
        public static TelephoneNumber ToTelephoneNumber(this IContactInfo contactInfo)
        {
            // this needs to get some work done on it. to properly parse the value and ensure
            // that the pieces and parts are good.
            return new TelephoneNumber(contactInfo.Value, 1);
        }

        public static async Task SendShortMessage(this INotificationService notificationServices, IEnumerable<TelephoneNumber> mobileNumbers, string content)
        {
            if (notificationServices.GetNotificationProviderByType(Channel.Sms) is ITelephoneNumberNotificationProvider provider)
            {
                var destinationUri = mobileNumbers.Skip(1)
                                                  .Aggregate(new StringBuilder().Append(mobileNumbers.FirstOrDefault().ToString() ?? ""), (sb, x) => sb.Append(";").Append(x));
                await notificationServices.CreateNotificationAsync(Channel.Sms, $"Text Message to {destinationUri}", destinationUri.ToString(), sourceText: content);
            }
            else
            {
                throw new NotSupportedException($"Could not find a NotificationProvider for sending Short Messages.");
            }
        }
    }
}