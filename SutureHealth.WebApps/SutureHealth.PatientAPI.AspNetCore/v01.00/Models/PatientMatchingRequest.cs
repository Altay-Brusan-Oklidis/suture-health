using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SutureHealth.Patients.v0100.Models
{
   public class PatientMatchingRequest 
   {
      public List<Identifier> Ids { get; set; } = new List<Identifier>();

      public DateTime Birthdate { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string PostalCode { get; set; }

      [JsonConverter(typeof(JsonStringEnumConverter))]
      public Gender Gender { get; set; }
   }
}

