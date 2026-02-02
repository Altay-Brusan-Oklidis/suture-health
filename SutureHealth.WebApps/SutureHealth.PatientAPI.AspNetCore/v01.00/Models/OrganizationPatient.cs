using System;

namespace SutureHealth.Patients.v0100.Models
{
   /// <summary>
   /// Class <see cref="OrganizationPatient"/> used for define the property of patient organization
   /// </summary>
   public class OrganizationPatient
    {
        public long Id { get; set; }

        public Patient Patient { get; set; }
        public long PatientId { get; set; }
        /// <summary>
        /// Gets or Sets the property of OrganizationUnitId
        /// </summary>
        public long OrganizationUnitId { get; set; }
        /// <summary>
        /// Gets or Sets the property of MasterOrganizationPatientId
        /// </summary>
        public long? MasterOrganizationPatientId { get; set; }
        /// <summary>
        /// Gets or Sets the property of IsPreferred
        /// </summary>
        public bool IsPreferred { get; set; }

        /// <summary>
        /// Gets or Sets the value of Created.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// Gets or Sets the value of Created.
        /// </summary>
        public DateTimeOffset? Updated { get; set; }
    }
}
