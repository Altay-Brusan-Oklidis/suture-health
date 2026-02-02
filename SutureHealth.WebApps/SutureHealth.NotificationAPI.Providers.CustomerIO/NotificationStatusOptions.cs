namespace SutureHealth.Notifications.Providers.CustomerIO
{
    public class NotificationStatusOptions
    {
        public enum EmailType
        {
            Text = 0,
            Template
        }

        public EmailType Type { get; set; }
    }
}
