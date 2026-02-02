namespace SutureHealth.Application.Organizations
{
    public class CreateOrganizationRequest
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
        public int? CompanyId { get; set; }
    }
}
