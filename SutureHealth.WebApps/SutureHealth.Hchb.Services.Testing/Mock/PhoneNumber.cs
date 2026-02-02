using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Mock
{
    [JsonObject]
    public class PhoneNumber
    {
        public string? home { get; set; } = string.Empty;
        public string? business { get; set; } = string.Empty;
    }
}
