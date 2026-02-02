namespace SutureHealth.AspNetCore.Areas.Admin.Models.Patient
{
    public class PatientListItem
    {
        public int PatientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string DateOfBirthDisplay { get; set; }
        public string SocialSecurityNumber { get; set; }
        public string OrganizationIds { get; set; }
        public string PatientRecordNumbers { get; set; }
        public string DateCreated { get; set; }
        public long DateCreatedTicks { get; set; }
        public string DateModified { get; set; }
        public long DateModifiedTicks { get; set; }
        public bool IsActive { get; set; }
        public string DetailUrl { get; set; }
    }
}
