
namespace SutureHealth.DataScraping
{
    public class Condition : PatientRelationalBase
    {
        public string? Diagnosis { get; set; }
        public string? DiagnosisCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Condition()
        {

        }
    }
}
