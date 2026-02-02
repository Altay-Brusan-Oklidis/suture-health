using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb
{
    public class RequestStatus
    {
        public int Id { get; set; }

        public int? St { get; set; }

        public System.Int16? Status { get; set; }

        public int? SubmitterId { get; set; }

        public int Patient { get; set; }

        public SuturePatient? RequestPatient { get; set; }

        /// <summary>
        /// Signed Date
        /// </summary>
        public DateTime StDate { get; set; }
    }
}
