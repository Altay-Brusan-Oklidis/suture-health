using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using SutureHealth.Notifications.Services;

namespace SutureHealth.Notifications.Providers
{
    public class SRFaxNotificationProvider : NotificationProviderBase, ITelephoneNumberNotificationProvider
    {
        private readonly Uri serverUrl = new Uri("https://www.srfax.com/SRF_SecWebSvc.php");
        private readonly string accessId;
        private readonly string accessPassword;

        private TimeZoneInfo standardTimezone;

        public override Channel Channel => Channel.Fax;
        public override Guid ProviderId => Guid.Parse("6de249a8-9195-4592-acd0-1258fdae5e1b");
        public override string ProviderType => typeof(SRFaxNotificationProvider).FullName;
        public override bool Preferred { get; }

        public SRFaxNotificationProvider
        (
           INotificationService notificationServices,
           IHttpClientFactory httpClientFactory,
           IConfiguration configuration,
           ILogger<INotificationProvider> logger
        ) : base(notificationServices, httpClientFactory, configuration, logger)
        {
            this.accessId = configuration["SutureHealth:NotificationServicesProvider:Providers:SRFax:AccessId"];
            this.accessPassword = configuration["SutureHealth:NotificationServicesProvider:Providers:SRFax:AccessPwd"];
            this.Preferred = bool.TryParse(configuration["SutureHealth:NotificationServicesProvider:Providers:SRFax:Preferred"], out var preferred) ? preferred : false;
            try { this.standardTimezone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); } catch (TimeZoneNotFoundException) { }
            if (this.standardTimezone == null)
            {
                try
                {
                    this.standardTimezone = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
                    logger.LogDebug(@"Using the timezone ""America\Chicago""");
                }
                catch (TimeZoneNotFoundException) { }
            }
            else
            {
                logger.LogDebug(@"Using the timezone ""Central Standard Time""");
            }
        }

        // todo - need to filter down to incomplete notifications (check everywhere we are marking complete)
        public async override Task HandleCallbackAsync(NotificationStatus notification, Microsoft.AspNetCore.Http.HttpRequest request)
        {
            StringValues values;
            var faxResponse = await request.ReadFormAsync();
            if (faxResponse.TryGetValue("DateSent", out values))
            {
                if (DateTime.TryParse(values.FirstOrDefault(), out var notificationDate))
                {
                    if (this.standardTimezone != null)
                    {
                        notificationDate = TimeZoneInfo.ConvertTime(notificationDate, this.standardTimezone, TimeZoneInfo.Utc);
                    }
                    notification.SendDateTime = notificationDate.ToUniversalTime();
                }
            }
            if (faxResponse.TryGetValue("DateQueued", out values))
            {
                if (DateTime.TryParse(values.FirstOrDefault(), out var queueDate))
                {
                    if (this.standardTimezone != null)
                    {
                        queueDate = TimeZoneInfo.ConvertTime(queueDate, this.standardTimezone, TimeZoneInfo.Utc);
                    }
                    notification.NotificationDate = queueDate.ToUniversalTime();
                }
            }
            if (faxResponse.TryGetValue("SentStatus", out values))
            {
                notification.StatusCode = values.FirstOrDefault();
            }

            switch (notification.StatusCode)
            {
                case "Sent":
                    notification.Success = true;
                    notification.Message = "The fax has been sent.";

                    break;
                case "Failed":
                    notification.Success = false;
                    notification.Complete = true;
                    notification.Message = $"The fax has failed because of a {faxResponse["ErrorCode"]} error.";

                    break;
            }
        }

        public async override Task SendNotificationAsync(NotificationStatus notification)
        {

            if (notification.TerminationDate <= DateTime.UtcNow)
            {
                Logger.LogInformation($"TerminationDate [{notification.TerminationDate}] is the the past for Notification [{notification.Id}]. Exiting.");
            }

            var response = await GetMessageContentAsync(notification.SourceUrl);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogCritical($"Error: [{response.StatusCode}] from {notification.SourceUrl} for Notification [{notification.Id}]. Exiting.");
            }

            notification.ProviderId = ProviderId;
            notification.ProviderType = ProviderType;

            var notificationUrl = NotificationServices.GenerateCallbackUrl(notification);

            /// TODO:
            // this needs to check what type of response came back. if its application/pdf
            // it will need to do a streamread. we are assuming this is just a html read for
            // now.
            var html = await response.Content.ReadAsStringAsync();
            var bytes = System.Text.Encoding.UTF8.GetBytes(html);
            var encoded = System.Convert.ToBase64String(bytes);

            var form = notification.AdditionalOptions.Select(x => new KeyValuePair<string, string>($"s{x.Key}", x.Value.ToString()))
                                         .Union(new Dictionary<string, string>
                                         {
                                             { "action", "Queue_Fax" },
                                             { "access_id", this.accessId },
                                             { "access_pwd", this.accessPassword },
                                             { $"s{TO_FAX_NUMBER}", string.Join("|", ParseStringForTelephoneNumber(notification.DestinationUri)) },
                                             { $"s{NOTIFY_URL}", notificationUrl.ToString() },
                                             { "sFaxType", "SINGLE" },
                                             { "sCallerID", "2057194210" },
                                             { "sSenderEmail", "support@suture.health" },
                                             { $"s{FILE_NAME}", "Document.html" }
                                         })
                                        .Aggregate(new MultipartFormDataContent(), (@params, nxt) =>
                                        {
                                            @params.Add(new StringContent(string.IsNullOrEmpty(nxt.Value) ? string.Empty : Uri.EscapeUriString(nxt.Value)), nxt.Key);
                                            return @params;
                                        });

            form.Add(new StringContent(encoded), $"s{FILE_CONTENT}");

            try
            {
                using (var http = HttpClientFactory.CreateClient(nameof(INotificationProvider)))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    http.BaseAddress = this.serverUrl;
                    var request = new HttpRequestMessage(HttpMethod.Post, this.serverUrl);
                    request.Content = form;

                    var result = await http.SendAsync(request);
                    if (result.IsSuccessStatusCode)
                    {
                        var resultString = await result.Content.ReadAsStringAsync();
                        var jObject = JObject.Parse(resultString);
                        var status = (string)jObject.SelectToken("Status");

                        if (status == "Success")
                        {
                            notification.StatusCode = HttpStatusCode.OK.ToString();
                            notification.ProviderExternalKey = (string)jObject.SelectToken("Result");
                            notification.Success = true;
                            notification.Complete = true;

                            Logger.LogInformation($"Fax Queued of {notification.Id}. Awaiting callback on {notificationUrl}.");
                        }
                        else
                        {
                            notification.StatusCode = HttpStatusCode.BadRequest.ToString();
                            notification.Message = (string)jObject.SelectToken("Result");
                            notification.Success = false;
                            notification.Complete = true;
                        }
                    }
                    else
                    {
                        notification.StatusCode = result.StatusCode.ToString();
                        notification.Message = result.ReasonPhrase;
                        notification.Success = false;
                        notification.Complete = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);

                notification.Message = e.Message;
                notification.Success = true;
                notification.StatusCode = HttpStatusCode.InternalServerError.ToString();
            }
        }
    }
}