namespace SutureHealth.DataScraping
{
    //corresponds to visit history 
    public class ProcedureHistory : PatientRelationalBase
    { 
        public string? Issue { get; set; }
        public string? Reason { get; set; }
        public string? Provider { get; set; } // stands for attended physcian
        public string? Billing { get; set; }
        public string? Insurance { get; set; }
        public DateTime? Date { get; set; }

        public ProcedureHistory()
        {

        }
    }
}
