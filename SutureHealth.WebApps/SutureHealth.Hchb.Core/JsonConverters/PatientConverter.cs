using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using SutureHealth.Patients;
using System.Linq;

namespace SutureHealth.Hchb.JsonConverters
{
    public class PatientConverter : JsonConverter<Patient>
    {
        public override Patient ReadJson(
            JsonReader reader, 
            Type objectType, 
            Patient existingValue, 
            bool hasExistingValue, 
            JsonSerializer serializer
        )
        {
            Patient patient = new ();
            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);
                patient.FirstName = (token.Value<string>("firstName"))?.Trim();
                patient.MiddleName = (token.Value<string>("middleName"))?.Trim();
                patient.LastName = (token.Value<string>("lastName"))?.Trim();
                patient.Suffix = (token.Value<string>("suffix"))?.Trim();
                patient.Birthdate = token.Value<DateTime>("birthDate");
                patient.Gender = EnumMemberExtensions.ToEnum<Gender>(token.Value<string>("gender"));
                patient.Addresses = new List<PatientAddress>()
                {
                    new PatientAddress()
                    {
                        Line1 = token.Value<string>("addressLine1"),
                        Line2 = token.Value<string>("addressLine2"),
                        City = token.Value<string>("city"),
                        StateOrProvince = token.Value<string>("state"),
                        PostalCode = token.Value<string>("postalCode")
                    }
                };

                patient.Identifiers = new List<PatientIdentifier>();
                var identifiers = (JArray)token["identifiers"];
                if (identifiers != null)
                {
                    foreach (var identifier in identifiers)
                    {
                        patient.Identifiers.Add(
                            new PatientIdentifier()
                            {
                                Type = (string)identifier["type"],
                                Value = (string)identifier["value"]
                            });
                    }
                }

                patient.Phones = new List<PatientPhone>();
                var phonenumbers = (JArray)token["phonenumbers"];
                if (phonenumbers != null) 
                {
                    foreach (var phone in phonenumbers)
                    {
                        patient.Phones.Add(new PatientPhone()
                        {
                            Type = ContactType.Phone,
                            Value = (string)phone
                        });
                    }
                }
            }

            return patient;
        }

        public override void WriteJson(JsonWriter writer, Patient value, JsonSerializer serializer)
        {
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
            writer.WriteValue(value.Birthdate);
            writer.WritePropertyName("gender");
            writer.WriteValue(value.Gender.ToString()?.ToUpper().Substring(0, 1));

            PatientAddress address = value.Addresses.FirstOrDefault();
            if (address != null) 
            {
                writer.WritePropertyName("addressLine1");
                writer.WriteValue(address.Line1);
                writer.WritePropertyName("addressLine2");
                writer.WriteValue(address.Line2);
                writer.WritePropertyName("city");
                writer.WriteValue(address.City);
                writer.WritePropertyName("state");
                writer.WriteValue(address.StateOrProvince);
                writer.WritePropertyName("postalCode");
                writer.WriteValue(address.PostalCode);
            }
            

            writer.WritePropertyName("identifiers");
            writer.WriteStartArray();
            foreach (var identifier in value.Identifiers)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("type");
                writer.WriteValue(identifier.Type);
                writer.WritePropertyName("value");
                writer.WriteValue(identifier.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("phonenumbers");
            writer.WriteStartArray();
            foreach (var phone in value.Phones)
            {
                writer.WriteValue(phone.Value);
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

    }
}
