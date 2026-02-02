
namespace SutureHealth.DataScraping
{
    public class ScrapPatientHtmlRequest
    {
        public string? OrganizationName { get; set; }
        public string? PageName { get; set; }
        public string? PageUrl { get; set; }
        public DateTime ScrapedAt { get; set; }
        public string? Html { get; set; }
    }
}