using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SutureHealth.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Options;
using SutureHealth.Storage;

namespace SutureHealth.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        protected IConfiguration Configuration { get; }
        protected ILogger<INotificationService> Logger { get; }
        protected IQueueService QueueService { get; }
        protected IServiceProvider ServiceProvider { get; }

        protected NotificationDbContext NotificationContext { get; }
        protected NotificationOptions NotificationOptions { get; }

        public NotificationService
        (
            IConfiguration configuration,
            ILogger<INotificationService> logger,
            IQueueService queueService,
            IServiceProvider serviceProvider,
            NotificationDbContext notificationDbContext,
            IOptions<NotificationOptions> notificationOptions
        )
        {
            Configuration = configuration;
            Logger = logger;
            QueueService = queueService;
            NotificationContext = notificationDbContext;
            NotificationOptions = notificationOptions.Value;
            ServiceProvider = serviceProvider;
        }

        protected NotificationService()
        { }

        private async Task<HttpClient> CreateHttpClient(string apiVersion)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(NotificationOptions.ApiBaseUri);

            var response = await httpClient.GetAsync($"/auth/refreshtoken?api-version={apiVersion}&refresh_token={NotificationOptions.ApiRefreshToken}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Could *NOT* refresh token.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token["access_token"].ToString());

            return httpClient;
        }

        public async Task<NotificationStatus> CreateNotificationAsync
        (
            Channel type,
            string subject,
            string destinationUri,
            DateTimeOffset? terminationDate = null,
            DateTimeOffset? desiredSendDateTime = null,
            Uri sourceUrl = null,
            string sourceText = null,
            Uri callbackUrl = null,
            string sourceId = null,
            IDictionary<string, object> additionalOptions = null
        )
        {
            var notification = new NotificationStatus
            {
                AdditionalOptions = additionalOptions,
                CallbackUrl = callbackUrl?.AbsoluteUri,
                DesiredSendDateTime = desiredSendDateTime?.UtcDateTime,
                DestinationUri = destinationUri,
                Id = Guid.NewGuid(),
                OriginationDate = DateTime.UtcNow,
                SourceText = sourceText,
                SourceId = sourceId,
                SourceUrl = sourceUrl?.AbsoluteUri,
                Subject = subject,
                TerminationDate = terminationDate.GetValueOrDefault(desiredSendDateTime.GetValueOrDefault(DateTimeOffset.UtcNow).UtcDateTime.AddDays(1)).Date,
                Channel = type
            };

            if (NotificationOptions.QueueProcessing)
            {
                notification = await CreateNotification(notification);

                if (!notification.DesiredSendDateTime.HasValue)
                {
                    await QueueService.QueueMessageAsync(NotificationOptions.CreateQueueUrl, notification.Id.ToString());
                }
            }
            else
            {
                notification = await CreateNotification(notification);
                await SendNotification(notification);
            }

            return notification;
        }

        protected async Task<NotificationStatus> CreateNotification(NotificationStatus notification)
        {
            await NotificationContext.AddAsync(notification);
            await NotificationContext.SaveChangesAsync();
            await NotificationContext.Entry(notification).ReloadAsync();
            return notification;
        }

        string INotificationService.GenerateCallbackUrl(NotificationStatus notification) =>
            NotificationOptions.GenerateCallbackUrl(notification);

        public async Task<NotificationStatus> GetLatestNotificationById(Guid uniqueNotificationId) =>
            await NotificationContext.Notification.Where(x => x.Id == uniqueNotificationId)
                                                  .OrderBy(x => x.NotificationDate)
                                                  .LastOrDefaultAsync();

        async Task<NotificationStatus> INotificationService.GetNotificationByStatusId(Guid uniqueId) =>
            await NotificationContext.Notification.SingleOrDefaultAsync(x => x.Id == uniqueId);

        public async Task<NotificationStatus> GetNotificationByProviderExternalId(string providerExternalid) =>
            await NotificationContext.Notification.SingleOrDefaultAsync(n => n.ProviderExternalKey == providerExternalid);

        public INotificationProvider GetNotificationProviderByType(Channel channel) =>
            ServiceProvider.GetServices<INotificationProvider>()
                           .Where(p => p.Channel == channel)
                           .OrderByDescending(p => p.Preferred)
                           .FirstOrDefault();

        IQueryable<NotificationStatus> INotificationService.GetNotifications()
            => NotificationContext.Notification.AsNoTracking();

        private async Task NotifyOriginatingApplication(NotificationStatus notification)
        {
            try
            {
                var http = await CreateHttpClient(notification.ApiVersion);
                await http.PostAsync($"{notification.Id}/notify", new System.Net.Http.StringContent(notification.StatusCode));
            }
            catch (System.Exception e)
            {
                Logger.LogError(e, "Exception encountered when Invoking Origination Callback for Notification {0}", notification.Id);
            }
        }

        async Task INotificationService.SendNotification(Guid uniqueNotificationId)
        {
            var notification = await GetLatestNotificationById(uniqueNotificationId);
            using (Logger.BeginScope($"Sending Notification {uniqueNotificationId}"))
            {
                try
                {
                    await SendNotification(notification);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, e.Message);
                }
                Logger.LogInformation($"Sent Notification to {notification.DestinationUri}");
            }
        }

        async Task INotificationService.SendPendingNotifications()
        {
            var notifications = await NotificationContext.Notification
                                                         .Where(x => (x.Complete == null || !x.Complete == false) &&
                                                                x.NotificationDate == null && x.TerminationDate > DateTime.UtcNow &&
                                                                x.DesiredSendDateTime != null && x.DesiredSendDateTime < DateTime.UtcNow)
                                                         .ToArrayAsync();
            foreach (var notification in notifications)
            {
                try
                {
                    await SendNotification(notification);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"The following exception was encountered processing notification {notification.NotificationId}. \n{ex}");
                }
            }
        }

        protected async Task SendNotification(NotificationStatus notification)
        {
            INotificationProvider provider = null;
            if (!notification.ProviderId.HasValue)
            {
                provider = GetNotificationProviderByType(notification.Channel);
            }
            else
            {
                provider = ServiceProvider.GetServices<INotificationProvider>()
                                          .SingleOrDefault(p => p.ProviderId == notification.ProviderId.Value);
            }

            if (provider == null)
                throw new NotSupportedException($"Could not find a NotificationProvider for Notification Id: {notification.Id}");

            await provider.SendNotificationAsync(notification);
            await NotificationContext.SaveChangesAsync();
        }

        async Task INotificationService.UpdateNotificationStatus(Guid uniqueNotificationId, Guid providerId, HttpRequest httpRequest)
        {
            var notification = await GetLatestNotificationById(uniqueNotificationId);
            var provider = ServiceProvider.GetServices<INotificationProvider>()
                                          .LastOrDefault(p => p.ProviderId == providerId);
            if (notification != null && provider != null)
            {
                await provider.HandleCallbackAsync(notification, httpRequest);
                await this.NotificationContext.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(notification.CallbackUrl))
                    await NotifyOriginatingApplication(notification);
            }
        }

        async Task INotificationService.UpdateNotificationStatus(long notificationId, DateTime sentAt)
        {
            var notification = await NotificationContext.Notification.FindAsync(notificationId);

            notification.SendDateTime = sentAt;

            await NotifyOriginatingApplication(notification);
            await NotificationContext.SaveChangesAsync();
        }

        async Task INotificationService.UpdateNotificationStatus(long notificationId, string errorMessage)
        {
            var notification = await NotificationContext.Notification.FindAsync(notificationId);

            notification.SendDateTime = DateTime.UtcNow;
            notification.Complete = true;
            notification.Message = errorMessage;
            notification.Success = false;

            await NotifyOriginatingApplication(notification);
            await NotificationContext.SaveChangesAsync();
        }
    }
}
