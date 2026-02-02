using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;


namespace SutureHealth.Hchb.JsonConverters
{
    public class TransactionConverter : JsonConverter<HchbTransaction>
    {
        public override HchbTransaction ReadJson(JsonReader reader, Type objectType, HchbTransaction existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            HchbTransaction transaction = new HchbTransaction();
            if (reader.TokenType != JsonToken.Null) 
            {
                JToken token = JToken.Load(reader);
                transaction.OrderDate = DateTime.ParseExact(token.Value<string>("orderDate"), "yyyyMMddHHmmss", null);
                transaction.OrderNumber = (token.Value<string>("orderNumber"))?.Trim();
                transaction.FileName = (token.Value<string>("filename"))?.Trim();
                transaction.AdmitDate = DateTime.ParseExact(token.Value<string>("admitDate"), "yyyyMMddHHmmss", null);
                transaction.ObservationId = (token.Value<string>("observationId"))?.Trim();
                transaction.ObservationText = (token.Value<string>("observationText"))?.Trim();
                transaction.AdmissionType = (token.Value<string>("admissionType"))?.Trim();
                transaction.PatientType = (token.Value<string>("patientType"))?.Trim();
                transaction.HchbPatientId = (token.Value<string>("hchbId"))?.Trim();
                transaction.EpisodeId = (token.Value<string>("episodeId"))?.Trim();
                transaction.SendDate = DateTime.ParseExact(token.Value<string>("sendDate"), "yyyyMMddHHmmss", null);
            }

            return transaction;
        }

        public override void WriteJson(JsonWriter writer, HchbTransaction value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName("orderDate");
            writer.WriteValue(value.OrderDate?.ToString("yyyyMMddHHmmss"));

            writer.WritePropertyName("orderNumber");
            writer.WriteValue(value.OrderNumber);

            writer.WritePropertyName("filename");
            writer.WriteValue(value.FileName);

            writer.WritePropertyName("admitDate");
            writer.WriteValue(value.AdmitDate?.ToString("yyyyMMddHHmmss"));

            writer.WritePropertyName("observationId");
            writer.WriteValue(value.ObservationId);

            writer.WritePropertyName("observationText");
            writer.WriteValue(value.ObservationText);

            writer.WritePropertyName("admissionType");
            writer.WriteValue(value.AdmissionType);

            writer.WritePropertyName("patientType");
            writer.WriteValue(value.PatientType);

            writer.WritePropertyName("hchbId");
            writer.WriteValue(value.HchbPatientId);

            writer.WritePropertyName("episodeId");
            writer.WriteValue(value.EpisodeId);
                        
            writer.WritePropertyName("sendDate");
            writer.WriteValue(value.SendDate?.ToString("yyyyMMddHHmmss"));

            writer.WriteEndObject();
        }

    }
}
