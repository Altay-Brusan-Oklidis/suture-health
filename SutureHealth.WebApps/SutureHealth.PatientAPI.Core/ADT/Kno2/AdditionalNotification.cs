using System.Text.Json.Serialization;

namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class AdditionalNotification
    {
        [JsonIgnore]
        public long Id { get; set; }
        [JsonPropertyName("id")]
        public string ObfuscatedId { get; set; }
        public long SignerId { get; set; }
        public string NotificationType { get; set; }
        public string Value { get; set; }

        public virtual Signer Signer { get; set; }
    }
}
