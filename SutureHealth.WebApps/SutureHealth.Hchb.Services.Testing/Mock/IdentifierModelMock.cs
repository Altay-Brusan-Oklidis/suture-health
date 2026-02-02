using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Mock
{
    [JsonObject]
    public class IdentifierModelMock
    {
        [JsonProperty("Type")]
        public string? Type { get; set; }
        [JsonProperty("value")]
        public string? Value { get; set; }
    }
}
