
namespace SutureHealth.DataScraping
{
    public class Allergy : PatientRelationalBase
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Reaction { get; set; }
        public string? Severity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Allergy()
        {

        }
    }
}
