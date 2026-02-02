using SutureHealth.Hchb.Services.Testing.Utility;
using SutureHealth.Hchb.Services.Testing.Model.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.PatientAPI.Services.Testing.Builder
{
    public class MSHSegmentBuilder
    {
        HeaderModel messageHeader;
        string medicalRecordNumber;

        public MSHSegmentBuilder(HCHBMessageType type, string medicalRecordNumber, ProcessingIdType? idType = null)
        {
            messageHeader = new HeaderModel();

            messageHeader.MessageType = type;
            this.medicalRecordNumber = medicalRecordNumber;

            if (idType.HasValue)
                messageHeader.ProcessingID = idType.Value;
            else
                messageHeader.ProcessingID = ProcessingIdType.P;
        }

        public HeaderModel Build()
        {
            messageHeader.SendingApplication = "253fJxdx5W24RrpFKQiN9A==";//"SutureHealth";//Utilities.GetRandomString(20);
            messageHeader.SendingFacility = "STVK8KTVK78VH82APKG87VTQEG";//"testing";//Utilities.GetGuid().Substring(0,20);//Utilities.GetRandomString(20);
            messageHeader.ReceivingApplication = "hchb";//Utilities.GetRandomString(20);
            //messageHeader.MessageControlID = Utilities.GetRandomString(20);            
            messageHeader.MessageControlID = DateTime.Now.UpToMinuteString() + medicalRecordNumber;

            return messageHeader;
        }


    }
}
