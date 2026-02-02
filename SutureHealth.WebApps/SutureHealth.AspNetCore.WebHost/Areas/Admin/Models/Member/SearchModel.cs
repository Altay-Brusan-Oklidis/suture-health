using System.ComponentModel;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Application;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Member
{
    public class SearchModel : BaseViewModel
    {
        public IEnumerable<KeyValuePair<int, string>> MemberTypes => Enum.GetValues<MemberType>()
                                                                         .Where(mt => mt != MemberType.Unknown)
                                                                         .Select(mt => new KeyValuePair<int, string>((int)mt, mt.GetEnumDescription()));
        public IEnumerable<KeyValuePair<int, string>> Roles => Enum.GetValues<RoleTypes>()
                                                                   .Select(rt => new KeyValuePair<int, string>((int)rt, rt.GetEnumDescription()));
        public AddEntityToOrganizationDialogModel AddDialog { get; set; }
        public string OrganizationDataSourceUrl { get; set; }

        public enum RoleTypes
        {
            [Description("Sender")]
            Sender,
            [Description("Collaborator")]
            Collaborator,
            [Description("Signer")]
            Signer,
            [Description("Signer | Collaborator")]
            SignerCollaborator,
            [Description("Staff")]
            Staff
        }

        public class OrganizationListItem : OrganizationSearchListItem
        {
            public string CreateMemberUrl { get; set; }
        }
    }
}
