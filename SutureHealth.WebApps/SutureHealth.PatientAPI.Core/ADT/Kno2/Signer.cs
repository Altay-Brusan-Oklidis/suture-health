using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class Signer
    {
        public Signer()
        {
            AdditionalNotifications = new HashSet<AdditionalNotification>();
            Tabs = new HashSet<Tab>();
        }

        [JsonIgnore]
        public long Id { get; set; }
        [JsonPropertyName("id")]
        public string ObfuscatedId { get; set; }
        public long MessageId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string HostName { get; set; }
        public string HostEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string SignerType { get; set; }
        public int? Order { get; set; }
        public string Role { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public virtual Message Message { get; set; }
        public virtual ICollection<AdditionalNotification> AdditionalNotifications { get; set; }
        public virtual ICollection<Tab> Tabs { get; set; }
    }
}
