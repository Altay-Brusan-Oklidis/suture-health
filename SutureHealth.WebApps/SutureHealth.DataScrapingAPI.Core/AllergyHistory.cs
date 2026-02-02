namespace SutureHealth.DataScraping
{
    //allergies
    public class AllergyHistory : PatientRelationalBase
    { 
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Reaction { get; set; }
        public string? Severity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }  

        public AllergyHistory()
        {

        }

    }
}
