using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class AccountModel : BaseViewModel, IAccountModel
    {
        public int MemberId { get; set; }
        public bool CanSetSigningName { get; set; }
        public bool IsNpiRequired { get; set; }
        public string ReturnUrl { get; set; }
        public AlertViewModel SaveAlert { get; set; }
        public bool HasSaveAlert => SaveAlert != null;
        public AlertViewModel ConfirmEmailAlert { get; set; }
        public bool HasConfirmEmailAlert => ConfirmEmailAlert != null;
        public string SendConfirmationEmailActionUrl { get; set; }
        public string SendMobileNumberConfirmationActionUrl { get; set; }
        public AccountConfirmationWizardModel ConfirmationWizard { get; set; }
        public bool HasConfirmationWizard => ConfirmationWizard != null;

        public bool IsExternalUser { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^$|^([A-Za-z,.' -]{1,50}$)", ErrorMessage = "First Name accepts only alphabetical characters")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^$|^([A-Za-z,.' -]{1,50}$)", ErrorMessage = "Last Name accepts only alphabetical characters")]
        public string LastName { get; set; }

        [DataType(DataType.Text)]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "INVALID")]
        [Remote("MemberIsNpiAvailable", AdditionalFields = "MemberId", HttpMethod = "POST", ErrorMessage = "NPI IS TAKEN")]
        public string Npi { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^(([^<>()\[\]\\.,;:\s@']+(\.[^<>() \[\]\\.,;:\s@']+)*)|('.+'))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$", ErrorMessage = "Invalid Email!")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Phone]
        [RegularExpression(@"^(\+?1[\s.-]*)?\(?[2-9]\d{2}\)?[\s.-]*[2-9]\d{2}[\s.-]*\d{4}$", ErrorMessage = "Invalid Phone Number!")]
        public string MobileNumber { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Phone]
        [RegularExpression(@"^(\+?1[\s.-]*)?\(?[2-9]\d{2}\)?[\s.-]*[2-9]\d{2}[\s.-]*\d{4}$", ErrorMessage = "Invalid Phone Number!")]
        public string OfficePhone { get; set; }
        public string OfficePhoneExtension { get; set; }

        public string Suffix { get; set; }
        public string ProfessionalSuffix { get; set; }
        [Required(ErrorMessage = "REQUIRED")]
        public string SigningName { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [DataType(DataType.Text)]
        [RegularExpression(@"^[A-Za-z0-9-_@.]{3,50}$", ErrorMessage = "Username accepts only alphanumeric and [-_@.] characters")]
        [Remote("MemberIsUserNameAvailable", AdditionalFields = "MemberId", HttpMethod = "POST", ErrorMessage = "USERNAME IS TAKEN")]
        public string UserName { get; set; }

        public CommunicationPreferencesModel CommunicationPreferences { get; set; }
        public bool HasCommunicationPreferences => CommunicationPreferences != null;

        public AlertViewModel SendConfirmationEmailReSentAlert => new AlertViewModel()
        {
            Id = "SendConfirmationEmailReSentAlert",
            FadeSeconds = 4,
            Hidden = true,
            Style = AlertViewModel.ContextClass.Success,
            Text = "An email has been sent."
        };

        public AlertViewModel SendMobileNumberConfirmationAlert => new AlertViewModel()
        {
            Id = "SendMobileNumberConfirmationAlert",
            FadeSeconds = 4,
            Hidden = true,
            Style = AlertViewModel.ContextClass.Success,
            Text = "A code has been sent to your mobile number."
        };

        public enum PostSaveType
        {
            None = 0,
            Page,
            MobileVerification
        }
    }
}
