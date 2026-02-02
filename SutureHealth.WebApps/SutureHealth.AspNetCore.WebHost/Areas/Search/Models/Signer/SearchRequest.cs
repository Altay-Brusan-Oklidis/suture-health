namespace SutureHealth.AspNetCore.Areas.Search.Models.Signer
{
    public class SearchRequest
    {
        public int? Count { get; set; }
        public string Search { get; set; }
        public string OrganizationStateOrProvinceFilter { get; set; }
    }
}
