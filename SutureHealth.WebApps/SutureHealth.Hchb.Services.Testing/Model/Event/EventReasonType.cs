using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Event
{
    /// <summary>
    /// HL7 Table - 0062 - Event reason
    /// </summary>
    public static class EventReasonType
    {
        public static readonly string PatientRequest = "01";
        public static readonly string PhysicianOrder = "02";
        public static readonly string CensusManagement = "03";
        public static readonly string Other = "O";
        public static readonly string Unknown = "U";
    }
}
