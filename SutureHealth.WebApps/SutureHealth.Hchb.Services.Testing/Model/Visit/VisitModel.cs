using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Visit
{
    public class VisitModel
    {
        public string SetId { get; set; }
        [Required]
        public string PatientClass { get; } = "O";
        public AssignedPatientLocation? AssignedPatientLocation { get; set; }
        public string? AdmissionType { get; set; }
        public AttendingDoctorType? AttendingDoctor { get; set; }
        public ReferringAndConsultingDoctorType? ReferringDoctor { get; set; }
        public ReferringAndConsultingDoctorType? ConsultingDoctor { get; set; }
        public string? PatientType { get; set; }
        /// <summary>
        /// HCHB Episode ID
        /// </summary>
        public string? VisitNumber { get; set; }
        /// <summary>
        /// refer to HL7 Table 0112
        /// </summary>
        [MaxLength(3)]
        public string? DischargeDisposition { get; set; } = "001";
        [MaxLength(47)]
        public string? DischargedToLocation { get; set; } = "004";
        [MaxLength(2)]
        public AccountStatusType? AccountStatus { get; set; }
        public string? AdmitDateTime { get; set; }
        public string? DischargeDateTime { get; set; }
    }
}
