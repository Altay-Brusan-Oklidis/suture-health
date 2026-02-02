using SutureHealth.Patients.JsonConverters;
using System.Text.Json.Serialization;

namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class Attachment
    {
        [JsonIgnore]
        public long Id { get; set; }
        [JsonPropertyName("id")]
        public string ObfuscatedId { get; set; }
        [JsonIgnore]
        public long MessageId { get; set; }
        [JsonPropertyName("messageId")]
        public string ObfuscatedMessageId { get; set; }
        public string Key { get; set; }
        public string DocumentType { get; set; }
        public string MimeType { get; set; }
        public string FileName { get; set; }
        public long? SizeInBytes { get; set; }
        [JsonConverter(typeof(JsonConverterString))]
        public string IntegrationMeta { get; set; }
        public bool? IsClone { get; set; }
        public bool? IsPreviewAvailable { get; set; }
        public bool? IsRestorable { get; set; }
        public string PreviewKey { get; set; }
        public string PreviewAvailable { get; set; }
        public string Recipients { get; set; }
        public string Transforms { get; set; }
        public string TransformStatus { get; set; }

        public virtual Message Message { get; set; }
    }
}
