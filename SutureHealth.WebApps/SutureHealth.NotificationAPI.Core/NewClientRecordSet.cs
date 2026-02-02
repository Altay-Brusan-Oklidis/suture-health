using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SutureHealth.Notifications
{
    public class NewClientRecordSet
    {
        [JsonPropertyName("lat")]
        public decimal Latitude { get; set; }
        [JsonPropertyName("long")]
        public decimal Longitude { get; set; }
        [JsonPropertyName("emails")]
        public string[] EmailAddresses { get; set; }
    }
}
