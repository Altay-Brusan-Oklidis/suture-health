using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Mock
{
    [JsonObject]
    public class PersonModelMock
    {
        [JsonProperty("Npi")]
        public string? NPI { get; set; } = string.Empty;

        [JsonProperty("FirstName")]
        public string? FirstName { get; set; } = string.Empty;

        [JsonProperty("LastName")]
        public string? LastName { get; set; } = string.Empty;

        [JsonProperty("BranchCode")]
        public string? BranchCode { get; set; } = string.Empty;
    }
}
