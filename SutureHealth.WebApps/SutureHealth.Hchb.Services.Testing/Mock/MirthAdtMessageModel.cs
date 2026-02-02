using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SutureHealth.Hchb.Services.Testing.HchbServiceProviderTest;

namespace SutureHealth.Hchb.Services.Testing.Mock
{
    public class MirthAdtMessageModel
    {
        [JsonProperty("MessageControlId")]
        public string? controlId { get; set; }

        [JsonProperty("RawFileName")] public string RawFilename { get; set; } = string.Empty;

        [JsonProperty("JsonFileName")] public string JsonFilename { get; set; } = string.Empty;

        [JsonProperty("Patient")]
        public PatientModelMock Patient { get; set; }
        [JsonProperty("Type")]
        public string? MessageType { get; set; }

        [JsonProperty("BranchCode")]
        public string? BranchCode { get; set; }

        [JsonProperty("patient_hchb")]
        public HchbPatientModelMock? HchbPatientModelMock { get; set; } =
         new HchbPatientModelMock()
         {
             admissionId = string.Empty,
             episodeId = string.Empty,
             externalId = string.Empty,
             hchbId = string.Empty,
             patientId = "0",
             status = string.Empty,
             physicianNpi = string.Empty,
             physicianFirstName = string.Empty,
             physicianLastName = string.Empty
         };
    }
}
