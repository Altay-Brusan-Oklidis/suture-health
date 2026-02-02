namespace SutureHealth.AspNetCore.Areas.Admin.Models.Patient
{
    public class PatientDetailModel
    {
        public string GridName { get; set; }
        public IEnumerable<PatientDetailListItem> Organizations { get; set; }
        public AddEntityToOrganizationDialogModel AddOrganizationDialog { get; set; }

        public class PatientDetailListItem
        {
            public int OrganizationId { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
            public string RecordNumber { get; set; }
            public string EditPatientActionUrl { get; set; }
        }

        public class OrganizationListItem : OrganizationSearchListItem
        {
            public string EditPatientUrl { get; set; }
        }
    }
}
