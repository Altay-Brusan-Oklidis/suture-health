using System.ComponentModel;
using System.Linq.Expressions;
using SutureHealth.Providers;

namespace SutureHealth.AspNetCore.Areas.Network.Models.Listing
{
    public abstract class SearchFilter
    {
        public FilterPreset? Preset { get; set; }
        public FilterToken[] Filters { get; set; }
        public string Search { get; set; } = string.Empty;
        public SearchRadius Radius { get; set; } = SearchRadius.Infinity;
        public SearchFilterSortMethod SortOrder { get; set; } = SearchFilterSortMethod.DistanceClosestFirst;
        public int ProvidersOnPage { get; set; } = 25;
    }

    public enum SearchFilterSortMethod
    {
        [Description("Claims History")]
        ClaimsHistory,
        [Description("Distance")]
        DistanceClosestFirst,
        [Description("Name")]
        Name,
        [Description("Recently Joined")]
        RecentlyJoined
    }

    public enum SearchRadius
    {
        Infinity = 2048,
        TwentyFive = 25,
        Fifty = 50,
        SeventyFive = 75,
        OneHundred = 100,
        EntireState = 0
    }

    public class FilterToken
    {
        public FilterCategoryType SectionKey { get; set; }
        public string FilterKey { get; set; }
    }

    public interface IFilterMapping<TSource>
        where TSource : class
    {
        string Name { get; set; }
        Expression<Func<TSource, bool>> Mapping { get; set; }
    }

    public class ProviderEntityMapping : IFilterMapping<NetworkProvider>
    {
        public string Name { get; set; }
        public Expression<Func<NetworkProvider, bool>> Mapping { get; set; }
    }

    public static class NetworkProviderExtensions
    {
        public static IEnumerable<NetworkProvider> OrderBy(this IEnumerable<NetworkProvider> providers, SearchFilterSortMethod sortMethod)
        {
            switch (sortMethod)
            {
                case SearchFilterSortMethod.ClaimsHistory:
                    return providers.OrderByDescending(i => i.MedicareClaimsCountWithProvider)
                                    .ThenBy(i => i.Distance);
                case SearchFilterSortMethod.Name:
                    return providers.OrderBy(i => i.FullName);
                case SearchFilterSortMethod.RecentlyJoined:
                    return providers.OrderByDescending(i => i.SutureCreatedAt)
                                    .ThenBy(i => i.Distance);
                case SearchFilterSortMethod.DistanceClosestFirst:
                default:
                    return providers.OrderBy(i => i.Distance);
            }
        }

        public static SutureHealth.Providers.Services.SearchFilterSortMethod AsProviderServiceSortMethod(this SearchFilterSortMethod sortMethod)
        {
            return Enum.GetValues(typeof(SutureHealth.Providers.Services.SearchFilterSortMethod)).Cast<SutureHealth.Providers.Services.SearchFilterSortMethod>().First(psm => string.Equals(psm.ToString(), sortMethod.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
