using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SutureHealth.Application
{
    public class Integrator
    {
        public int IntegratorId { get; set; }
        public string ApiKey { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }

        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }

        [NotMapped]
        public virtual ICollection<IntegratorContact> Contacts { get; set; }
        public virtual ICollection<IntegratorOrganization> Organizations { get; set; }
    }

    public class IntegratorContact : ContactInfo<int, Integrator, int> { }
}
