using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Patient
{
    /// <summary>
    /// HL7 Table 0001 - Administrative Sex
    /// 
    /// HCHB does not suppor O (Other), N (Not applicable), A (Ambiguous)
    /// </summary>
    public enum GenderType
    {
        F, //Female
        M, //Male
        U  //Unknown
    }
}
