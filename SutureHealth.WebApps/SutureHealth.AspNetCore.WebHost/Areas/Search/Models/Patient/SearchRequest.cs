namespace SutureHealth.AspNetCore.Areas.Search.Models.Patient
{
    public class SearchRequest
    {
        public int? OrganizationId { get; set; }
        public int? Count { get; set; }
        public string Search { get; set; }
    }
}
