using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Model.Request
{
    public class ObservationRequestModel
    {
        public string SetId { get; set; }
        [MaxLength(22)]
        public string? FillerOrderNumber { get; set; }
        [MaxLength(22)]
        public UniversalServiceIdentifierType? UniversalServiceIdentifier { get; set; }
        public DateTime? RequestedDateTime { get; set; }
        public DateTime? ObservationDateTime { get; set; }
        public string? FillerField1 { get; set; }
        public string? ResultStatusDate { get; set; }
        public ResultStatus? ResultStatus { get; set; }


    }
}
