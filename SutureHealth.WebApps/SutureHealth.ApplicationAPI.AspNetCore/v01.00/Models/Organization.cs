using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Application.v0100.Models
{
    public class Organization
    {
        public int OrganizationId { get; set; }
        public string NPI { get; set; }
        public string MedicareNumber { get; set; }

        public int? ParentId { get; set; }
        public int? CompanyId { get; set; }
        public int? OrganizationTypeId { get; set; }
        public string Name { get; set; }
        public string OtherDesignation { get; set; }
        public bool IsActive { get; set; }
        public bool IsFree { get; set; }
        public bool IsSender { get; set; }
        public bool IsSigner { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string StateOrProvince { get; set; }
        public string CountryOrRegion { get; set; }
        public string PostalCode { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public virtual OrganizationType OrganizationType { get; set; }
        public virtual ICollection<OrganizationContact> Contacts { get; set; }

        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class OrganizationalUnit {\n");
            sb.Append("  Id: ").Append(OrganizationId).Append("\n");
            sb.Append("  Other Designation: ").Append(OtherDesignation).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    public class OrganizationContact : ContactInfo<int, Organization, int>
    {
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class CustomOrganization : Organization
    {
        public byte[] Logo { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsCurrentUserAdmin { get; set; } = false;
        public bool IsSubscribedToInboxMarketing { get; set; }
    }
}
