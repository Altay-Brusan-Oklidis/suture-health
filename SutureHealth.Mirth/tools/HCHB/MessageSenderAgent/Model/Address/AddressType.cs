using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Address
{
    public class AddressType
    {
        /// <summary>
        /// HCHB renamed "Street Address" (Street or Mailing Address) to  "Address"
        /// </summary>
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        /// <summary>
        /// HCHB renamed "State or Province" to "State"
        /// </summary>
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        /// <summary>
        /// HCHB renamed "Address Type" to "Facility Type"
        /// </summary>
        public FascilityType? FacilityType { get; set; }
        /// <summary>
        /// HCHB renamed "Other Geographic Designation" to "Facility Name"
        /// </summary>
        public string? FacilityName { get; set; }
        /// <summary>
        /// HCHB renamed "County Parish Code" to "Facility ID"
        /// </summary>
        public string? FacilityId { get; set; }

    }
}
