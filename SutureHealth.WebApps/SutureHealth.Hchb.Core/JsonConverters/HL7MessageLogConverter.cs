using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.JsonConverters
{
    public class HL7MessageLogConverter : JsonConverter<HL7MessageLog>
    {
        public override HL7MessageLog ReadJson(JsonReader reader, Type objectType, HL7MessageLog existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            HL7MessageLog log = new HL7MessageLog();
            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);
                log.Id = token.Value<int>("id");
                log.MessageControlId = token.Value<string>("messageControlId");
                //log.Type = token.Value<string>("type");
                log.SubType = token.Value<string>("type");
                log.Message = token.Value<string>("message");
                log.RawMessageFile = (token.Value<string>("rawfilename"))?.Trim();
                log.JsonMessageFile = (token.Value<string>("jsonfilename"))?.Trim();                
                log.IsProcessed = token.Value<bool?>("isProcessed");
                log.HchbPatientId = (token.Value<string>("HchbId"))?.Trim();
                log.HchbPatientId = (token.Value<string>("episodeId"))?.Trim();
                log.HchbPatientId = (token.Value<string>("reason"))?.Trim();
                log.ProcessedDate = token.Value<DateTime?>("processedDate");
                log.ReceivedDate = token.Value<DateTime?>("receivedDate");                
            }
            return log;
        }

        public override void WriteJson(JsonWriter writer, HL7MessageLog value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
