using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Patient
{
    /// <summary>
    /// HL7 Table - 0136 - Yes/no indicator
    /// </summary>
    [Serializable]
    public enum PatientDeathIndicator
    {
        N, //no
        Y  //Yes
    }
}
