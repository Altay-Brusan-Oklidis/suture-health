
namespace SutureHealth.DataScraping
{
    public class Observation : PatientRelationalBase
    {
        public string? Labs { get; set; }
        public string? Vitals { get; set; }

        public Observation()
        {

        }
    }
}
