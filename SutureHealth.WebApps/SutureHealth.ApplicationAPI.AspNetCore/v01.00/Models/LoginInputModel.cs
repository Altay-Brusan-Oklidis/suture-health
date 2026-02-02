using SutureHealth.Application;
using System.ComponentModel.DataAnnotations;

namespace SutureHealth.Application.v0100.Models;

public class LoginInputModel
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
    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; } = true;

    public string PublicId { get; set; } = null;
}