using Microsoft.OData.Edm;
using SutureHealth.Requests;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Review
{
    public class AssociateMergeRequest
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public bool IsMiddleNameMatch { get; set; } = true;
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public bool IsSuffixMatch { get; set; } = true;
        public string DOB { get; set; }
        public string Line1 { get; set; }
        public bool IsLine1Match { get; set; } = true;
        public string Line2 { get; set; }
        public bool IsLine2Match { get; set; } = true;
        public string City { get; set; }
        public bool IsCityMatch { get; set; } = true;
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Gender { get; set; }
        public string OrganizationName { get; set; }
        public string SSN { get; set; } // place holder for both SSN and SSN4
        // is true when SSN is replaced by SSN4/Null or SSN4 replaced by Null
        public bool IsSSNDownCasted { get; set; } = false;
        public string MedicareMBI { get; set; }
        public string MedicaidNumber { get; set; }
        public string FacilityMRN { get; set; }
        public string SelectedPatientId { get; set; }
        public bool? IsSelfPaid { get; set; }

        public string HomePhone { get; set; }
        public string WorkPhone { get; set; }
        public string Mobile { get; set; }
        public string OtherPhone { get; set; }
        public string PrimaryPhone { get; set; }
        public bool IsHomePhoneMatch { get; set; } = true;
        public bool IsWorkPhoneMatch { get; set; } = true;
        public bool IsMobileMatch { get; set; } = true;
        public bool IsOtherPhoneMatch { get; set; } = true;
        public bool? IsPrivateInsuranceAvailable { get; set; }
    }
}
