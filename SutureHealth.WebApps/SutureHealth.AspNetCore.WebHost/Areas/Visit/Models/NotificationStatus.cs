namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Models
{
    public class NotificationStatus
    {
        public Guid Id { get; set; }
        public string DestinationUri { get; set; }
        public DateTime OriginationDate { get; set; }
        public DateTime TerminationDate { get; set; }
        public string CallbackUrl { get; set; }
        public string SourceUrl { get; set; }
        public string SourceText { get; set; }
        public string SourceId { get; set; }
        public string StatusCode { get; set; }
        public bool? Success { get; set; }
        public bool? Complete { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public Guid? ProviderId { get; set; }
        public string ProviderType { get; set; }
        public string ProviderExternalKey { get; set; }
        public DateTime? NotificationDate { get; set; }
    }
}