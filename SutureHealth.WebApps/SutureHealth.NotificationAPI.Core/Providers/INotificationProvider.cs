using System;
using System.Threading.Tasks;

namespace SutureHealth.Notifications
{
    public interface INotificationProvider
    {
        Guid ProviderId { get; }
        Channel Channel { get; }
        bool Preferred { get; }

        Task HandleCallbackAsync(NotificationStatus notification, Microsoft.AspNetCore.Http.HttpRequest httpRequest);
        Task SendNotificationAsync(NotificationStatus notification);
    }
}
