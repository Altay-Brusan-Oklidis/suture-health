namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class ResetPasswordModel
    {
        public string UserName { get; set; }
        public string ResetPasswordActionUrl { get; set; }
        public bool IsRegistrationRequired { get; set; }
        public bool CanResetPassword => !IsRegistrationRequired;
    }
}
