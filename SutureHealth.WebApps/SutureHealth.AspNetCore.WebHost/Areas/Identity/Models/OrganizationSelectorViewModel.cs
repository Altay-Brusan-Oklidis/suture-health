using Microsoft.AspNetCore.Mvc.Rendering;

namespace SutureHealth.AspNetCore.Areas.Identity.Models
{
    public class OrganizationSelectorViewModel
    {
        public IEnumerable<SelectListItem> Organizations { get; set; }
    }
}
