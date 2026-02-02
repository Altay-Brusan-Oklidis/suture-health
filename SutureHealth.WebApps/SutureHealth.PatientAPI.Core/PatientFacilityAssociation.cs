using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Patients
{
    public class PatientFacilityAssociation
    {
        public int PatientId { set; get; }
        public int FacilityId { set; get; }
        public string? FacilityMRN { set; get; }
        public string? CompanyMRN { set; get; }
        public bool? Active { set; get; }
        public DateTime? CreateDate { set; get; }
        public DateTime? ChangeDate { set; get; }
        public int? ChangeBy { set; get; }
    }
}
