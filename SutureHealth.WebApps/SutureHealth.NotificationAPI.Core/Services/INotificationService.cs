using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SutureHealth.Notifications.Services
{
    public interface INotificationService
    {
        Task<NotificationStatus> CreateNotificationAsync
        (
            Channel channel,
            string subject,
            string destinationUri,
            DateTimeOffset? terminationDate = null,
            DateTimeOffset? desiredSendDateTime = null,
            Uri sourceUrl = null,
            string sourceText = null,
            Uri callbackUrl = null,
            string sourceId = null,
            IDictionary<string, object> additionalOptions = null
        );

        string GenerateCallbackUrl(NotificationStatus notification);

        Task<NotificationStatus> GetLatestNotificationById(Guid uniqueNotifiationId);
        Task<NotificationStatus> GetNotificationByProviderExternalId(string providerExternalId);
        INotificationProvider GetNotificationProviderByType(Channel channel);
        Task<NotificationStatus> GetNotificationByStatusId(Guid uniqueNotificationId);
        IQueryable<NotificationStatus> GetNotifications();

        Task SendNotification(Guid uniqueNotificationId);
        Task SendPendingNotifications();

        Task UpdateNotificationStatus(Guid uniqueId, Guid providerId, HttpRequest httpRequest);
        Task UpdateNotificationStatus(long notificationId, DateTime sentAt);
        Task UpdateNotificationStatus(long notificationId, string errorMessage);
    }
}
