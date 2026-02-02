using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Mock
{
    [JsonObject]
    public class PatientModelMock
    {
        [JsonProperty("FirstName")]
        public string? FirstName { get; set; } = string.Empty;

        [JsonProperty("middleName")]
        public string? MiddleName { get; set; } = string.Empty;

        [JsonProperty("LastName")]
        public string? LastName { get; set; } = string.Empty;

        [JsonProperty("BirthDate")]
        public string? birthDate { get; set; } = string.Empty;

        [JsonProperty("Gender")]
        public string? Gender { get; set; } = string.Empty;

        [JsonProperty("addressLine1")]
        public string? AddressLine1 { get; set; } = string.Empty;

        [JsonProperty("addressLine2")]
        public string? AddressLine2 { get; set; } = string.Empty;

        [JsonProperty("city")]
        public string? City { get; set; } = string.Empty;

        [JsonProperty("state")]
        public string? State { get; set; } = string.Empty;

        [JsonProperty("postalCode")]
        public string? PostalCode { get; set; } = string.Empty;

        [JsonProperty("identifiers")]
        public List<IdentifierModelMock> identifiers { get; set; } = new List<IdentifierModelMock>();

        [JsonProperty("phoneNumber")]
        public PhoneNumber? Phonenumber { get; set; } = new PhoneNumber();

    }
}
