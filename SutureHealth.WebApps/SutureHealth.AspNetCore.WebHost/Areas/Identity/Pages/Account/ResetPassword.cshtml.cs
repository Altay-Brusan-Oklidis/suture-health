using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using SutureHealth.Application;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [IgnoreLoginHandler]
    [ChangePasswordHandler]
    public class ResetPasswordViewModel : BasePageModel
    {
        private readonly SutureSignInManager SignInManager;
        private readonly SutureUserManager UserManager;

        public ResetPasswordViewModel
        (
            SutureSignInManager signInManager,
            SutureUserManager userManager
        )
        {
            SignInManager = signInManager;
            UserManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel : SutureHealth.AspNetCore.Areas.Identity.Models.ChangePasswordFields
        {
            [Required(ErrorMessage = "REQUIRED")]
            [DataType(DataType.Text)]
            [StringLength(50, ErrorMessage = "Your user name must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [RegularExpression(RegexValidation.UserNameValidate, ErrorMessage = "Your user name may only contains letters, numbers, or any of the following symbols: . - _ @")]
            public string UserName { get; set; }
        }

        [FromQuery(Name = "auth")]
        public string AuthenticationToken { get; set; }
        [FromQuery]
        public string Code { get; set; }
        [FromQuery]
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            MemberIdentity codedMember = null;

            if (!string.IsNullOrWhiteSpace(AuthenticationToken))
            {
                try
                {
                    codedMember = await UserManager.FindByPublicIdAsync(Guid.Parse(Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(AuthenticationToken))));
                }
                catch { }

                if (codedMember == null)
                {
                    return RedirectToPage("/Account/Login", new { externalResultId = LoginModel.RESULT_EXPIRED_TOKEN, auth = AuthenticationToken });
                }
            }
            if (CurrentUser != null && codedMember != null && CurrentUser.MemberId != codedMember.MemberId)
            {
                await SignInManager.SignOutAsync();
                return RedirectToPage(new { auth = AuthenticationToken, code = Code, returnurl = ReturnUrl });
            }
            if (CurrentUser == null && string.IsNullOrWhiteSpace(Code))
            {
                return RedirectToPage("/Account/Login");
            }

            Input = new InputModel()
            {
                UserName = CurrentUser?.UserName
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (CurrentUser != null)
                ModelState.Remove<ResetPasswordViewModel>(m => Input.UserName);
            else
                ModelState.Remove<ResetPasswordViewModel>(m => Input.OldPassword);

            IdentityResult result = new();
            if (ModelState.IsValid)
            {
                if (CurrentUser != null)
                {
                    result = await UserManager.ChangePasswordAsync(CurrentUser, Input.OldPassword, Input.Password);
                    if (result.Succeeded)
                    {
                        await SignInManager.RefreshSignInAsync(CurrentUser);
                    }
                }
                else
                {
                    var user = await UserManager.FindByNameAsync(Input.UserName);
                    if (user != null)
                    {
                        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
                        result = await UserManager.ResetPasswordAsync(user, code, Input.Password);
                    }
                }
            }

            if (result.Succeeded)
            {
                return !string.IsNullOrWhiteSpace(ReturnUrl) ? Redirect(ReturnUrl) : RedirectToPage("/Account/Login", new { externalResultId = LoginModel.RESULT_PASSWORD_CHANGED });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}