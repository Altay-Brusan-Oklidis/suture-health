using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SutureHealth.Patients;
using System;
using System.Collections.Generic;

namespace SutureHealth.Hchb.JsonConverters
{
    public class PatientMatchingRequestConverter : JsonConverter<PatientMatchingRequest>
    {
        public override PatientMatchingRequest ReadJson(JsonReader reader, Type objectType, PatientMatchingRequest existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            PatientMatchingRequest matchingRequest = new ();

            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);

                // get Patient information
                matchingRequest.FirstName = (token.Value<string>("firstName"))?.Trim();
                matchingRequest.MiddleName = (token.Value<string>("middleName"))?.Trim();
                matchingRequest.LastName = (token.Value<string>("lastName"))?.Trim();
                matchingRequest.Suffix = (token.Value<string>("suffix"))?.Trim();

                matchingRequest.Birthdate = token.Value<DateTime>("birthDate");
                matchingRequest.Gender = EnumMemberExtensions.ToEnum<Gender>(token.Value<string>("gender"));

                matchingRequest.AddressLine1 = (token.Value<string>("addressLine1"))?.Trim();
                matchingRequest.AddressLine2 = (token.Value<string>("addressLine2"))?.Trim();
                matchingRequest.City = (token.Value<string>("city"))?.Trim();
                matchingRequest.StateOrProvince = (token.Value<string>("state"))?.Trim();
                matchingRequest.PostalCode = (token.Value<string>("postalCode"))?.Trim();
                
                var identifiers = (JArray)token["identifiers"];
                if (identifiers != null)
                {
                    foreach (JToken identifier in identifiers)
                    {
                        matchingRequest.Ids.Add(new PatientIdentifier
                        {
                            Type = (identifier.Value<string>("type"))?.Trim(),
                            Value = (identifier.Value<string>("value"))?.Trim()
                        });
                    }
                }

                var phones = (JArray)token["phones"];
                if (phones != null)
                {
                    foreach (JToken phone in phones)
                    {
                        matchingRequest.Phones.Add(new PatientPhone
                        {
                            Type = ContactType.Phone,
                            Value = phone.ToString()
                        });
                    }
                }
            }
            return matchingRequest;

        }

        public override void WriteJson(JsonWriter writer, PatientMatchingRequest value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName("firstName");
            writer.WriteValue(value.FirstName);

            writer.WritePropertyName("middleName");
            writer.WriteValue(value.MiddleName);

            writer.WritePropertyName("lastName");
            writer.WriteValue(value.LastName);

            writer.WritePropertyName("suffix");
            writer.WriteValue(value.Suffix);

            writer.WritePropertyName("birthDate");
            writer.WriteValue(value.Birthdate.ToString("yyyy-MM-dd"));

            writer.WritePropertyName("gender");
            writer.WriteValue(value.Gender.ToString()?.ToUpper().Substring(0, 1));

            writer.WritePropertyName("addressLine1");
            writer.WriteValue(value.AddressLine1);

            writer.WritePropertyName("addressLine2");
            writer.WriteValue(value.AddressLine2);

            writer.WritePropertyName("city");
            writer.WriteValue(value.City);

            writer.WritePropertyName("state");
            writer.WriteValue(value.StateOrProvince);

            writer.WritePropertyName("postalCode");
            writer.WriteValue(value.PostalCode);

            writer.WritePropertyName("identifiers");
            writer.WriteStartArray();
            foreach (var identifier in value.Ids)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue(identifier.Type);
                writer.WritePropertyName("value");
                writer.WriteValue(identifier.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

    }
}
