namespace SutureHealth.AspNetCore.Areas.Network.Models.Invitation
{
    public class FaxInvitation : InvitationContent
    {
        public string DownloadUrl { get; set; }
        public bool IsDownloading { get; set; }
    }
}
