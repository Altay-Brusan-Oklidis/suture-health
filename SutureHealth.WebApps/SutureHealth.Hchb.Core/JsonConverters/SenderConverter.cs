using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;


namespace SutureHealth.Hchb.JsonConverters
{
    public class SenderConverter : JsonConverter<Person>
    {
        public override Person ReadJson(JsonReader reader, Type objectType, Person existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Person sender = new Person();

            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);
                
                string senderNpi = token.Value<string>("npi");
                if (senderNpi != "")
                {
                    sender.Npi = long.Parse(token.Value<string>("npi"));
                    sender.FirstName = (token.Value<string>("firstName"))?.Trim();
                    sender.LastName = (token.Value<string>("lastName"))?.Trim();
                    sender.BranchCode = (token.Value<string>("branchCode"))?.Trim();
                }
                else
                {
                    sender.Npi = 1891111911;
                    sender.FirstName = "HCHB";
                    sender.LastName = "User";
                    sender.BranchCode = (token.Value<string>("branchCode"))?.Trim();
                }
            }
            return sender;
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
