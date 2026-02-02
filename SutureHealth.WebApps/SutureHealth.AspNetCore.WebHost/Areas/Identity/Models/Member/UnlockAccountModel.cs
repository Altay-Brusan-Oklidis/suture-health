namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class UnlockAccountModel
    {
        public const string ACCOUNT_UNLOCKED = "UnlockAccountModel:AccountUnlocked";

        public string UserName { get; set; }
        public string UnlockAccountActionUrl { get; set; }
    }
}
