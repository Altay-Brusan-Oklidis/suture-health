using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Visit
{
    public class AttendingDoctorType
    {
        /// <summary>
        /// HCHB renamed "Id Number" to "NPI"
        /// </summary>
        [MaxLength(15)]
        public string? NPI { get; set; }
        /// <summary>
        /// HCHB renamed "Family Name (Surname)" to "Last Name"
        /// </summary>
        [MaxLength(194)]
        public string? LastName { get; set; }
        /// <summary>
        /// HCHB renmaed "Given Name" to "First Name"
        /// </summary>
        [MaxLength(30)]
        public string? FirstName { get; set; }
    }
}
