using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class Conversation
    {
        public Conversation()
        {
            Messages = new HashSet<Message>();
        }

        [JsonIgnore]
        public long Id { get; set; }
        [JsonPropertyName("id")]
        public string ObfuscatedId { get; set; }
        public string ConversationStatus { get; set; }
        public string Type { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}
