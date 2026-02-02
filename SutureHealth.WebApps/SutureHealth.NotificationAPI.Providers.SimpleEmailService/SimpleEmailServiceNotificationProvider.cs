using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Amazon.SimpleEmail;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SutureHealth.Notifications.Services;

namespace SutureHealth.Notifications.Providers
{
    public class SimpleEmailServiceNotificationProvider : NotificationProviderBase, IEmailNotificationProvider
    {
        string ConfigurationSet { get; set; }
        string ReplyToAddress { get; set; }
        string SenderAddress { get; set; }

        bool BypassSendingEmail { get; set; } = false;

        public override Channel Channel => Channel.Email;
        public override Guid ProviderId => Guid.Parse("ef3f2c21-d9ba-4846-8b95-4bd2fe614358");
        public override string ProviderType => typeof(SimpleEmailServiceNotificationProvider).FullName;
        public override bool Preferred { get; }

        protected IAmazonSimpleEmailService SimpleEmailService { get; }

        public SimpleEmailServiceNotificationProvider
        (
           INotificationService notificationServices,
           IHttpClientFactory httpClientFactory,
           IConfiguration configuration,
           ILogger<INotificationProvider> logger,
           IAmazonSimpleEmailService simpleEmailService
        ) : base(notificationServices, httpClientFactory, configuration, logger)
        {
            SimpleEmailService = simpleEmailService;

            BypassSendingEmail = bool.TryParse(configuration["SutureHealth:NotificationServicesProvider:Providers:SimpleEmailService:BypassSendingEmail"], out bool bypass) && bypass;
            ConfigurationSet = configuration["SutureHealth:NotificationServicesProvider:Providers:SimpleEmailService:ConfigurationSet"];
            ReplyToAddress = configuration["SutureHealth:NotificationServicesProvider:Providers:SimpleEmailService:ReplyToAddress"];
            SenderAddress = configuration["SutureHealth:NotificationServicesProvider:Providers:SimpleEmailService:SenderAddress"];
            Preferred = bool.TryParse(configuration["SutureHealth:NotificationServicesProvider:Providers:SimpleEmailService:Preferred"], out var preferred) ? preferred : false;
        }

        string IEmailNotificationProvider.CreateDestination(IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc) =>
            string.Join(";", (to ?? Array.Empty<string>()).Select(a => $"mailto:{a}")
                                                          .Union((cc ?? Array.Empty<string>()).Select(a => $"mailcc:{a}"))
                                                          .Union((bcc ?? Array.Empty<string>()).Select(a => $"mailbcc:{a}")));

        public async Task<NotificationStatus> GenerateSimpleNotificationServiceRequest
        (
            Guid notificationId,
            string destinationUrl,
            string sourceUrl,
            string callbackUrl,
            string subject,
            DateTime terminationDate,
            IDictionary<string, object> additionalOptions
        )
        {
            var notification = new NotificationStatus
            {
                Message = "250 ByPass",
                NotificationDate = DateTime.UtcNow,
                ProviderExternalKey = Guid.NewGuid().ToString(),
                ProviderId = ProviderId,
                ProviderType = ProviderType,
                Channel = Channel.Email,
                Success = true
            };

            return await Task.FromResult(notification);
        }

        public override async Task HandleCallbackAsync(NotificationStatus notification, Microsoft.AspNetCore.Http.HttpRequest request)
        {
            JToken json = null;
            using (StreamReader streamReader = new StreamReader(request.Body))
            {
                json = await JToken.ReadFromAsync(new JsonTextReader(streamReader));

                notification.ProviderId = ProviderId;
                notification.ProviderType = ProviderType;
                notification.ProviderExternalKey = (string)json["mail"]["messageId"];
                notification.StatusCode = (string)json["notificationType"];

                var result = json[notification.StatusCode.ToLower()];
                switch (notification.StatusCode)
                {
                    case "Delivery":
                        string response = (string)result["smtpResponse"];

                        notification.Message = response;
                        notification.NotificationDate = (DateTime?)result["timestamp"];
                        notification.Success = true;
                        notification.Complete = true;

                        break;
                    case "Bounce":
                        notification.Message = $"{result["bounceType"]}:{result["bounceSubType"]}";
                        notification.NotificationDate = (DateTime?)result["timestamp"];
                        notification.Success = false;
                        notification.Complete = true;

                        break;
                    case "Complaint":
                        notification.Message = (string)result["complaintFeedbackType"] ?? string.Empty;
                        notification.NotificationDate = (DateTime?)result["timestamp"];
                        notification.Success = false;
                        notification.Complete = true;

                        break;
                }
            }
        }

        public override async Task SendNotificationAsync(NotificationStatus notification)
        {
            if (!BypassSendingEmail)
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
                    var message = new Amazon.SimpleEmail.Model.SendEmailRequest
                    {
                        ConfigurationSetName = ConfigurationSet,
                        Destination = new Amazon.SimpleEmail.Model.Destination
                        {
                            ToAddresses = ParseStringForDestinations(notification.DestinationUri, "mailto").ToList(),
                            BccAddresses = ParseStringForDestinations(notification.DestinationUri, "mailbcc").ToList(),
                            CcAddresses = ParseStringForDestinations(notification.DestinationUri, "mailcc").ToList()
                        },
                        Message = new Amazon.SimpleEmail.Model.Message
                        {
                            Body = new Amazon.SimpleEmail.Model.Body
                            {
                                Html = new Amazon.SimpleEmail.Model.Content(content)
                            },
                            Subject = new Amazon.SimpleEmail.Model.Content(notification.Subject),
                        },
                        ReplyToAddresses = new List<string> { ReplyToAddress },
                        Source = SenderAddress,
                        Tags = new List<Amazon.SimpleEmail.Model.MessageTag> {
                                new Amazon.SimpleEmail.Model.MessageTag
                                {
                                    Name = "NotificationId",
                                    Value = notification.Id.ToString()
                                }
                            }
                    };

                    var emailResponse = await SimpleEmailService.SendEmailAsync(message);

                    notification.ProviderId = ProviderId;
                    notification.ProviderType = ProviderType;
                    notification.ProviderExternalKey = emailResponse.MessageId;
                    notification.StatusCode = emailResponse.HttpStatusCode.ToString();

                    if (emailResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        notification.Success = true;
                    }
                    else
                    {
                        notification.Message = emailResponse.ToString();
                        notification.Success = false;
                    }
                }
                else
                {
                    notification.Message = "We got nothing back from the httpResponse.";
                    notification.Success = false;
                }
            }
            else
            {
                await GenerateSimpleNotificationServiceRequest(notification.Id,
                                                               notification.DestinationUri,
                                                               notification.SourceUrl,
                                                               notification.CallbackUrl,
                                                               notification.Subject,
                                                               notification.TerminationDate,
                                                               notification.AdditionalOptions);
            }
        }
    }
}
