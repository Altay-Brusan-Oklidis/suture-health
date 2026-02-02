using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SutureHealth.Notifications.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SutureHealth.Notifications.Providers
{
    public abstract class NotificationProviderBase : INotificationProvider
    {
        protected INotificationService NotificationServices { get; }
        protected IHttpClientFactory HttpClientFactory { get; }
        protected IConfiguration Configuration { get; }
        protected ILogger Logger { get; }

        public NotificationProviderBase
        (
           INotificationService notificationServices,
           IHttpClientFactory httpClientFactory,
           IConfiguration configuration,
           ILogger<INotificationProvider> logger
        )
        {
            Configuration = configuration;
            HttpClientFactory = httpClientFactory;
            Logger = logger;
            NotificationServices = notificationServices;
        }

        #region INotificationProvider
        public abstract Channel Channel { get; }
        public abstract Guid ProviderId { get; }
        public abstract string ProviderType { get; }
        public abstract bool Preferred { get; }

        public abstract Task HandleCallbackAsync(NotificationStatus notification, Microsoft.AspNetCore.Http.HttpRequest request);
        public abstract Task SendNotificationAsync(NotificationStatus notification);
        #endregion

        protected async Task<HttpResponseMessage> GetMessageContentAsync(string sourceUrl)
        {
            using (var http = HttpClientFactory.CreateClient(nameof(INotificationProvider)))
            {
                return await http.GetAsync(sourceUrl);
            }
        }

        public static IEnumerable<string> ParseStringForDestinations(string source, string protocol = null)
        {
            var destinations = new List<string>();
            var addresses = source.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return addresses.Select(x => x.Split(':'))
                            .Where(x => x.Count() > 1 && !string.IsNullOrWhiteSpace(x.ElementAt(1)) && string.Equals(x.ElementAt(0), protocol ?? x.ElementAt(0), StringComparison.OrdinalIgnoreCase))
                            .Select(x => x.ElementAt(1))
                            .Distinct();
        }

        public static IEnumerable<string> ParseStringForTelephoneNumber(string source)
        {
            return ParseStringForDestinations(source, "tel")
                .Select(s => Regex.Replace(s, "[^0-9]", "", RegexOptions.Compiled))
                .Select(n => !n.StartsWith("1") && n.Length == 10 ? $"1{n}" : n);
        }

        public const string CALLER_ID = "CallerID";
        public const string SENDER_EMAIL = "SenderEmail";
        public const string FAX_TYPE = "FaxType";
        public const string TO_FAX_NUMBER = "ToFaxNumber";
        public const string COVER_PAGE = "CoverPage";
        public const string CP_SUBJECT = "CPSubject";
        public const string CP_COMMENTS = "CPComments";
        public const string CP_FROM_NAME = "CPFromName";
        public const string CP_TO_NAME = "CPToName";
        public const string CP_ORGANIZATION = "CPOrganization";
        public const string FILE_NAME = "FileName_1";
        public const string FILE_CONTENT = "FileContent_1";
        public const string NOTIFY_URL = "NotifyURL";
    }
}
