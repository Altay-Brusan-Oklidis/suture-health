using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing.Mock
{

    [JsonObject]
    public class HchbPatientModelMock
    {
        public string patientId { get; set; } = string.Empty; // PatientId of suture
        public string externalId { get; set; } = string.Empty;
        public string hchbId { get; set; } = string.Empty; // PatientId of hchb
        public string admissionId { get; set; } = string.Empty;
        public string episodeId { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string physicianNpi { get; set; } = string.Empty;
        public string physicianFirstName { get; set; } = string.Empty;
        public string physicianLastName { get; set; } = string.Empty;
    }
}
