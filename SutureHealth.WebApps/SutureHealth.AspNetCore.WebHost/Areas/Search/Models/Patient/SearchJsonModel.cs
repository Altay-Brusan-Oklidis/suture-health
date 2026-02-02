namespace SutureHealth.AspNetCore.Areas.Search.Models.Patient
{
    public class SearchJsonModel
    {
        public IEnumerable<Patient> Patients { get; set; }

        public class Patient
        {
            public int PatientId { get; set; }
            public string Summary { get; set; }
        }
    }
}
