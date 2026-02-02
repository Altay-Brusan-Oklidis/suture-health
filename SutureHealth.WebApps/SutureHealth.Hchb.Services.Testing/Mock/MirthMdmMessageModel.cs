using Newtonsoft.Json;

namespace SutureHealth.Hchb.Services.Testing.Mock
{
    class HchbPatientModel
    {
        [JsonProperty("externalId")] public string ExternalId { get; set; } = string.Empty;

        [JsonProperty("PatientId")] public string PatientId { get; set; } = string.Empty;

        [JsonProperty("admissionId")] public string AdmissionId { get; set; } = string.Empty;

        [JsonProperty("EpisodeId")] public string EpisodeId { get; set; } = string.Empty;

        [JsonProperty("physicianNpi")] public string PhysicianNpi { get; set; } = string.Empty;

        [JsonProperty("physicianFirstName")] public string PhysicianFirstName { get; set; } = string.Empty;

        [JsonProperty("physicianLastName")] public string PhysicianLastName { get; set; } = string.Empty;
    }
    
    class MirthMdmMessageModel
    {
        [JsonProperty("MessageControlId")] public string MessageControlId { get; set; }
        
        [JsonProperty("RawFileName")] public string RawFilename { get; set; }
        
        [JsonProperty("JsonFileName")] public string JsonFilename { get; set; }
        
        [JsonProperty("Patient")]
        public PatientModelMock Patient { get; set; }

        [JsonProperty("Transaction")]
        public TransactionModelMock Transaction { get; set; }

        [JsonProperty("Signer")]
        public PersonModelMock Signer { get; set; }

        [JsonProperty("Sender")]
        public PersonModelMock Sender { get; set; }

        [JsonProperty("patient_hchb")] public HchbPatientModel HchbPatient;
    }
}
