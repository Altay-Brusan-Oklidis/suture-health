using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.JsonConverters
{
    public class OruConverter : JsonConverter<Oru>
    {
        public override Oru ReadJson(JsonReader reader, Type objectType, Oru existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Oru oru = new Oru();
            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);
                oru.FirstName = (token.Value<string>("firstName"))?.Trim();
                oru.LastName = (token.Value<string>("lastName"))?.Trim();
                oru.Gender = (token.Value<string>("gender"))?.Trim();
                oru.BirthDate = (token.Value<string>("birthDate"))?.Trim();
                oru.PatientId = (token.Value<string>("patientId"))?.Trim();
                oru.EpisodeId = (token.Value<string>("episodeId"))?.Trim();
                oru.OrderNumber = (token.Value<string>("orderNumber"))?.Trim();
                oru.ObservationId = (token.Value<string>("observationId"))?.Trim();
                oru.ObservationText = (token.Value<string>("observationText"))?.Trim();
                oru.RequestedDateTime = (token.Value<string>("requestedDateTime"))?.Trim();
                oru.RejectReason = (token.Value<string>("rejectReason"))?.Trim();
                oru.ResultDate = (token.Value<string>("resultDate"))?.Trim();
                oru.ResultStatus = (token.Value<string>("resultStatus"))?.Trim();

            }
            return oru;
        }

        public override void WriteJson(JsonWriter writer, Oru value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("firstName");
            writer.WriteValue(value.FirstName);
            writer.WritePropertyName("lastName");
            writer.WriteValue(value.LastName);
            writer.WritePropertyName("gender");
            writer.WriteValue(value.Gender.ToString()?.ToUpper().Substring(0, 1));
            writer.WritePropertyName("birthDate");
            writer.WriteValue(value.BirthDate.ToString());
            writer.WritePropertyName("patientId");
            writer.WriteValue(value.PatientId);
            writer.WritePropertyName("episodeId");
            writer.WriteValue(value.EpisodeId);
            writer.WritePropertyName("orderNumber");
            writer.WriteValue(value.OrderNumber);
            writer.WritePropertyName("observationId");
            writer.WriteValue(value.ObservationId);
            writer.WritePropertyName("observationText");
            writer.WriteValue(value.ObservationText);
            writer.WritePropertyName("requestedDateTime");
            writer.WriteValue(value.RequestedDateTime);
            writer.WritePropertyName("rejectReason");
            writer.WriteValue(value.RejectReason);
            writer.WritePropertyName("resultDate");
            writer.WriteValue(value.ResultDate);
            writer.WritePropertyName("resultStatus");
            writer.WriteValue(value.ResultStatus);

            writer.WriteEndObject();
        }
    }
}
