using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Amazon.Lambda.SNSEvents;

namespace SutureHealth.Notifications.Services.Lambda
{
    public partial class Function
    {
        public async Task<string> SimpleEmailServiceSNSHandler(SNSEvent model)
        {
            foreach (var record in model.Records)
            {
                var sesMessage = Newtonsoft.Json.Linq.JObject.Parse(record.Sns.Message);

                if (sesMessage["mail"] == null || sesMessage["mail"]["messageId"] == null)
                {
                    var senderAddressDomain = Regex.Match(this.Configuration["SutureHealth:NotificationServicesProvider:Providers:SimpleEmailService:SenderAddress"], @"@(?<Domain>.+)$").Groups["Domain"].Value;

                    this.Logger.LogCritical("'messageId' was not found on the message body.  Ensure 'Include Original Headers' is enabled for the '{SenderAddressDomain}' domain.", senderAddressDomain);
                    throw new MissingMemberException("'messageId' was not found on the message body.  This message cannot be processed.");
                }
                var emailMessageId = (string)sesMessage["mail"]["messageId"];

                var notification = await NotificationService.GetNotificationByProviderExternalId(emailMessageId);
                if (notification != null)
                {
                    var contentBody = sesMessage.ToString(Newtonsoft.Json.Formatting.None);
                    var http = this.HttpClientFactory.CreateClient();
                    var endpoint = NotificationService.GenerateCallbackUrl(notification);
                    this.Logger.LogDebug($"Invoking common external POST request to {endpoint} for {notification.Id}");

                    (await http.PostAsync(endpoint, new StringContent(contentBody, System.Text.Encoding.Default, "application/json"))).EnsureSuccessStatusCode();
                }
                else
                {
                    // Fail silently; anything that sends emails outside of the CreateNotification workflow (such as provider integration tests) will cause us to come up empty here.
                    this.Logger.LogWarning("No notification could be found in the database with ProviderExternalKey '{emailMessageId}'.", emailMessageId);
                }
            }

            return string.Empty;
        }
    }
}