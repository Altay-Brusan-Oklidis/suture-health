using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;


namespace SutureHealth.Hchb.JsonConverters
{
    public class SignerConverter : JsonConverter<Person>
    {
        public override Person ReadJson(JsonReader reader, Type objectType, Person existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Person signer = new Person();
            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);

                long npi=0;
                long.TryParse(token.Value<string>("npi"), out npi);
                signer.Npi = npi;
                signer.FirstName = (token.Value<string>("firstName"))?.Trim();
                signer.LastName = (token.Value<string>("lastName"))?.Trim();
                signer.BranchCode = (token.Value<string>("branchCode"))?.Trim();

            }
            return signer;
        }

        public override void WriteJson(JsonWriter writer, Person value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName("npi");
            writer.WriteValue(value.Npi);

            writer.WritePropertyName("firstName");
            writer.WriteValue(value.FirstName);

            writer.WritePropertyName("lastName");
            writer.WriteValue(value.LastName);

            writer.WritePropertyName("branchCode");
            writer.WriteValue(value.BranchCode);

            writer.WriteEndObject();
        }

    }
}
