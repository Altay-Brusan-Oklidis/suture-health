namespace SutureHealth.AspNetCore.Areas.Request.Models.Send
{
    public class UpdatePatientModel
    {
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string SocialSecurityNumber { get; set; }
        public SocialSecurityNumberStyle? SocialSecurityNumberType { get; set; }
        public string MedicareMbi { get; set; }
        public bool CanEditMedicareMbi { get; set; }

        public bool HasGender => Gender.HasValue && (Gender.Value == SutureHealth.Gender.Male || Gender.Value == SutureHealth.Gender.Female);
        public bool HasSocialSecurityNumber => !string.IsNullOrWhiteSpace(SocialSecurityNumber);
        public bool HasSocialSecurityNumberType => SocialSecurityNumberType.HasValue && SocialSecurityNumberType.Value != SocialSecurityNumberStyle.Unavailable;
        public bool HasMedicareMbi => !string.IsNullOrWhiteSpace(MedicareMbi);

        public enum SocialSecurityNumberStyle
        {
            Unavailable = 0,
            Full,
            Last4
        }
    }
}
