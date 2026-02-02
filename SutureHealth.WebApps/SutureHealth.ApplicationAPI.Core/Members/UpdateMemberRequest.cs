using System.Collections.Generic;

namespace SutureHealth.Application.Members
{
    public class UpdateMemberRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string ProfessionalSuffix { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public string OfficePhone { get; set; }
        public string OfficePhoneExtension { get; set; }
        public string UserName { get; set; }
        public int? MemberTypeId { get; set; }
        public string Npi { get; set; }
        public string SigningName { get; set; }
        public bool? CanSign { get; set; }

        public IEnumerable<int> RelatedMemberIds { get; set; }
        public IEnumerable<OrganizationMember> OrganizationMembers { get; set; }
        public string Password { get; set; }

        public class OrganizationMember
        {
            public int OrganizationId { get; set; }
            public bool IsAdministrator { get; set; }
            public bool IsBillingAdministrator { get; set; }
            public bool IsPrimary { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
