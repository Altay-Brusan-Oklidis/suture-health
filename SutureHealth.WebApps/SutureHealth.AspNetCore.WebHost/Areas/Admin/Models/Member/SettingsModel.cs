using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Member
{
    public class SettingsModel : BaseViewModel
    {
        public string UserName { get; set; }
        public SettingsGridModel SettingsGrid { get; set; }
    }
}
