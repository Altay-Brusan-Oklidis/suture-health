namespace SutureHealth.DataScraping
{
    public class ImmunizationHistory : PatientRelationalBase
    { 
        public string? Name { get; set; }
        public string? Code { get; set; }
        public DateTime? AdministrationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public ImmunizationHistory()
        {

        }


    }
}
