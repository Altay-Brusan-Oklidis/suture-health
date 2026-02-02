using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using SutureHealth.Notifications.Services;

namespace SutureHealth.Notifications.Providers.TwilioSMS
{
    public class TwilioTextToSpeechProvider : NotificationProviderBase, ITelephoneNumberNotificationProvider
    {
        public override Channel Channel => Channel.TextToSpeech;
        public override Guid ProviderId => Guid.Parse("177E0A55-FBEC-47EC-B45E-F1959EBF1245");
        public override string ProviderType => typeof(TwilioTextToSpeechProvider).FullName;
        public override bool Preferred { get; }

        public TwilioTextToSpeechProvider
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
                    var sourceText = notification.SourceText;
                    if (string.IsNullOrEmpty(sourceText))
                    {
                        var response = await GetMessageContentAsync(notification.SourceUrl);
                        sourceText = await response.Content.ReadAsStringAsync();
                    }

                    var call = await CallResource.CreateAsync(
                        twiml: new Twilio.Types.Twiml(sourceText),
                        to: new Twilio.Types.PhoneNumber(phoneNumber),
                        from: new Twilio.Types.PhoneNumber(FromPhoneNumber),
                        statusCallback: new Uri(NotificationServices.GenerateCallbackUrl(notification))
                    );

                    Console.WriteLine($"Twilio call id = {call.Sid}");

                    notification.ProviderId = ProviderId;
                    notification.ProviderType = ProviderType;
                    notification.ProviderExternalKey = call.Sid;
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
                    Console.WriteLine("Error occurred during callback:  " + ex.Message);
                }
            }
        }

        public async override Task HandleCallbackAsync
        (
            NotificationStatus notification,
            Microsoft.AspNetCore.Http.HttpRequest request
        )
        {
            var callbackResponse = await request.ReadFormAsync();
            switch (callbackResponse["CallStatus"].ToString())
            {
                case "completed":
                    notification.Success = true;
                    notification.Complete = true;
                    notification.SendDateTime = DateTime.UtcNow;
                    notification.Message = "Call completed successfully.";
                    break;

                case "busy":
                    notification.Success = false;
                    notification.Complete = true;
                    notification.Message = "The phone number was busy.";
                    break;

                case "no-answer":
                    notification.Success = false;
                    notification.Complete = true;
                    notification.Message = "The call was not answered.";
                    break;

                case "failed":
                case "InternalServerError":
                    notification.Success = false;
                    notification.Complete = true;
                    notification.Message = $"Unknown error.";
                    break;
            }
        }
    }
}
