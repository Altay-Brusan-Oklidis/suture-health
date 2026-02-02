namespace SutureHealth.AspNetCore.Areas.Admin.Models.Review
{
    public class ReviewGridListItem
    {
        public DateTime DateSubmitted { get; set; }
        public int PatientId { get; set; }
        public int MatchPatientLogID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string MaidenName { get; set; }
        public DateTime Birthdate { get; set; }
        public Gender Gender { get; set; }
        public string SocialSecurityNumber { get; set; }
        public string SocialSecuritySerial { get; set; }
        public string MedicareNumber { get; set; }
        public string MedicaidNumber { get; set; }
        public string MedicaidState { get; set; }
        public int MemberId { get; set; }
        public string SubmittedFacilityName { get; set; }
        public string ExistingFacilityName { get; set; }
        public string PatientMatchUrl { get; set; }
        public bool IsPatientMatched { get; set; }
        public string AddressStreetLine1 { get; set; }
        public string AddressCity { get; set; }
        public string AddressState { get; set; }
        public string AddressZipCode { get; set; }
        public string Source { get; set; }

    }


}
