namespace SutureHealth.AspNetCore.Areas.Admin.Models.Api
{
    public class RequestGridListItem
    {
        public long RequestId { get; set; }
        public string DateSubmitted { get; set; }
        public string SendingOrganization { get; set; }
        public string SigningOrganization { get; set; }
        public string SignerName { get; set; }
        public string PatientName { get; set; }
        public string Status { get; set; }
        public string RequestJson { get; set; }
        public string PatientMatchJson { get; set; }
    }
}
