using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb
{
    public class SutureTask
    {
        public int TaskId { get; set; }

        public int? FormId { get; set; }

        public int? UserId { get; set; }

        public int? FacilityId { get; set; }

        public int? ActionId { get; set; }

        public int? TemplateId { get; set; }

        public int? PatientId { get; set; }

        public int? OutcomeId { get; set; }

        public int? RuleId { get; set; }

        public int? SubmittedBy { get; set; }

        public string Data { get; set; }

        public DateTime CreateDate { get; set; }

        public bool Active { get; set; }

        public int? SubmittedByFacility { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? StartOfCare { get; set; }

        public int? Icd9CodeId { get; set; }
    }
}
