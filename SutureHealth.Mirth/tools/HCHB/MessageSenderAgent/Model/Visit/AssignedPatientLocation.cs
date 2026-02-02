using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Visit
{
    public class AssignedPatientLocation
    {
        /// <summary>
        /// HCHB renamed "Point Of Care" to "Agency Name"
        /// </summary>
        public string? AgencyName { get; set; }
        public string? Room { get; set;}
        /// <summary>
        /// HCHB renamed "Facility (Namespace ID)" to "Branch Code"
        /// </summary>        
        [MaxLength(20)]
        public string? BranchCode { get; set; }

        /// <summary>
        /// HCHB renamed "Facility (Universal ID)" to "Branch Name"
        /// </summary>        
        [MaxLength(199)]
        public string? BranchName { get; set; }
        /// <summary>
        /// HCHB renamed "PersonLocationType" to "Service Location"
        /// </summary>
        public string? ServiceLine { get; set; }
        /// <summary>
        /// HCHB renamed "Location Description" to "Team Name"
        /// </summary>
        [MaxLength(199)]
        public string? TeamName { get; set; } = null;
    }
}
