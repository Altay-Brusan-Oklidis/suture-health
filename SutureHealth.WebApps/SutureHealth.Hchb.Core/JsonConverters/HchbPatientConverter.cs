using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.JsonConverters
{
    public class HchbPatientConverter : JsonConverter<HchbPatientWeb>
    {
        public override HchbPatientWeb ReadJson(JsonReader reader, Type objectType, HchbPatientWeb existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            HchbPatientWeb hchbPatientWeb = new HchbPatientWeb();
            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);
                var hchbId = token.Value<string>("HchbId");

                if (hchbId == null || hchbId == string.Empty)
                {
                    hchbPatientWeb.HchbPatientId = token.Value<string>("patientId");
                }
                else
                {
                    hchbPatientWeb.HchbPatientId = hchbId;
                }
                int patientId;
                if (int.TryParse(token.Value<string>("patientId"),out patientId))
                    hchbPatientWeb.PatientId = patientId;
                
                hchbPatientWeb.EpisodeId = (token.Value<string>("episodeId"))?.Trim();
                hchbPatientWeb.IcdCode = (token.Value<string>("icd10code"))?.Trim();
                hchbPatientWeb.Status = (token.Value<string>("status"))?.Trim();


            }
            return hchbPatientWeb;
        }

        public override void WriteJson(JsonWriter writer, HchbPatientWeb value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("HchbId");
            writer.WriteValue(value.HchbPatientId);
            writer.WritePropertyName("patientId");
            writer.WriteValue(value.PatientId);
            writer.WritePropertyName("episodeId");
            writer.WriteValue(value.EpisodeId);
            writer.WritePropertyName("icd10code");
            writer.WriteValue(value.IcdCode);
            writer.WritePropertyName("status");
            writer.WriteValue(value.Status);

            writer.WriteEndObject();
        }

    }
}
