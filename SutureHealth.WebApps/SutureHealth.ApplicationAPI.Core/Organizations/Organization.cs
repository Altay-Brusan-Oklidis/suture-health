using System;
using System.Collections.Generic;
using System.Text;

namespace SutureHealth.Application
{
    public class Organization 
    {
        public Organization()
        {
            Contacts = new List<OrganizationContact>();
        }

        public int OrganizationId { get; set; }
        public string NPI { get; set; }
        public string MedicareNumber { get; set; }
        public string PhoneNumber { get; set; }

        public int? ParentId { get; set; }
        public int? CompanyId { get; set; }
        public int? OrganizationTypeId { get; set; }
        public string Name { get; set; }
        public string OtherDesignation { get; set; }
        public bool IsActive { get; set; }
        public bool IsFree { get; set; }

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

    public class OrganizationContact : ContactInfo<int, Organization,int>
    {
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
    }
}
