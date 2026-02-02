using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Patient
{
    public class SearchModel : BaseViewModel
    {
        public AddEntityToOrganizationDialogModel AddDialog { get; set; }

        public class OrganizationListItem : OrganizationSearchListItem
        {
            public string CreatePatientUrl { get; set; }
        }
    }
}
