using SutureHealth.Patients.JsonConverters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class Message
    {
        public Message()
        {
            Attachments = new HashSet<Attachment>();
            Signers = new HashSet<Signer>();
        }

        [JsonIgnore]
        public long Id { get; set; }
        [JsonPropertyName("id")]
        public string ObfuscatedId { get; set; }
        public long? ThreadId { get; set; }
        public string OrganizationId { get; set; }
        public string OriginalObjectId { get; set; }
        public DateTime? MessageDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string PatientName { get; set; }
        [JsonConverter(typeof(JsonConverterString))]
        public string Properties { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public long? PatientId { get; set; }
        public string ReasonForDisclosure { get; set; }
        public string Origin { get; set; }
        public bool? IsProcessed { get; set; }
        public bool? IsUrgent { get; set; }
        public bool? IsDraft { get; set; }
        public string Status { get; set; }
        public string ProcessedType { get; set; }
        [JsonConverter(typeof(JsonConverterString))]
        public string ProcessTypes { get; set; }
        public string Priority { get; set; }
        public string ChannelId { get; set; }
        public string SourceType { get; set; }
        public DateTime? UnprocessedNotificationSent { get; set; }
        public bool? Attachments2Pdf { get; set; }
        public bool? Attachments2Cda { get; set; }
        public bool? Attachments2Hl7 { get; set; }
        public string AttachmentSendType { get; set; }
        public string ReleaseTypeId { get; set; }
        public bool? IsNew { get; set; }
        public long? ConversationId { get; set; }
        public string Type { get; set; }
        public string MessageType { get; set; }
        public long? ClassificationId { get; set; }
        [JsonConverter(typeof(JsonConverterString))]
        public string HispMessageIds { get; set; }

        public virtual Classification Classification { get; set; }
        public virtual Conversation Conversation { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<Signer> Signers { get; set; }
    }
}
