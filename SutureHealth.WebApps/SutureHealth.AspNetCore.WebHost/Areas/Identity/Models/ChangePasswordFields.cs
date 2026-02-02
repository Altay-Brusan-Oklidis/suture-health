using SutureHealth.Application;
using System.ComponentModel.DataAnnotations;

namespace SutureHealth.AspNetCore.Areas.Identity.Models
{
    public abstract class ChangePasswordFields
    {
        [Required(ErrorMessage = "REQUIRED")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(RegexValidation.PasswordValidate, ErrorMessage = ValidationMessages.PasswordInvalidFieldMessage)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "REQUIRED")]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
