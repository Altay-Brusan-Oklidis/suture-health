using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Patient
{
    /// <summary>
    /// HL7 Table - 0002 - Marital Status
    /// </summary>
    [Serializable]
    public enum MaritalStatus
    {
        A, //  Separated
        B, //  Unmarried
        C, //  Common law
        D, //   Divorced
        E, //   Legally Separated
        G, //   Living together
        I, //   Interlocutory
        M, //   Married
        N, //   Annulled
        O, //   Other
        P, //   Domestic partner
        R, //   Registered domestic partner
        S, //   Single
        T, //   Unreported
        U, //  Unknown
        W  // Widowed
    };
}
