using System.ComponentModel;
using SutureHealth.AspNetCore.Areas.Network.Models.Network;
using SutureHealth.AspNetCore.Areas.Network.Models.Listing;
using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Network.Models
{
    public class IndexViewModel : BaseViewModel
    {
        public const string EVENT_QUERY_CHANGED = "SutureHealth:Index:QueryChanged";
        public const string EVENT_QUERY_RETRY = "SutureHealth:Index:QueryRetry";
        public const string EVENT_MASK_CHANGED = "SutureHealth:Index:MaskChanged";
        public const string EVENT_DISPLAY_WARNING = "SutureHealth:Index:DisplayWarning";
        public const string EVENT_DISPLAY_MESSAGE = "SutureHealth:Index:DisplayMessage";
        public const string EVENT_VIEW_STYLE_CHANGED = "SutureHealth:Index:ViewStyleChanged";
        public const string EVENT_INVITE_COMPLETED = "SutureHealth:Index:InviteCompleted";
        public const string EVENT_PRESET_COUNT_RECEIVED = "SutureHealth:Index:PresetCountReceived";

        public const int VIEW_STYLE_TRANSITION_THRESHOLD_PIXELS = 1024;

        public InitializationParameters Initialization { get; set; }
        public ModeSelector Mode { get; set; }
        public FilterSelector Filter { get; set; }
        public SearchHeader Header { get; set; }
        public EntityListing Listing { get; set; }

        public class ModeSelector
        {
            public const string EVENT_PRESET_CHANGED = "SutureHealth:ModeSelector:PresetChanged";

            public IEnumerable<PresetSelection> Presets { get; set; }
        }

        public class FilterSelector
        {
            public const string EVENT_FILTERS_CHANGED = "SutureHealth:FilterSelector:FiltersChanged";

            public FilterCategory AccountType { get; set; }
            public FilterCategory Provider { get; set; }
            public FilterCategory Specialty { get; set; }
        }

        public class EntityListing
        {
            public const string EVENT_INVITATIONS_CHANGED = "SutureHealth:EntityListing:InvitationsChanged";

            public long LastInvocationDateTicks { get; set; }
            public int PageSize { get; set; }
            public string UpgradeUrl { get; set; }
            public string NewFeatureUrl { get; set; }
        }

        public class PagedEntityListing
        {
            public bool IsFirstPage { get; set; }
            public bool HasMorePages { get; set; }
            public bool HasEntities => this.Entities != null && this.Entities.Any();
            public IEnumerable<EntityListItem> Entities { get; set; }
        }

        public class GroupedEntityListing
        {
            public IEnumerable<KeyValuePair<string, IEnumerable<EntityListItem>>> EntitiesByGroup { get; set; }
        }

        public class FailedEntityListing
        {
            public bool ShouldRetry { get; set; }
            public int RetryCount { get; set; }
            public bool DisplayMessage { get; set; }
        }

        public class SearchHeader
        {
            public const string EVENT_SEARCH_CHANGED = "SutureHealth:SearchHeader:SearchChanged";
            public const string EVENT_INVITE = "SutureHealth:SearchHeader:Invite";
            public const string EVENT_DOWNLOAD = "SutureHealth:SearchHeader:Download";
        }

        public class InitializationParameters
        {
            public ListingRequest QueryState { get; set; }
            public IReadOnlyDictionary<string, ListingRequest> Presets { get; set; }
            public EntityViewStyle? ViewStyle { get; set; }
        }

        public enum EntityViewStyle
        {
            [Description("row-listing")]
            Row = 0,
            [Description("grid-listing")]
            Grid = 1
        }
    }
}
