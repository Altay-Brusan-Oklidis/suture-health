using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Notifications.Services;
using SutureHealth.Reporting.Services;

namespace SutureHealth.AspNetCore.Areas.Identity.Pages.Account
{
    public class ConfirmEmailViewModel
    {
        public MemberIdentity SutureUser { get; set; }
        public string TriggerSelector { get; set; }
    }

    public class ConfirmEmailModel : BasePageModel
    {
        protected ILogger<ConfirmEmailModel> Logger { get; }
        protected IIdentityService IdentityServices { get; }
        protected INotificationService DeliveryServices { get; }
        protected IGenerationService GenerationServices { get; }
        protected SutureUserManager UserManager { get; }

        public ConfirmEmailModel
        (
            IIdentityService identityService,
            INotificationService deliveryService,
            IGenerationService generationService,
            ILogger<ConfirmEmailModel> logger,
            SutureUserManager userManager
        )
        {
            DeliveryServices = deliveryService;
            GenerationServices = generationService;
            IdentityServices = identityService;
            Logger = logger;
            UserManager = userManager;
        }

        public IEnumerable<IdentityError> ConfirmationErrors { get; set; }

        public async Task<IActionResult> OnGetAsync(string userName, string token)
        {
            if (userName != null && token != null)
            {
                var user = await UserManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return NotFound($"Unable to load user with username '{userName}'.");
                }

                if (user.Id != CurrentUser.Id)
                {
                    //FailureMessage = "You must login using the account associated with the email you wish to confirm.";
                    return RedirectToPage("/Account/Login");
                }

                var result = await UserManager.ConfirmEmailAsync(user, token);
                if (!result.Succeeded)
                {
                    //FailureMessage = result.Errors.ToString();
                    return RedirectToPage("/Account/Login");
                }
                else
                    return RedirectToRoute("MemberAccountIndex");
            }

            return Redirect(Url.DefaultLandingPage(CurrentUser));
        }

        public async Task<IActionResult> OnPostSendConfirmationTokenAsync()
        {
            await UserManager.SendEmailConfirmationAsync(CurrentUser);
            return new JsonResult(IdentityResult.Success);
        }
    }
}
