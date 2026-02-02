namespace SutureHealth.Application
{
    public static class RegexValidation
    {
        public const string NameValidation = @"^$|^([A-Za-z,.' -]{1,50}$)";
        public const string EmailValidate = @"^(([^<>()\[\]\\.,;:\s@]+(\.[^<>() \[\]\\.,;:\s@]+)*)|('.+'))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";
        public const string AddressValidate = @"^[a-zA-Z0-9\/\s\#, '-]*$";
        public const string CityValidate = @"^[A-z '-]+$";
        public const string PatientRecord = @"^$|^(.{3,50}$)";
        public const string NumberValidate = @"^[0-9]*$";
        public const string UserNameValidate = @"^[A-Za-z0-9-_@.+]{3,50}$";
        public const string AlphaNumericOnly = @"^[A-Za-z0-9]+$";
        public const string PasswordValidate = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d !""#$%&'()*+,.\/:;<=>?@\[\\\]^_`{|}~-]{8,50}$";
        public const string SSNValidate = @"^(?!000)(?!666)(?!9)\d{3}[-](?!00)\d{2}[-](?!0000)\d{4}?$";
        public const string SSNValidateLastFourDigits = @"^(?!0000)([0-9]{4})$";
        public const string PatientMedicaidValidate = @"^$|^[a-zA-Z0-9]{3,20}$";
        public const string DateValidate = @"^(0?[1-9]|1[0-2])\/(0?[1-9]|1\d|2\d|3[01])\/(19|20)\d{2}$";
        public const string PhoneValidate = @"^(\+?1[\s.-]*)?\(?[2-9]\d{2}\)?[\s.-]*[2-9]\d{2}[\s.-]*\d{4}$";
        public const string NPIValidate = @"^[0-9]{10}$";
        public const string ZipValidate = @"^[0-9]{5}$";
        public const string OrganizationMedicareNumber = @"^[0-9]{5}[a-zA-Z0-9]{1}$";

        public const string SSNFullValidate = @"^(?!000)(?!666)(?!9)\d{3}[-](?!00)\d{2}[-](?!0000)\d{4}?$|^(?!0000)([0-9]{4})$";

        //Medicare Validation Regex
        public const string RgxMedicareSameSNN = @"^$|^(?!000)(?!666)(?!9)(?!123456789)\d{3}(?!00)\d{2}(?!0000)\d{4}(a|ta|t|m1|m|j[1-4]){1}$";
        public const string RgxMedicare = @"^$|^(?!000)(?!666)(?!9)(?!123456789)\d{3}(?!00)\d{2}(?!0000)\d{4}([b][1-9|a|d|g|h|j|k|l|n|p|q|r|t|w|y]?|c[a-z1-9]|d[a|c|d|g|h|j-n|p-t|v-z|1-9]?|e[1-9|a-d|f-h|j|k|m]?|f[1-8]|k[1-9|a-h|j|l|m]|t[b-h|j-n|p-z|2]|w[1-9|b|c|f|g|j|r|t]?)$";
        public const string RgxMedicarePrefix = @"^$|^(ma|mh|wa|wh|wd|wca|wcd|wch|ca|pa|pd|ph|ja)((\d{6})|((?!000)(?!666)(?!9)(?!123456789)\d{3}(?!00)\d{2}(?!0000)\d{4}?))$";
        public const string RgxPrefixMedicareSameSNN = @"^[a|h]{1}(?!000)(?!666)(?!9)(?!123456789)\d{3}(?!00)\d{2}(?!0000)\d{4}$";
        public const string NotMatchMedicarePrefix = @"^$|^(a|h)((\d{6})?)$";

        public const string RgxMedicareMBI = @"^[1-9]{1}[^SLOIBZsloibz|^0-9]{1}[^SLOIBZsloibz]{1}[0-9]{1}-?[^SLOIBZsloibz|^0-9]{1}[^SLOIBZsloibz]{1}[0-9]{1}-?[^SLOIBZsloibz|^0-9]{1}[^SLOIBZsloibz|^0-9]{1}[0-9]{1}[0-9]{1}$";
        public const string RgxOldMedicareMBI = @"^[1-9]{4}-?[1-9]{3}-?[1-9]{2}[A-Za-z]{1}$";

        public const string RgxMedicaidNumbers = @"^$|^[a-zA-Z0-9]{3,20}$";
    }

    public static class ValidationMessages
    {
        public const string InvalidFieldMessage = "INVALID";

        public const string RequiredFieldMessage = "REQUIRED";
        public const string MaxLengthMessage = "{0} can't be more than {1} charactor.";
        public const string MinLengthMessage = "{0} should be more than {1} charactor.";
        public const string UniqueMessage = "{0} should be unique.";
        public const string MedicareRequired = "Submit at least one of the Medicare Numbers below";
        public const string InvalidMBI = InvalidFieldMessage + ". See MBI Format link below";
        public const string PasswordInvalidFieldMessage = "Password requirements not met";

        public const string ConfirmPasswordNotSame = "Password and Confirm Passwords don\'t match";
        public const string TemporaryAndNewPasswordNot = "Temporary password and new password can not be same";
        public const string PasswordNotContainUserName = "Your password may not contain segments of your username";

        public const string InvalidFieldMessageHelp = InvalidFieldMessage + ": This accepts only alphabetical characters";
        public const string UserNameMessageHelp = InvalidFieldMessage + ": Username accepts only alphanumeric and [-_+@.] characters";

        public const string RequiredInvalidUserFacilityMessage = "At least one facility must be active and set to default.";
        public const string RequiredInvalidUserRelationMessage = "At least one physician must be active for assistants and collaborators.";

        public const string UserNameTakenMessage = "USERNAME IS TAKEN";
        public const string NPITakenMessage = "NPI IS TAKEN.";
        public const string MedicareTakenMessage = "MEDICARE CERTIFICATION NUMBER IS TAKEN.";

    }
}
