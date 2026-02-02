using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Organization
{
    public class SettingsModel : BaseViewModel
    {
        public string OrganizationName { get; set; }
        public SettingsGridModel SettingsGrid { get; set; }
    }
}
