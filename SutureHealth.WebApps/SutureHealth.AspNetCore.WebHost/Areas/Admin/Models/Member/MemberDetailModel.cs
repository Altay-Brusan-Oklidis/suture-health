namespace SutureHealth.AspNetCore.Areas.Admin.Models.Member
{
    public class MemberDetailModel
    {
        public string GridName { get; set; }
        public IEnumerable<MemberDetailListItem> Organizations { get; set; }
        public AddEntityToOrganizationDialogModel AddOrganizationDialog { get; set; }

        public class MemberDetailListItem
        {
            public int OrganizationId { get; set; }
            public string Name { get; set; }
            public string InternalName { get; set; }
            public bool IsAdministrator { get; set; }
            public bool IsActive { get; set; }
            public bool IsPrimary { get; set; }
        }

        public class OrganizationListItem : OrganizationSearchListItem
        {
            public string EditMemberUrl { get; set; }
        }
    }
}
