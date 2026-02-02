using Microsoft.AspNetCore.Mvc.Rendering;
using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Revenue.Models
{
    public class IndexViewModel : BaseViewModel
    {
        public MetricsViewModel Metrics { get; set; }
        public IEnumerable<SelectListItem> Offices { get; set; }
        public IEnumerable<SelectListItem> Providers { get; set; }
        public IEnumerable<SelectListItem> Statements { get; set; }

        public bool HasMultipleOffices => Offices != null && Offices.Count() > 1;
        public bool HasMultipleProviders => Providers != null && Providers.Count() > 1;
    }

    public static class SelectListItemExtensions
    {
        public static IEnumerable<SelectListItem> GetWithAllSelection(this IEnumerable<SelectListItem> items)
        {
            if (items == null || items.Count() <= 1)
            {
                return items;
            }

            return (new SelectListItem[] { new SelectListItem("All", "0") }).Union(items);
        }
    }
}
