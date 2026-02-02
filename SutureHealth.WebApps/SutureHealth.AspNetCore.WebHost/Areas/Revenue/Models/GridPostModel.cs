namespace SutureHealth.AspNetCore.Areas.Revenue.Models
{
    public class GridPostModel
    {
        public int UserID { get; set; }
        public string UserRole { get; set; }
        public int SignerFacilityId { get; set; }
        public int Signer { get; set; }
        public string ServiceStartDate { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SortExpr { get; set; }
        public string SortDir { get; set; }
        public string EffectiveStartDate { get; set; }
        public string EffectiveEndDate { get; set; }
        public string ServiceEndDate { get; set; }
        public string Statement { get; set; }

        public int PatientId { get; set; }
    }
}
