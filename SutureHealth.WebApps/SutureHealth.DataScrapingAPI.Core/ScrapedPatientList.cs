
namespace SutureHealth.DataScraping
{
    public class ScrapedPatientList
    {
        public List<ScrapedPatientHistory> ScrapedPatients { get; set; }

        public ScrapedPatientList()
        {
            ScrapedPatients = new List<ScrapedPatientHistory>();
        }

        
    }
}
