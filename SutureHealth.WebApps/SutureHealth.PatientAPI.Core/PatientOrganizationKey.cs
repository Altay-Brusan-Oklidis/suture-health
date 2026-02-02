using System;
namespace SutureHealth.Patients
{
    public class PatientOrganizationKey
    {
        public int OrganizationId { get; set; }
        public string MedicalRecordNumber { get; set; }
        public int PatientId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public bool IsActive { get; set; }

        public virtual Patient Patient { get; set; }
    }
}