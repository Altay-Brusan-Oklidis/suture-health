
namespace SutureHealth.DataScraping
{
    public class Prescription : PatientRelationalBase
    {
        public string? DrugName { get; set; }
        public string? Details { get; set; }
        public string? Quantity { get; set; }
        public string? Refills { get; set; }
        public DateTime? FillDate { get; set; }

        public Prescription()
        {

        }
    }
}
