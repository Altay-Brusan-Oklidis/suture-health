using System.ComponentModel;

namespace SutureHealth.AspNetCore.Areas.Network.Models
{
    public abstract class FilterCategory
    {
        public string Title { get; set; }
        public bool CanClearSection { get; set; }
        public string TooltipViewName { get; set; }
        public bool HasTooltip => !string.IsNullOrWhiteSpace(TooltipViewName);
    }

    public class UnifiedFilterCategory : FilterCategory
    {
        public FilterCategoryType Type { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Filters { get; set; }
    }

    public class SectionedFilterCategory : FilterCategory
    {
        public IEnumerable<FilterCategorySection> Sections { get; set; }
    }

    public class FilterCategorySection
    {
        public FilterCategoryType Type { get; set; }
        public string Title { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Filters { get; set; }
    }

    public enum FilterCategoryType
    {
        [Description("Account Type")]
        AccountType,
        [Description("Organizations")]
        Organizations,
        [Description("Clinicians")]
        Clinicians,
        [Description("Speciality")]
        Specialty
    }
}
