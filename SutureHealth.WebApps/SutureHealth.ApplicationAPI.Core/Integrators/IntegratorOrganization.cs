using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SutureHealth.Application
{
    public class IntegratorOrganization
    {
        public int IntegratorOrganizationId { get; set; }
        public int IntegratorId { get; set; }
        public int OrganizationId { get; set; }
        public string ApiKey { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? EffectiveDate { get; set; }
        public DateTimeOffset? ExpirationDate { get; set; }

        [JsonIgnore]
        public virtual Integrator Integrator { get; set; }
        [JsonIgnore]
        public virtual Organization Organization { get; set; }
    }
}
