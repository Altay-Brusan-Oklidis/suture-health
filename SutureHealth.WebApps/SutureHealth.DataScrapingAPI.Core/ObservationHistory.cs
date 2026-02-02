namespace SutureHealth.DataScraping
{
    //vitals, labs
    public class ObservationHistory : PatientRelationalBase
    {
        public string? Labs { get; set; }
        public string? Vitals { get; set; }

        public ObservationHistory()
        {

        }

        public ObservationHistory(Guid patientId, string labs, string vitals)
        {
            PatientId = patientId;
            Labs = labs;
            Vitals = vitals;
        }
    }
}
