using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Template
{
    public class OrganizationSearchModel : BaseViewModel
    {
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string AddTemplateUrl { get; set; }
    }
}
