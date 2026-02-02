using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.OData.Edm;
using SutureHealth.AspNetCore.Models;
using SutureHealth.AspNetCore.Mvc.Rendering;
using SutureHealth.Requests;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Review
{
    public class PatientMatchModel : BaseViewModel
    {
        public string SubmittedDate { get; set; }
        public PatientInfo Patient { get; set; }
        public SendingOrganizationInfo SendingOrg { get; set; }
        public string SendingMemberName { get; set; }
        public string RecipientSigner { get; set; }
        public string RecipientOrg { get; set; }
        public IList<PatientInfo> PatientMatches { get; set; }
        public long RequestId { get; set; }
        public int PatientMatchIndex { get; set; }
        public Guid UniqueRequestId { get; set; }
        public Guid UniquePatientId { get; set; }
        public bool IsAutoMerge { get; set; }
        public bool IsAutoCreate { get; set; }
        public string  Source { get; set; }

        public class PatientInfo
        {
            public string FirstName { get; set; }
            public bool IsFirstNameMatch { get; set; }
            public string MiddleName { get; set; }
            public bool IsMiddleNameMatch { get; set; }
            public string LastName { get; set; }
            public bool IsLastNameMatch { get; set; }
            public string Suffix { get; set; }
            public bool IsSuffixMatch { get; set; }
            public Date DOB { get; set; }
            public bool IsDOBMatch { get; set; }
            public PatientAddress Address { get; set; }
            public bool IsLine1Match { get; set; }
            public bool IsLine2Match { get; set; }
            public bool IsCityMatch { get; set; }
            public bool IsStateMatch { get; set; }
            public bool IsPostalCodeMatch { get; set; }
            public Gender Gender { get; set; }
            public bool IsGenderMatch { get; set; }
            public string SSN { get; set; }
            public bool IsSSNMatch { get; set; }
            public string MedicareMBI { get; set; }
            public bool IsMedicareMBIMatch { get; set; }
            public string MedicaidNumber { get; set; }
            public bool IsMedicaidNumberMatch { get; set; }
            public string MedicaidState { get; set; }
            public bool IsMedicaidStateMatch { get; set; }
            public string FacilityMRN { get; set; }

            public Patients.PatientPhone HomePhone { get; set; }
            public bool IsHomePhoneMatch { get; set; }
            public Patients.PatientPhone WorkPhone { get; set; }
            public bool IsWorkPhoneMatch { get; set; }
            public Patients.PatientPhone Mobile { get; set; }
            public bool IsMobileMatch { get; set; }
            public Patients.PatientPhone OtherPhone { get; set; }
            public bool IsOtherPhoneMatch { get; set; }

            public string ExternalMRN { get; set; }
            public bool IsMRNMatch { get; set; }
            public double MatchScore { get; set; }
            public int MatchScorePercentage { get; set; }
            public int MatchIndex { get; set; }
            public int OrganizationId { get; set; }
            public string OrganizatioName { get; set; }
            public bool IsOrganizatioNameMatch { get; set; }
            public IEnumerable<PatientContact> Contacts { get; set; }
            public int? PatientId { get; set; }
            public bool IsPrivateInsuranceAvailable { get; set; }
            public bool IsSelfPaid { get; set; }
            public bool IsMedicareAdventageAvailable { get; set; }

            public bool IsPrivateInsuranceMatch { get; set; }
            public bool IsSelfPaidMatch { get; set; }
            public bool IsMedicareAdventageMatch { get; set; }

            public IEnumerable<RequestPatientMatchLog> MatchLogs { get; set; }
            public IEnumerable<string> DocumentNames { get; set; } 
            public SelectList Facilities { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public class SendingOrganizationInfo
        {
            public string Name { get; set; }
            public string Phone { get; set; }
        }
    }
}
