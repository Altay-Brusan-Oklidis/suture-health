namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Models
{
    public class ExpiredErrorModel
    {
        public string HostName { get; internal set; }
        public string HostSupportPhoneNumber { get; internal set; }
        public bool IsAuthenticated { get; internal set; }
    }
}