using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SutureHealth.Hchb.Services.Testing.Model.Request
{
    /// <summary>
    /// HL7 Table - 0123 - Result Status
    /// 
    /// HL7 Standard Tables - Tables defined by the HL7 organization. 
    /// Most tables in this category are related to the standard and 
    /// its Message structures.
    /// </summary>
    public enum ResultStatus
    {
        A, // Some, but not all, results available
        C, // Correction to results
        F, // Final results; results stored and verified.Can only be changed with a corrected result.
        I, // No results available; specimen received, procedure incomplete
        O, // Order received; specimen not yet received
        P, // Preliminary: A verified early result is available, final results not yet obtained
        R, // Results stored; not yet verified
        S, // No results available; procedure scheduled, but not done
        X, // No results available; Order canceled.
        Y, // No order on record for this test.  (Used only on queries)
        Z, // No record of this Patient. (Used only on queries)
    }
}
