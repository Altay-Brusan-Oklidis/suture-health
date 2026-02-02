using System;

namespace SutureHealth.Application.Organizations
{
    public class UpdateOrganizationRequest
    {
        public string Name { get; set; }
        public string OtherDesignation { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string StateOrProvince { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Npi { get; set; }
        public string MedicareNumber { get; set; }
        public int? OrganizationTypeId { get; set; }
        public bool CompanyIdSpecified { get; set; } = false;
        public int? CompanyId { get; set; }
        public bool ClosedAtSpecified { get; set; } = false;
        public DateTime? ClosedAt { get; set; }
    }
}
