namespace SutureHealth.DataScraping
{
    // mostly drugs
    public class PrescriptionHistory : PatientRelationalBase
    { 
        public string? DrugName { get; set; }
        public string? Details { get; set; }
        public string? Quantity { get; set; }
        public string? Refills { get; set; }
        public DateTime? FillDate { get; set; }

        public PrescriptionHistory()
        {

        }
    }
}
