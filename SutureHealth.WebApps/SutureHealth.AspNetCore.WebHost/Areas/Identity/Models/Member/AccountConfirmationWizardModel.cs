namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class AccountConfirmationWizardModel
    {
        public const string EVENT_SEND_EMAIL = "AccountConfirmationWizard:SendEmail";
        public const string EVENT_SEND_CODE = "AccountConfirmationWizard:SendCode";

        public string VerifyMobileNumberCodeActionUrl { get; set; }
        public bool HasAnyWizard => HasEmailConfirmation || HasPhoneConfirmation;
        public EmailConfirmationWizard EmailConfirmation { get; set; }
        public PhoneConfirmationWizard PhoneConfirmation { get; set; }
        public bool HasEmailConfirmation => EmailConfirmation != null;
        public bool HasPhoneConfirmation => PhoneConfirmation != null;

        public class EmailConfirmationWizard
        {
            public string Email { get; set; }
        }

        public class PhoneConfirmationWizard
        {
            public string MobileNumber { get; set; }
            public string MobileConfirmationReturnUrl { get; set; }
        }
    }
}
