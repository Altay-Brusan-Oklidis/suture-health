using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using SutureHealth.Notifications.Services;

namespace SutureHealth.Notifications.Providers.TwilioSMS
{
    public class TwilioSMSProvider : NotificationProviderBase, ITelephoneNumberNotificationProvider
    {
        public override Channel Channel => Channel.Sms;
        public override Guid ProviderId => Guid.Parse("44C51F88-676D-48E5-AFDE-F6B04E18AFDE");
        public override string ProviderType => typeof(TwilioSMSProvider).FullName;
        public override bool Preferred { get; }

        public TwilioSMSProvider
        (
           INotificationService notificationServices,
           IHttpClientFactory httpClientFactory,
           IConfiguration configuration,
           ILogger<INotificationProvider> logger
        ) : base(notificationServices, httpClientFactory, configuration, logger)
        {

            AccountSid = configuration["SutureHealth:NotificationServicesProvider:Providers:Twilio:AccountSid"];
            ApiKey = configuration["SutureHealth:NotificationServicesProvider:Providers:Twilio:ApiKey"];
            ApiSecret = configuration["SutureHealth:NotificationServicesProvider:Providers:Twilio:ApiSecret"];
            Preferred = bool.TryParse(configuration["SutureHealth:NotificationServicesProvider:Providers:Twilio:Preferred"], out var preferred) ? preferred : false;
            FromPhoneNumber = "+12054304200";
        }

        protected string ApiKey { get; private set; }
        protected string ApiSecret { get; private set; }
        protected string AccountSid { get; private set; }
        protected string FromPhoneNumber { get; private set; }

        public async override Task SendNotificationAsync(NotificationStatus notification)
        {
            try
            {
                TwilioClient.Init(ApiKey, ApiSecret, AccountSid);

                foreach (var phoneNumber in ParseStringForTelephoneNumber(notification.DestinationUri))
                {
                    var message = await MessageResource.CreateAsync
                    (
                        body: notification.SourceText,
                        from: new Twilio.Types.PhoneNumber(FromPhoneNumber),
                        statusCallback: new Uri(NotificationServices.GenerateCallbackUrl(notification)),
                        to: new Twilio.Types.PhoneNumber(phoneNumber)
                    );

                    Console.WriteLine($"Twilio SMS message id = {message.Sid}");

                    notification.ProviderId = ProviderId;
                    notification.ProviderType = ProviderType;
                    notification.ProviderExternalKey = message.Sid;
                    notification.NotificationDate = DateTime.UtcNow;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);

                notification.Message = e.Message;
                notification.Success = false;
                notification.Complete = true;
                notification.StatusCode = HttpStatusCode.InternalServerError.ToString();

                try
                {
                    using (var http = HttpClientFactory.CreateClient())
                    {
                        var content = new StringContent(JsonConvert.SerializeObject(notification), System.Text.Encoding.UTF8, "application/json");
                        await http.PostAsync(notification.CallbackUrl, content);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Exception encountered when Sending Callback");
                }
            }
        }

        public async override Task HandleCallbackAsync(NotificationStatus notification, Microsoft.AspNetCore.Http.HttpRequest request)
        {
            StringValues values;

            var faxResponse = await request.ReadFormAsync();
            if (faxResponse.TryGetValue("date_sent", out values))
            {
                if (DateTime.TryParse(values.FirstOrDefault(), out var notificationDate))
                {
                    notification.NotificationDate = notificationDate.ToUniversalTime();
                }
            }
            if (faxResponse.TryGetValue("date_created", out values))
            {
                if (DateTime.TryParse(values.FirstOrDefault(), out var queueDate))
                {
                    notification.OriginationDate = queueDate.ToUniversalTime();
                }
            }
            if (faxResponse.TryGetValue("status", out values))
            {
                notification.StatusCode = values.FirstOrDefault();
            }

            faxResponse.TryGetValue("error_code", out values);
            switch (faxResponse["SmsStatus"].ToString())
            {
                case "sent":
                    notification.Message = "The message has been sent.";
                    break;

                case "delivered":
                    notification.Message = "The message has been delivered.";
                    notification.SendDateTime = DateTime.UtcNow;
                    notification.Success = true;
                    notification.Complete = true;
                    break;

                case "undelivered":
                case "failed":
                case "InternalServerError":
                    notification.Success = false;
                    notification.Complete = true;
                    notification.Message = $"The message has failed because of a {values.FirstOrDefault()} error.";
                    break;
            }
        }
    }
}
