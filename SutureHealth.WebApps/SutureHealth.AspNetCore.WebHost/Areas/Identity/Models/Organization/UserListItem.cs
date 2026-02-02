namespace SutureHealth.AspNetCore.Areas.Identity.Models.Organization
{
    public class UserListItem
    {
        public int UserId { get; set; }
        public bool Active { get; set; }
        public bool Admin { get; set; }
        public bool Locked { get; set; }
        public bool Registered { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Profession { get; set; }
        public string Role { get; set; }
        public string EditUserUrl { get; set; }
        public string ChangePasswordModalUrl { get; set; }
        public string UnlockAccountModalUrl { get; set; }
        public string SendRegistrationEmailModalUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTimeOffset? LastLoginDate { get; set; }
        public DateTime? DateDeactivated { get; set; }
        public string CreatedDateDisplay { get; set; }
        public string DateDeactivatedDisplay { get; set; }
    }
}
