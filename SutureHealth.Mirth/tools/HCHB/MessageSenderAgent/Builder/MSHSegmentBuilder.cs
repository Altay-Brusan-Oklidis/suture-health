using MessageSenderAgent.Model.Header;
using MessageSenderAgent.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Builder
{
    public class MSHSegmentBuilder
    {
        HeaderModel messageHeader;        
        string medicalRecordNumber;     
      
        public MSHSegmentBuilder(MessageType type,string medicalRecordNumber, ProcessingIdType? idType = null) 
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
            messageHeader.SendingApplication = "SutureHealth";//Utilities.GetRandomString(20);
            messageHeader.SendingFacility = "SutureHealth";//Utilities.GetGuid().Substring(0,20);//Utilities.GetRandomString(20);
            messageHeader.ReceivingApplication = "SutureHealth";//Utilities.GetRandomString(20);
            //messageHeader.MessageControlID = Utilities.GetRandomString(20);            
            messageHeader.MessageControlID = DateTime.Now.UpToMinuteString() + medicalRecordNumber;

            return messageHeader;
        }


    }
}
