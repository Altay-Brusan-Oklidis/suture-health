using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Observation
{
    public class ObservationModel
    {
        public string? SetID { get; set; }
        public ObservationValueType? ValueType { get; set; }
        public string? ObservationValue { get; set; }
        public ObservationIdentifier? Identifier { get; set; }
        public IEnumerable<string> FileLines { get; set; } = new List<string>();
    }
}
