using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Header
{
    /// <summary>
    /// HL7 Table - 0103 - Processing ID
    /// </summary>
    public enum ProcessingIdType
    {
        D, //Debugging
        P, //Production         
        T  //Training
    }
}
