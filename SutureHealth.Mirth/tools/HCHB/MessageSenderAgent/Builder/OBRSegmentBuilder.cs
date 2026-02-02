using MessageSenderAgent.Model.Request;
using MessageSenderAgent.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Builder
{
    public class OBRSegmentBuilder
    {
        ObservationRequestModel observationRequestModel;
        public OBRSegmentBuilder(Model.Request.ResultStatus  sattus = ResultStatus.P ) 
        {
            observationRequestModel = new ObservationRequestModel();
            observationRequestModel.ResultStatus = sattus;
        }

        public ObservationRequestModel build() 
        {
            observationRequestModel.ObservationDateTime = Utilities.GetRandomDateTime();
            observationRequestModel.ObservationDateTime = Utilities.GetRandomDateTime();
            observationRequestModel.FillerOrderNumber = Utilities.GetRandomAlphabeticString(5);
            observationRequestModel.UniversalServiceIdentifier = new UniversalServiceIdentifierType()
            {
                Identifier = Utilities.GetRandomAlphabeticString(5),
                Text = Utilities.GetRandomAlphabeticString(5)
            };
            observationRequestModel.FillerField1 = "Demo filler field";
            observationRequestModel.ResultStatusDate = Utilities.GetRandomDateTime().UpToDateString();
           
            return observationRequestModel;
        }
    }
}
