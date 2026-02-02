namespace SutureHealth.DataScraping
{
    //medical problems - diagnoses
    public class ConditionHistory : PatientRelationalBase
    { 
        public string? Diagnosis { get; set; }
        public string? DiagnosisCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ConditionHistory()
        {

        }

        public ConditionHistory(Guid patientId, string diagnosis)
        {
            PatientId = patientId;
            Diagnosis = diagnosis;
        }
    }
}
