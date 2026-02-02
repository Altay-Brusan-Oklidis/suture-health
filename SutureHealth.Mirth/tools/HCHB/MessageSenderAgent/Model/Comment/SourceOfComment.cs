using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Comment
{
    /// <summary>
    /// HL7 Table - 0105 - Source of comment
    /// 
    /// <warning>
    /// HCHB default value is HCHB. This item is not available in HL7 
    /// default standard and it is included manually. 
    /// </warning>
    /// </summary>
    public enum SourceOfComment
    {
        L, //   Ancillary (filler) department is source of comment
        O, //   Other system is source of comment
        P, //   Orderer (placer) is source of comment
        HCHB
    }
}
