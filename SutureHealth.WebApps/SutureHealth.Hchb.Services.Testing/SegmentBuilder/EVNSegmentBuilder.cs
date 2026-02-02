using SutureHealth.Hchb.Services.Testing.Utility;
using SutureHealth.Hchb.Services.Testing.Model.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Builder
{
    public class EVNSegmentBuilder
    {
        EventModel eventModel;
        DateTime? recordedDateTime = null;
        DateTime? dateTimePlannedEvent = null;
        public EVNSegmentBuilder(EventCodeType eventCode,
                                  DateTime? recordedDateTime = null,
                                  DateTime? dateTimePlannedEvent = null,
                                  string? reason = null)
        {
            eventModel = new EventModel();

            eventModel.EventTypeCode = eventCode;
            this.recordedDateTime = recordedDateTime;
            this.dateTimePlannedEvent = dateTimePlannedEvent;
            this.eventModel.EventReasonCode = !String.IsNullOrEmpty(reason) ? reason : EventReasonType.Unknown;
        }
        public EventModel Build()
        {
            if (recordedDateTime is null)
                eventModel.RecordedDateTime = Utilities.GetRandomDateTime().UpToMinuteString();
            else
                eventModel.RecordedDateTime = recordedDateTime.Value.UpToMinuteString();

            if (dateTimePlannedEvent is null)
                eventModel.DateTimePlannedEvent = Utilities.GetRandomDateTime().UpToMinuteString();
            else
                eventModel.DateTimePlannedEvent = dateTimePlannedEvent.Value.UpToMinuteString();

            // TODO: may not be best practice to use "Now". 
            eventModel.EventOccurred = DateTime.Now.UpToMinuteString();

            return eventModel;
        }
        public EventModel GetModel()
        {
            return eventModel;
        }

    }
}
