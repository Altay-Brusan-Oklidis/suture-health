using NHapi.Base.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent
{
    public class MessageFactory
    {
        //you will pass in parameters here in the form of a DTO or domain object
        //for message construction in your implementation
        public static IMessage CreateMessage()
        {    
            return new ORU_R01MessageBuilder().Build();
            //return new MDM_T02MessageBuilder().Build();
            //return new ADT_MessageBuilder().Build(triggerEvent:Model.Header.TriggerEvent.A09);

        }
    }
}
