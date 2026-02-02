namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Models
{
    public class ViewVisit
    {
        public ViewVisit()
        {
            Visits = new List<Visit>();
        }
        public List<Visit> Visits { get; set; }
        public int RowCount { get; set; }
    }

    public class Visit
    {
        public long VideoVisitId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PublicId { get; set; }
        public bool Active { get; set; }
        public int HostUserId { get; set; }
        public string HostName { get; set; }
        public string HostSupportPhoneNumber { get; set; }
        public string HostIpAddress { get; set; }
        public string ParticipantPhoneNumber { get; set; }
        public int? TotalMinutes { get; internal set; }
        public int? BillableMinutes { get; internal set; }

        public string PatientName { get; set; }
    }
}