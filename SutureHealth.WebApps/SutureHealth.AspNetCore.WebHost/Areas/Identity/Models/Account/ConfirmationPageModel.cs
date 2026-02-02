using SutureHealth.AspNetCore.Models;
using SutureHealth.AspNetCore.Identity;

namespace SutureHealth.AspNetCore.Areas.Identity.Models.Account
{
    public class ConfirmationPageModel : BasePageModel
    {
        protected SutureUserManager UserManager { get; }

        public ConfirmationPageModel(SutureUserManager userManager)
        {
            UserManager = userManager;
        }
    }
}
