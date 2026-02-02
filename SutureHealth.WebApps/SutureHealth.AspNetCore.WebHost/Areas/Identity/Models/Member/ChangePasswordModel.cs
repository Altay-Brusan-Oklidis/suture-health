namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class ChangePasswordModel : ChangePasswordFields
    {
        public bool HasPassword => !string.IsNullOrWhiteSpace(Password);
    }
}
