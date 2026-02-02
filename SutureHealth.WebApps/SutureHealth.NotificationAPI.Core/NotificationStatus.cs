using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SutureHealth.Notifications
{
    public class NotificationStatus
    {
        [Key]
        public long NotificationId { get; set; }
        public Guid Id { get; set; }

        [NotMapped]
        public string ApiVersion { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Channel Channel { get; set; }
        [MaxLength(2048)]
        public string DestinationUri { get; set; }
        /// <summary>
        /// Date the notification was added to the service for processing
        /// </summary>
        public DateTime OriginationDate { get; set; }
        public DateTime TerminationDate { get; set; }
        [MaxLength(2048)]
        public string CallbackUrl { get; set; }
        [MaxLength(2048)]
        public string SourceUrl { get; set; }
        public string SourceText { get; set; }
        [MaxLength(256)]
        public string SourceId { get; set; }
        [MaxLength(256)]
        public string StatusCode { get; set; }
        public bool? Success { get; set; }
        public bool? Complete { get; set; }
        public string Subject { get; set; }
        [Column("Message", TypeName = "Text")]
        public string Message { get; set; }
        public Guid? ProviderId { get; set; }
        public string ProviderType { get; set; }
        [MaxLength(128)]
        public string ProviderExternalKey { get; set; }
        /// <summary>
        /// Date the notification was sent to the provider
        /// </summary>
        public DateTime? NotificationDate { get; set; }
        /// <summary>
        /// Date the notification should be processed
        /// </summary>
        public DateTime? DesiredSendDateTime { get; set; }
        /// <summary>
        /// Date the notification was confirmed delivered by the provider
        /// </summary>
        public DateTime? SendDateTime { get; set; }
        public string AdditionalOptionsJson { get; set; }

        [NotMapped]
        public IDictionary<string, object> AdditionalOptions
        {
            get
            {
                if (AdditionalOptionsJson == null)
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(AdditionalOptionsJson);
            }
            set
            {
                if (value != null)
                {
                    AdditionalOptionsJson = JsonConvert.SerializeObject(value);
                }
                else
                {
                    AdditionalOptionsJson = null;
                }
            }
        }
    }
}
