

namespace SutureHealth.DataScraping
{
    public class Medication : PatientRelationalBase
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Medication()
        {

        }
    }
}
