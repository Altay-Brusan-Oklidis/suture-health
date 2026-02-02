using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.Application;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.AspNetCore.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using SutureHealth.AspNetCore.Services;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using Microsoft.Extensions.Options;

namespace SutureHealth.AspNetCore.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [IgnoreLoginHandler]
    [IgnoreAntiforgeryToken(Order = 1001)]
    [MemberRegistrationHandler]
    public class LoginModel : BasePageModel
    {
        public const string VIEWDATA_ANTIFORGERY_FIELD_NAME = "AntiforgeryFieldName";

        public static readonly Guid RESULT_PASSWORD_CHANGED = new Guid("3aa12c6a-8242-48c2-9460-2727845a01d9");
        public static readonly Guid RESULT_REGISTRATION_COMPLETED = new Guid("12c0c631-99b1-4447-a2d8-358a695af846");
        public static readonly Guid RESULT_EXPIRED_TOKEN = new Guid("8bf49979-647c-4afb-9761-9c52d5bc891f");

        private static readonly IReadOnlyDictionary<Guid, ExternalResult> ExternalResults = new Dictionary<Guid, ExternalResult>()
        {
            { RESULT_PASSWORD_CHANGED, new ExternalResult{ Type = ExternalResult.ExternalResultType.Success, Message = "Your password has been changed." } },
            { RESULT_REGISTRATION_COMPLETED, new ExternalResult{ Type = ExternalResult.ExternalResultType.Success, Message = "Your registration is complete. Please use your new password to login." } },
            { RESULT_EXPIRED_TOKEN, new ExpiredTokenExternalResult{ Type = ExternalResult.ExternalResultType.Failure } }
        };

        private readonly ILogger<LoginModel> _logger;
        private readonly IOptionsMonitor<Saml2Configuration> _saml2OptionsMonitor;
        private readonly SutureSignInManager _signInManager;

        public LoginModel
        (
            SutureSignInManager signInManager,
            ILogger<LoginModel> logger,
            IOptionsMonitor<Saml2Configuration> saml2OptionsMonitor
        )
        {
            _logger = logger;
            _saml2OptionsMonitor = saml2OptionsMonitor;
            _signInManager = signInManager;
        }

        [TempData]
        public string TempErrorMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }
        public ExternalResult StatusMessage { get; set; }
        public bool HasStatusMessage => StatusMessage != null;
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "REQUIRED")]
            [DataType(DataType.Text)]
            [MinLength(3, ErrorMessage = "Your user name must be at least 3 characters long.")]
            [StringLength(50, ErrorMessage = "You user name is less than 50 characters long.")]
            [RegularExpression(RegexValidation.UserNameValidate, ErrorMessage = "Your user name may only contains letters, numbers, or any of the following symbols: . - _ @")]
            public string Username { get; set; }
            [Required(ErrorMessage = "REQUIRED")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            //[Display(Name = "Remember me?")]
            //public bool RememberMe { get; set; } = true;
            public string AuthenticationToken { get; set; }
        }

        public class ExternalResult
        {
            public string Message { get; set; }
            public ExternalResultType Type { get; set; }

            public enum ExternalResultType
            {
                Success,
                Failure
            }
        }

        public class ExpiredTokenExternalResult : ExternalResult { }

        public async Task<IActionResult> OnGetAsync
        (
            [FromQuery]
            string userName = null,
            [FromQuery]
            string returnUrl = null,
            [FromQuery]
            Guid? externalResultId = null,
            [FromQuery(Name = "auth")]
            string authenticationToken = null
        )
        {
            // Clear the existing external cookie to ensure a clean login process
            if (CurrentUser != null)
            {
                await User.LogOutAsync(_saml2OptionsMonitor, _signInManager);
                return RedirectToPage(new { userName, returnUrl, externalResultId });
            }

            ReturnUrl = returnUrl;
            Input = new InputModel
            {
                Username = userName,
                AuthenticationToken = authenticationToken
            };

            StatusMessage = ExternalResults.GetValueOrDefault(externalResultId ?? Guid.Empty);

            if (TempErrorMessage != null)
                ModelState.AddModelError(string.Empty, TempErrorMessage);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync
        (
            [FromQuery]
            string returnUrl = null)
        {

            // Clear the existing external cookie to ensure a clean login process
            if (CurrentUser != null)
            {
                await User.LogOutAsync(_saml2OptionsMonitor, _signInManager);
                CurrentUser = null;
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, false, lockoutOnFailure: true);
                return result.ToActionResult(ModelState, _logger, Url, Page(), returnUrl);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostForgotPasswordHandlerAsync
        (
            [FromServices] SutureUserManager userManager
        )
        {
            var member = Input switch
            {
                _ when !string.IsNullOrWhiteSpace(Input.Username) => await userManager.FindByNameAsync(Input.Username),
                _ when !string.IsNullOrWhiteSpace(Input.AuthenticationToken) => await FindByEncodedPublicIdentityAsync(userManager, Input.AuthenticationToken),
                _ => null
            };

            if (member != null && member.IsActive)
            {
                if (member.MustRegisterAccount)
                {
                    await userManager.SendRegistrationConfirmationAsync(member);
                }
                else
                {
                    await userManager.ResetPasswordAsync(member);
                }
            }

            return new JsonResult(new
            {
                Result = true,
                Message = Input switch
                {
                    _ when !string.IsNullOrWhiteSpace(Input.AuthenticationToken) => "Please check your email. If you do not see it, check spam or contact your administrator.",
                    _ => "Check your email for instructions on how to reset your password. If you have not received an email, contact your organization's administrator for help."
                }
            });
        }

        public static async Task<MemberIdentity> FindByEncodedPublicIdentityAsync(SutureUserManager userManager, string authenticationToken)
        {
            try
            {
                return await userManager.FindByPublicIdAsync(Guid.Parse(Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(authenticationToken))), true);
            }
            catch
            {
                return null;
            }
        }
    }
}
