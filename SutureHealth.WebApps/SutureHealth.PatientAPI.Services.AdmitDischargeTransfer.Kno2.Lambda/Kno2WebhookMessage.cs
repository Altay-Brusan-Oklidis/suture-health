namespace SutureHealth.PatientAPI.Services.Kno2.Lambda
{
    public class Kno2WebhookMessage
    {
        public string Id { get; set; }

        public string Event { get; set; }

        public string Url { get; set; }

        public string? Status { get; set; }
    }
}
