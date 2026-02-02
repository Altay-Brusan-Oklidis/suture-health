namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class SendRegistrationEmailModel
    {
        public string UserName { get; set; }
        public bool IsRegistered { get; set; }
        public string SendRegistrationEmailActionUrl { get; set; }
    }
}
