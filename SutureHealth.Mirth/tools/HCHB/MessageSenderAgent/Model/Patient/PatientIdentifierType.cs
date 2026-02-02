using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Patient
{
    /// <summary>
    /// Based on  PID.3
    /// </summary>
    public class PatientIdentifierType
    {
        public string? Id { get; set; }
        public string? AssigningAuthority { get; set; }
        public IdentifierCodeType? IdentifierTypeCode { get; set; }

        public override string ToString()
        {
            return Id + "^^^" + AssigningAuthority + "^" + IdentifierTypeCode;
        }
    }
}
