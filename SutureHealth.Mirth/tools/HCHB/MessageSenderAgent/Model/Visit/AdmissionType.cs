using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Visit
{
    public static class AdmissionType
    {
        public static string? NEW_ADMISSION { get; } = "NEW ADMISSION";
        public static string? READMISSION { get; } = "READMISSION";
        public static string? RECERTIFICATION { get; } = "RECERTIFICATION";
        public static string? BEREAVEMENT { get; } = "BEREAVEMENT";
        public static string? TRANSITION { get; } = "TRANSITION";        
    }
}
