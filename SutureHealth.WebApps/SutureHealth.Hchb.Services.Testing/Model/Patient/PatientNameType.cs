using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Patient
{
    [Serializable]
    public class PatientNameType
    {        
        /// <summary>
        /// HCHB renamed the HL7's "GivedName" to "FirstName" 
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// HCHB renamed the HL7's "LastName" to "FamilyName"
        /// </summary>
        public string? FamilyName { get; set; }

        /// <summary>
        /// HCHB renamed the HL7's "Second and Further Given Names or Initials Thereof" to "MiddleInitial"
        /// </summary>
        public string? MiddleInitial { get; set; }

    }
}
