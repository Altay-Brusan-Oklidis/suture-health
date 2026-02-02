namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Models
{
    public class RoomViewModel
    {
        public string Name { get; set; }
        public string OtherParticipantName { get; set; }
        public int? UserId { get; set; }
        public long VideoVisitId { get; set; }
        public long VideoVisitSessionId { get; set; }
        public string VisitPublicId { get; set; }
        public string AccessToken { get; set; }
    }
}
