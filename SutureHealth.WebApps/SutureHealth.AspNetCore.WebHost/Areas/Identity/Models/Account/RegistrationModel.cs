using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SutureHealth.Application;
using SutureHealth.AspNetCore.Models;
using System.ComponentModel.DataAnnotations;

namespace SutureHealth.AspNetCore.Areas.Identity.Models.Account
{
    public class RegistrationModel : BaseViewModel
    {
        public string ActionType { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string PageTitle { get; set; }
        public string RedirectUrl { get; set; }
        public string RegistrationText { get; set; }
        public string Role { get; set; }

        public bool HideUserDetails { get; set; }

        public SelectList AssistantTypes { get; set; }
        public SelectList UserSuffixes { get; set; }

        public Guid? PublicId { get; set; }
        public int UserId { get; set; }
        [Display(Name = "First Name")]
        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        [RegularExpression(RegexValidation.NameValidation, ErrorMessage = ValidationMessages.InvalidFieldMessageHelp)]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        [RegularExpression(RegexValidation.NameValidation, ErrorMessage = ValidationMessages.InvalidFieldMessageHelp)]
        public string LastName { get; set; }
        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        [RegularExpression(RegexValidation.UserNameValidate, ErrorMessage = ValidationMessages.UserNameMessageHelp)]
        [Remote("ValidateUserName", ErrorMessage = "USERNAME IS TAKEN.", HttpMethod = "Post")]
        public string UserName { get; set; }
        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        [RegularExpression(RegexValidation.EmailValidate, ErrorMessage = ValidationMessages.InvalidFieldMessage)]
        public string Email { get; set; }
        [RegularExpression(RegexValidation.NameValidation, ErrorMessage = ValidationMessages.InvalidFieldMessageHelp)]
        [Display(Name = "Professional Suffix")]
        public string Credential { get; set; }
        public int AssistantTypeId { get; set; } = -1;
        [Display(Name = "Temporary Password")]
        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        public string TemporaryPassword { get; set; }
        [RegularExpression(RegexValidation.PasswordValidate, ErrorMessage = ValidationMessages.PasswordInvalidFieldMessage)]
        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        public string Password { get; set; }
        [Display(Name = "Confirm Password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        public string ConfirmPassword { get; set; }
        [Display(Name = "Suffix")]
        public string UserSuffix { get; set; }
    }
}
