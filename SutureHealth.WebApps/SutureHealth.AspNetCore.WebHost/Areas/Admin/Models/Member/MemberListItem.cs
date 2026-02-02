namespace SutureHealth.AspNetCore.Areas.Admin.Models.Member
{
    public class MemberListItem
    {
        public int MemberId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int MemberTypeId { get; set; }
        public string MemberType { get; set; }  // Occupation
        public string Role { get; set; }
        public string Npi { get; set; }
        public bool IsActive { get; set; }
        public bool IsPaid { get; set; }
        public bool IsLocked { get; set; }
        public bool IsRegistered { get; set; }
        public DateTimeOffset? DateLastLoggedIn { get; set; }
        public string DateLastLoggedInDisplay { get; set; }
        public DateTime? DateCreated { get; set; }
        public string DateCreatedDisplay { get; set; }
        public string DetailUrl { get; set; }
        public string EditActionUrl { get; set; }
        public string ChangePasswordModalUrl { get; set; }
        public string SendRegistrationEmailModalUrl { get; set; }
        public string ToggleActiveModalUrl { get; set; }
        public string UnlockActionUrl { get; set; }
        public string SettingsActionUrl { get; set; }
        public string CommunicationPreferencesActionUrl { get; set; }
    }
}
