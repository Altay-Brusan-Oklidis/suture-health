using SutureHealth.Patients.JsonConverters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SutureHealth.Patients.ADT.Kno2
{
    public partial class Patient
    {
        public Patient()
        {
            Messages = new HashSet<Message>();
        }

        [JsonIgnore]
        public long Id { get; set; }
        [JsonPropertyName("patientId")]
        public string ObfuscatedId { get; set; }
        [JsonPropertyName("patientIdRoot")]
        public string ObfuscatedPatientIdRoot { get; set; }
        [JsonConverter(typeof(JsonConverterString))]
        public string PatientIds { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Gender { get; set; }
        public string VisitId { get; set; }
        public string Issuer { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? VisitDate { get; set; }
        public string FullName { get; set; }
        public string Zip { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Telephone { get; set; }
        public string IntegrationMeta { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}
