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
    public class AdtConverter : JsonConverter<Adt>
    {
        public override Adt ReadJson(JsonReader reader, Type objectType, Adt existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Adt adt = new Adt();
            if (reader.TokenType != JsonToken.Null)
            {
                JToken token = JToken.Load(reader);

                var patientToken = token["patient"];
                if (patientToken != null)
                {
                    adt.Patient = patientToken.ToObject<Patient>(serializer);
                }

                
                var hchbPatientToken = token["patient_hchb"];
                if (hchbPatientToken != null)
                {
                    adt.HchbPatient = hchbPatientToken.ToObject<HchbPatientWeb>(serializer);
                }


                adt.BranchCode = (token.Value<string>("branchCode"))?.Trim();
                adt.MessageControlId = (token.Value<string>("messageControlId"))?.Trim();
                adt.Type = EnumMemberExtensions.ToEnum<ADT>(token.Value<string>("type"));
                adt.RawFileName = (token.Value<string>("rawfilename"))?.Trim();
                adt.JsonFileName = (token.Value<string>("jsonfilename"))?.Trim();                
            }
            return adt;
        }

        public override void WriteJson(JsonWriter writer, Adt value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            // Serialize the 'Patient' object using its converter
            writer.WritePropertyName("patient");
            serializer.Serialize(writer, value.Patient);

            // Serialize the 'HchbPatientWeb' object using its converter
            writer.WritePropertyName("patient_hchb");
            serializer.Serialize(writer, value.HchbPatient);

            writer.WritePropertyName("branchCode");
            writer.WriteValue(value.BranchCode);
            writer.WritePropertyName("messageControlId");
            writer.WriteValue(value.MessageControlId);
            writer.WritePropertyName("type");
            writer.WriteValue(value.Type.ToString());
            writer.WritePropertyName("rawfilename");
            writer.WriteValue(value.RawFileName);
            writer.WritePropertyName("jsonfilename");
            writer.WriteValue(value.JsonFileName);

            writer.WriteEndObject();
        }

    }
}
