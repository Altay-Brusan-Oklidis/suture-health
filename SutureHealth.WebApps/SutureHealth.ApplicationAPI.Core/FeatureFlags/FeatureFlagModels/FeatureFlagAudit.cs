using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Application
{
    public class FeatureFlagAudit
    {
        public int Id { get; set; }
        public int FeatureFlagId { get; set; }
        public FeatureFlag FeatureFlag { get; set; }

        public DateTime CreateDate { get; set; }
        public bool OldActive { get; set; }
        public bool NewActive { get; set; }
        public bool OldHasCohort { get; set; }
        public bool NewHasCohort { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime DeleteionDate { get; set; }
        public bool IsRestored { get; set; }
        public DateTime RestorationDate { get; set; }
    }

}
