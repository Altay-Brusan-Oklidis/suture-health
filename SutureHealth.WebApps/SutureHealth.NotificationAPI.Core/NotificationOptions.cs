using System;

namespace SutureHealth.Notifications.Services
{
    public class NotificationOptions : SutureHealth.Services.DefaultOptions
    {
        public string CallbackUrl { get; set; }
        public string CreateQueueUrl { get; set; }
        public bool QueueProcessing { get; set; } = true;

        public Func<NotificationStatus, string> GenerateCallbackUrl { get; set; }
    }
}
