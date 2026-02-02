namespace SutureHealth.AspNetCore.Areas.Admin.Models.Organization
{
    public class OrganizationListItem
    {
        public int OrganizationId { get; set; }
        public string ExternalName { get; set; }
        public string InternalName { get; set; }
        public int? TypeId { get; set; }
        public string Type { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public bool IsActive { get; set; }
        public bool IsPaid { get; set; }
        public string DateClosed { get; set; }
        public long DateClosedTicks { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string MedicareNumber { get; set; }
        public string Npi { get; set; }
        public int? ParentOrganizationId { get; set; }
        public int? CompanyId { get; set; }
        public string DateCreated { get; set; }
        public long DateCreatedTicks { get; set; }
        public string EditOrganizationUrl { get; set; }
        public string SettingsActionUrl { get; set; }
        public string TemplateManagementUrl { get; set; }
    }
}
