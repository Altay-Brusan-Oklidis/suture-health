using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using SutureHealth.Application;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [IgnoreLoginHandler]
    [MemberRegistrationHandler]
    public class RegistrationConfirmationModel : BasePageModel
    {
        protected ILogger<LoginModel> Logger { get; }
        protected SutureUserManager UserManager { get; }
        protected SutureSignInManager SignInManager { get; }

        public RegistrationConfirmationModel
        (
            SutureUserManager userManager,
            SutureSignInManager signInManager,
            ILogger<LoginModel> logger
        )
        {
            Logger = logger;
            UserManager = userManager;
            SignInManager = signInManager;
        }

        [FromQuery(Name = "auth")]
        public string AuthenticationToken { get; set; }
        [FromQuery(Name = "token")]
        public string ConfirmationToken { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }
        [FromQuery(Name = "returnUrl")]
        public string ReturnUrl { get; set; }

        public SelectList AssistantTypes { get; set; } = new SelectList(from a in MemberTypes.Assistants
                                                                        select new SelectListItem
                                                                        {
                                                                            Text = Enum.GetName<MemberType>((MemberType)a),
                                                                            Value = a.ToString()
                                                                        }, "Value", "Text");
        public SelectList UserSuffixes { get; set; } = new SelectList(SutureHealth.AspNetCore.Mvc.Rendering.Lists.Suffixes, "Text", "Value");

        public class InputModel
        {
            [Display(Name = "First Name")]
            [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
            [RegularExpression(RegexValidation.NameValidation, ErrorMessage = ValidationMessages.InvalidFieldMessageHelp)]
            public string FirstName { get; set; }
            [Display(Name = "Last Name")]
            [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
            [RegularExpression(RegexValidation.NameValidation, ErrorMessage = ValidationMessages.InvalidFieldMessageHelp)]
            public string LastName { get; set; }
            //[Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
            //[RegularExpression(RegexValidation.UserNameValidate, ErrorMessage = ValidationMessages.UserNameMessageHelp)]
            //[PageRemote(AdditionalFields = "PublicId", ErrorMessage = "USERNAME IS TAKEN.", HttpMethod = "Post", PageHandler = "ValidateUserNameHandler")]
            public string UserName { get; set; }
            //[Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
            //[RegularExpression(RegexValidation.EmailValidate, ErrorMessage = ValidationMessages.InvalidFieldMessage)]
            public string Email { get; set; }
            [RegularExpression(RegexValidation.NameValidation, ErrorMessage = ValidationMessages.InvalidFieldMessageHelp)]
            [Display(Name = "Professional Suffix")]
            public string Credential { get; set; }
            public int AssistantTypeId { get; set; } = -1;
            [RegularExpression(RegexValidation.PasswordValidate, ErrorMessage = ValidationMessages.PasswordInvalidFieldMessage)]
            [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
            public string Password { get; set; }
            [Display(Name = "Confirm Password")]
            [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
            [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
            public string ConfirmPassword { get; set; }
            [Display(Name = "Suffix")]
            public string UserSuffix { get; set; }

            public string ConfirmationToken { get; set; }
            public Guid PublicIdentifier { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            MemberIdentity codedMember = null;
            var publicIdentifier = Guid.Empty;

            if (!string.IsNullOrWhiteSpace(AuthenticationToken))
            {
                try
                {
                    publicIdentifier = Guid.Parse(Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(AuthenticationToken)));
                    codedMember = await UserManager.FindByPublicIdAsync(publicIdentifier);
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
                return RedirectToPage(new { auth = AuthenticationToken, token = ConfirmationToken, returnurl = ReturnUrl });
            }

            if ((CurrentUser != null && CurrentUser.MustRegisterAccount) || (codedMember != null && !string.IsNullOrWhiteSpace(ConfirmationToken)))
            {
                var memberIdentity = CurrentUser ?? codedMember;

                if (memberIdentity != null)
                {
                    Input = new InputModel
                    {
                        FirstName = memberIdentity.FirstName,
                        LastName = memberIdentity.LastName,
                        UserName = memberIdentity.UserName,
                        UserSuffix = memberIdentity.Suffix,
                        Credential = memberIdentity.ProfessionalSuffix,
                        AssistantTypeId = memberIdentity.MemberTypeId,
                        Email = memberIdentity.Email,
                        ConfirmationToken = !string.IsNullOrWhiteSpace(ConfirmationToken) ? Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(ConfirmationToken)) : await UserManager.GeneratePasswordResetTokenAsync(memberIdentity),
                        PublicIdentifier = publicIdentifier
                    };

                    return Page();
                }
            }

            return RedirectToPage("/Account/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var member = CurrentUser ?? await UserManager.FindByPublicIdAsync(Input.PublicIdentifier);
            if (ModelState.IsValid && member != null)
            {
                var generateSigningName = string.IsNullOrWhiteSpace(member.SigningName)
                    || Input.FirstName != member.FirstName
                    || Input.LastName != member.LastName
                    || (Input.UserSuffix ?? string.Empty) != (member.Suffix ?? string.Empty)
                    || (Input.Credential ?? string.Empty) != (member.ProfessionalSuffix ?? string.Empty);

                member.FirstName = Input.FirstName;
                member.LastName = Input.LastName;
                member.EmailConfirmed = true;
                member.MemberTypeId = Input.AssistantTypeId;
                member.MustRegisterAccount = false;
                member.ProfessionalSuffix = Input.Credential;
                member.SigningName = generateSigningName ? ($"{Input.FirstName} {Input.LastName}" +
                    (!string.IsNullOrWhiteSpace(Input.UserSuffix) ? $" {Input.UserSuffix}" : string.Empty) +
                    (!string.IsNullOrWhiteSpace(Input.Credential) ? $", {Input.Credential}" : string.Empty)) : member.SigningName;
                member.Suffix = Input.UserSuffix;

                var passwordResult = await UserManager.ResetPasswordAsync(member, Input.ConfirmationToken, Input.Password);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return Page();
                }

                await UserManager.LogAuditEventAsync(member, AuditEvents.Registration, $"{member} has completed registration.", true);
                if (Input.PublicIdentifier != Guid.Empty)
                {
                    await UserManager.VerifyPublicIdentityAsync(member, Input.PublicIdentifier, true);
                }

                return string.IsNullOrWhiteSpace(ReturnUrl) ? RedirectToPage("/Account/Login", new { externalResultId = LoginModel.RESULT_REGISTRATION_COMPLETED }) : Redirect(ReturnUrl);
            }

            return Page();
        }
    }
}