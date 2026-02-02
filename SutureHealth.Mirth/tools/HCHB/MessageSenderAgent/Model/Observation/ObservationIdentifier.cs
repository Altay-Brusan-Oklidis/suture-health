using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Observation
{
    public class ObservationIdentifier
    {
        /// <summary>
        /// HCHB renamed "Identifier" to "Order Type Id"
        /// </summary>
        public string? OrderTypeId { get; set; }
        /// <summary>
        /// HCHB renamed "Text" to  "Order Type Code"
        /// </summary>
        public string? OrderTypeCode { get; set; }
        public NameofCodingSystem? CodingSystem { get; set; }

    }
}
