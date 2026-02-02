using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Notification
{
    /// <summary>
    /// HL7 Table - 0271 - Document completion status
    /// 
    /// <warning>
    /// HCHB default value is UNAUTH. This item is not available in HL7 
    /// default standard and it is included manually. 
    /// </warning>
    /// HL7 Standard Tables - Tables defined by the HL7 organization.
    /// Most tables in this category are related to the standard and its message structures.
    /// </summary>
    public enum CompletionStatus
    {
        AU,  // Authenticated
        DI,  // Dictated
        DO,  // Documented
        IN,  // Incomplete
        IP,  // In Progress
        LA,  // Legally authenticated
        PA,  // Pre-authenticated
        UNAUTH // default value used by HCHB
    }
}
