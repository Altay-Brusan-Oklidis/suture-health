using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb
{
    public class HchbTemplate
    {
        public int Id { get; set; }
        public string AdmissionType { get; set; }
        public string ObservationId { get; set; }
        public string ObservationText { get; set; }
        public string PatientType { get; set; }
        public int TemplateId { get; set; }
    }
}
