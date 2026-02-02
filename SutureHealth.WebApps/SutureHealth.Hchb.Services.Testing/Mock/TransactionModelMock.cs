using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Mock
{
    [JsonObject]
    class TransactionModelMock
    {

        [JsonProperty("OrderDate")]
        public string OrderDate { get; set; } = string.Empty;

        [JsonProperty("OrderNumber")]
        public string OrderNumber { get; set; } = string.Empty;

        [JsonProperty("FileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonProperty("effectiveDate")]
        public string EffectiveDate { get; set; } = string.Empty;

        [JsonProperty("ObservationId")]
        public string ObservationId { get; set; } = string.Empty;

        [JsonProperty("ObservationText")]
        public string ObservationText { get; set; } = string.Empty;

        [JsonProperty("TemplateId")]
        public string TemplateId { get; set; } = string.Empty;

        [JsonProperty("AdmitDate")] 
        public string AdmitDate { get; set; } = string.Empty;

        [JsonProperty("AdmissionType")] 
        public string AdmissionType { get; set; } = string.Empty;

        [JsonProperty("PatientType")] 
        public string PatientType { get; set; } = string.Empty;

        [JsonProperty("SendDate")]
        public string SendDate { get; set; } = string.Empty;
    }
}
