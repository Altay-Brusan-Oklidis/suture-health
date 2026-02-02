using System.Text.Json.Serialization;

namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class Tab
    {
        [JsonIgnore]
        public long Id { get; set; }
        [JsonPropertyName("id")]
        public string ObfuscatedId { get; set; }
        public long SignerId { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }

        public virtual Signer Signer { get; set; }
    }
}
