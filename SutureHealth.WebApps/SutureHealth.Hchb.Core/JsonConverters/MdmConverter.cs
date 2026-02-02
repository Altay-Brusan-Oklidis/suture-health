using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SutureHealth.Patients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.JsonConverters
{
    public class MdmConverter : JsonConverter<Mdm>
    {
        public override Mdm ReadJson(JsonReader reader, Type objectType, Mdm existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Mdm mdm = new Mdm();

            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);

                var patientToken = token["patient"];
                if (patientToken != null)
                {
                    mdm.Patient = patientToken.ToObject<PatientMatchingRequest>(serializer);
                }
                
                var transactionToken = token["transaction"];
                if (transactionToken != null)
                {
                    mdm.Transaction = transactionToken.ToObject<HchbTransaction>(serializer);
                }

                var signerToken = token["signer"];
                if (signerToken != null)
                {
                    mdm.Signer = signerToken.ToObject<Person>(serializer);
                }

                var senderToken = token["sender"];
                if (senderToken != null)
                {
                    mdm.Sender = senderToken.ToObject<Person>(serializer);
                }

                mdm.MessageControlId = (token.Value<string>("messageControlId"))?.Trim();
                mdm.Rawfilename = (token.Value<string>("rawfilename"))?.Trim();
                mdm.Jsonfilename = (token.Value<string>("jsonfilename"))?.Trim();
                mdm.Status = (token.Value<string>("status"))?.Trim();

            }
            return mdm;
        }

        public override void WriteJson(JsonWriter writer, Mdm value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("patient");
            serializer.Serialize(writer, value.Patient);

            writer.WritePropertyName("transaction");
            serializer.Serialize(writer, value.Transaction);


            writer.WritePropertyName("signer");
            serializer.Serialize(writer, value.Signer);

            writer.WritePropertyName("sender");
            serializer.Serialize(writer, value.Sender);


            writer.WritePropertyName("messageControlId");
            writer.WriteValue(value.MessageControlId);
            writer.WritePropertyName("rawfilename");
            writer.WriteValue(value.Rawfilename);
            writer.WritePropertyName("jsonfilename");
            writer.WriteValue(value.Jsonfilename);
            writer.WritePropertyName("status");
            writer.WriteValue(value.Status);

            writer.WriteEndObject();
        }
    }
}
