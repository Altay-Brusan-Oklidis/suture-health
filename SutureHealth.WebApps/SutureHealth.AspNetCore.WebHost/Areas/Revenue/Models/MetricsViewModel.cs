namespace SutureHealth.AspNetCore.Areas.Revenue.Models
{
    public class MetricsViewModel
    {
        public decimal Last30DaysRevenue { get; set; }
        public decimal YearToDateRevenue { get; set; }
        public decimal AllTimeRevenue { get; set; }
        public decimal Last30DaysRVU { get; set; }
        public decimal YearToDateRVU { get; set; }
        public decimal AllTimeRVU { get; set; }

        public bool IsPaidUser { get; set; }
    }
}
