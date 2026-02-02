using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Patients
{
    public class Organization
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; }
        public virtual IEnumerable<MatchLog> MatchLogs { get; set; }
    }

}
