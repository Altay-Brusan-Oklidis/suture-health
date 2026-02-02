using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Visit
{
    public class PatientType
    {
        public string? HOMEHEALTH { get; } = "HOME HEALTH";
        public string? HOSPICE { get; } = "HOSPICE";
        /// <summary>
        /// Agency can have custom Service Lines as well.
        /// </summary>
        public string? PRIVATEDUTY { get; } = "PRIVATE DUTY";

    }
}
