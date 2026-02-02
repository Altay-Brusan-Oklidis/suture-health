using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SutureHealth.Notifications.Services;

namespace SutureHealth.Notifications.Providers.CustomerIO
{
    public class CustomerIoNotificationProvider : NotificationProviderBase, INotificationProvider, IEmailNotificationProvider
    {
        public override Guid ProviderId => Guid.Parse("373c7440-6c42-4084-a482-c68dd1a0f72c");
        public override Channel Channel => Channel.Email;
        public override string ProviderType => typeof(CustomerIoNotificationProvider).FullName;
        public override bool Preferred { get; }

        protected string ApiKey { get; set; }
        protected string ReplyToAddress { get; set; }
        protected string SenderAddress { get; set; }

        public CustomerIoNotificationProvider
        (
            INotificationService notificationServices,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<INotificationProvider> logger
        ) : base(notificationServices, httpClientFactory, configuration, logger)
        {
            ApiKey = configuration["SutureHealth:NotificationServicesProvider:Providers:CustomerIo:ApiKey"];
            ReplyToAddress = configuration["SutureHealth:NotificationServicesProvider:Providers:CustomerIo:ReplyToAddress"];
            SenderAddress = configuration["SutureHealth:NotificationServicesProvider:Providers:CustomerIo:SenderAddress"];
            Preferred = bool.TryParse(configuration["SutureHealth:NotificationServicesProvider:Providers:CustomerIo:Preferred"], out var preferred) ? preferred : false;
        }

        public override async Task SendNotificationAsync(NotificationStatus notification)
        {
            var options = null as NotificationStatusOptions;
            var recipients = new Recipients(notification);

            notification.ProviderId = ProviderId;
            notification.ProviderType = ProviderType;

            if (string.IsNullOrWhiteSpace(recipients.To.FirstOrDefault()))
            {
                notification.Success = false;
                notification.Message = $"ProviderType \"{ProviderType}\" requires at least one To recipient.";
                return;
            }
            if (recipients.Count > 15)
            {
                notification.Success = false;
                notification.Message = $"ProviderType \"{ProviderType}\" does not support sending to more than 15 recipients at a time.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(notification.AdditionalOptionsJson))
            {
                options = JsonSerializer.Deserialize<NotificationStatusOptions>(notification.AdditionalOptionsJson);
            }

            switch (options?.Type ?? default)
            {
                case NotificationStatusOptions.EmailType.Text:
                    await SendTextNotificationAsync(notification, recipients);
                    break;
                case NotificationStatusOptions.EmailType.Template:
                    if (!string.IsNullOrWhiteSpace(notification.SourceUrl))
                    {
                        notification.Success = false;
                        notification.Message = $"Template email types do not support specifying {nameof(notification.SourceUrl)}";
                        return;
                    }

                    await SendTemplateNotificationAsync(notification, recipients, options);
                    break;
                default:
                    notification.Success = false;
                    notification.Message = $"Unknown email type '{options.Type}'";
                    break;
            }

            notification.Complete = notification.Success.HasValue;
        }

        protected async Task SendTextNotificationAsync(NotificationStatus notification, Recipients recipients)
        {
            var content = string.Empty;
            if (!string.IsNullOrWhiteSpace(notification.SourceUrl))
            {
                var httpResponse = await GetMessageContentAsync(notification.SourceUrl);
                content = await httpResponse.Content.ReadAsStringAsync();
            }
            else if (!string.IsNullOrWhiteSpace(notification.SourceText))
            {
                content = notification.SourceText;
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                using var client = GetApiClient();
                using var response = await client.PostAsJsonAsync("send/email", new
                {
                    from = SenderAddress,
                    to = string.Join(",", recipients.To),
                    bcc = recipients.Bcc.Any() ? string.Join(",", recipients.Bcc) : null,
                    reply_to = !string.IsNullOrWhiteSpace(ReplyToAddress) ? ReplyToAddress : null,
                    subject = notification.Subject ?? string.Empty,
                    body = content,
                    identifiers = new { id = 0 },
                    send_to_unsubscribed = true,
                    tracked = true
                });

                if (!response.IsSuccessStatusCode)
                {
                    notification.Success = false;
                    notification.Message = await response.Content.ReadAsStringAsync();
                    return;
                }

                notification.Success = true;
                notification.NotificationDate = DateTime.UtcNow;
                notification.StatusCode = "Sent";
                notification.ProviderExternalKey = ((JsonElement)JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync())["delivery_id"]).GetString();
            }
            else
            {
                notification.Message = "We got nothing back from the httpResponse.";
                notification.Success = false;
            }
        }

        protected async Task SendTemplateNotificationAsync(NotificationStatus notification, Recipients recipients, NotificationStatusOptions options)
        {
            notification.Success = false;
            notification.Message = "Sending of Customer.IO template emails has not yet been implemented.";
        }

        protected HttpClient GetApiClient()
        {
            var client = HttpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
            client.BaseAddress = new Uri(@"https://api.customer.io/v1/");

            return client;
        }

        public override Task HandleCallbackAsync(NotificationStatus notification, HttpRequest httpRequest)
        {
            throw new NotImplementedException();
        }

        public string CreateDestination(IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc)
        {
            if ((cc ?? Array.Empty<string>()).Any())
            {
                throw new NotSupportedException($"ProviderType \"{ProviderType}\" does not support CC'ing recipients.");
            }

            return string.Join(";", (to ?? Array.Empty<string>()).Select(a => $"mailto:{a}")
                                    .Union((bcc ?? Array.Empty<string>()).Select(a => $"mailbcc:{a}")));
        }

        protected class Recipients
        {
            public Recipients()
            {
                To = Array.Empty<string>();
                Bcc = Array.Empty<string>();
            }

            public Recipients(NotificationStatus notification)
            {
                To = ParseStringForDestinations(notification.DestinationUri, "mailto")
                        .Union(ParseStringForDestinations(notification.DestinationUri, "mailcc"));
                Bcc = ParseStringForDestinations(notification.DestinationUri, "mailbcc");
            }

            public IEnumerable<string> To { get; set; }
            public IEnumerable<string> Bcc { get; set; }

            public int Count => (To ?? Array.Empty<string>()).Union(Bcc ?? Array.Empty<string>()).Count();
        }
    }
}
