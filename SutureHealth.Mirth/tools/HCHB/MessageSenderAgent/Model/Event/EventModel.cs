using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Event
{
    public class EventModel
    {
        [Required]
        public EventCodeType EventTypeCode { get; set; }
        [Required]
        public string RecordedDateTime { get; set; }
        public string? DateTimePlannedEvent { get; set; }
        [Required]
        public string EventReasonCode { get; set; } = EventReasonType.Unknown;
        [Required]
        public string EventOccurred { get; set; }


    }
}
