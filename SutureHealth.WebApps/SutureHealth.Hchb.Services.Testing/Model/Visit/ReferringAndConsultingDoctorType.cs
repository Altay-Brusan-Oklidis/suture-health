using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Visit
{
    public class ReferringAndConsultingDoctorType
    {
        /// <summary>
        /// HCHB renamed "ID Number" to "NPI"
        /// </summary>
        [MaxLength(15)]
        public string? NPI { get; set; }

        /// <summary>
        /// HCHB renamed "FamilyName (Surname)" to "Last Name"
        /// </summary>
        [MaxLength(194)]
        public string? LastName { get; set; }

        /// <summary>
        /// HCHB renamed "Given Name" to "First Name"
        /// </summary>
        [MaxLength(30)]
        public string? FirstName { get; set; }
        
        [MaxLength(5)]
        public DegreeLicenseCertificateType? Degree { get; set; }
    }
}
