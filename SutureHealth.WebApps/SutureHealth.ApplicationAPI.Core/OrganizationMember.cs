using System;
using System.Text.Json.Serialization;

namespace SutureHealth.Application
{
    public class OrganizationMember
    {
        public int OrganizationMemberId { get; set; }
        public int OrganizationId { get; set; }
        public int MemberId { get; set; }
        public bool CanEditProfile { get; set; }
        public bool CanSign { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsAutomatedUser { get; set; }
        public bool IsBillingAdministrator { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? EffectiveAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }

        [JsonIgnore]
        public virtual Organization Organization { get; set; }
        [JsonIgnore]
        public virtual Member Member { get; set; }
    }
}
