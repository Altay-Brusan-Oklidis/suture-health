using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Notifications.Services;
using SutureHealth.Application.Services;
using SutureHealth.Reporting.Services;
using SutureHealth.Application;
using Microsoft.AspNetCore.Identity;

namespace SutureHealth.AspNetCore.Areas.Identity.Pages.Account
{
    public class ConfirmPhoneViewModel
    { 
        public MemberIdentity SutureUser { get; set; }
        public string TriggerSelector { get; set; }
        public string VerificationToken { get; set; }
    }

    public class ConfirmPhoneModel : BasePageModel
    {
        protected ILogger<ConfirmPhoneModel> Logger { get; }
        protected IIdentityService IdentityServices { get; }
        protected INotificationService DeliveryServices { get; }
        protected IGenerationService GenerationServices { get; }
        protected SutureSignInManager SignInManager { get; }
        protected SutureUserManager UserManager { get; }

        public ConfirmPhoneModel
        (
            IIdentityService identityService,
            SutureSignInManager signInManager,
            INotificationService deliveryService,
            IGenerationService generationService,
            ILogger<ConfirmPhoneModel> logger,
            SutureUserManager userManager
        ) 
        {
            DeliveryServices = deliveryService;
            GenerationServices = generationService;
            IdentityServices = identityService;
            Logger = logger;
            SignInManager = signInManager;
            UserManager = userManager;
        }

        [BindProperty]
        protected ConfirmPhoneModel Input { get; set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostSendConfirmationTokenAsync()
        {
            await UserManager.SendMobileNumberConfirmationAsync(CurrentUser);
            return new JsonResult(IdentityResult.Success);
        }

        public async Task<IActionResult> OnPostVerifyConfirmationTokenAsync(string verificationToken)
            => new JsonResult(await UserManager.ConfirmMobileAsync(CurrentUser, verificationToken));
    }
}
