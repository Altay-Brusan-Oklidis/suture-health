using SutureHealth.Hchb.Services.Testing.Model.Observation;
using SutureHealth.Hchb.Services.Testing.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Builder
{
    public class OBXBuilder
    {
        string? fullPathToDestinationFile;
        ObservationModel observationModel;
        public OBXBuilder(string? setId,
                          string? fileFullPath = null,
                          ObservationValueType? observationValue = null,
                          ObservationIdentifier? identifier = null)
        {
            this.observationModel = new();
            this.observationModel.SetID = setId;
            this.fullPathToDestinationFile = fileFullPath;
            this.observationModel.ValueType = observationValue == null ? observationValue : ObservationValueType.ED;
            this.observationModel.Identifier = identifier;
        }

        public ObservationModel Build()
        {

            int obx5MaxSize = 90000;
            Base64Encoder _ourBase64Helper = new Base64Encoder();
            var base64EncodedStringOfPdfReport = _ourBase64Helper.ConvertToBase64String(new FileInfo(fullPathToDestinationFile));

            observationModel.FileLines = Enumerable.Range(0, (int)Math.Ceiling((double)base64EncodedStringOfPdfReport.Length / obx5MaxSize))
                            .Select(i => base64EncodedStringOfPdfReport.Substring(i * obx5MaxSize, Math.Min(obx5MaxSize, base64EncodedStringOfPdfReport.Length - i * obx5MaxSize)));

            return observationModel;

        }

    }
}
