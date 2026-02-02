namespace SutureHealth.AspNetCore.Areas.Network.Models.Network
{
    public class PresetSelection
    {
        public PresetSelection(FilterPreset preset)
        {
            this.Preset = preset;
        }

        protected static readonly IReadOnlyDictionary<FilterPreset, string> PRESET_TITLE = new Dictionary<FilterPreset, string>()
        {
            { FilterPreset.NearMe, FilterPreset.NearMe.GetEnumDescription() },
            { FilterPreset.MyNetwork, FilterPreset.MyNetwork.GetEnumDescription() },
            { FilterPreset.ClaimsWithMe, FilterPreset.ClaimsWithMe.GetEnumDescription() },
            { FilterPreset.Invitations, FilterPreset.Invitations.GetEnumDescription() },
            { FilterPreset.RecentlyJoined, FilterPreset.RecentlyJoined.GetEnumDescription() },
            { FilterPreset.InviteSenders, FilterPreset.InviteSenders.GetEnumDescription() }
        };
        protected static readonly IReadOnlyDictionary<FilterPreset, string> PRESET_KEY = new Dictionary<FilterPreset, string>()
        {
            { FilterPreset.NearMe, FilterPreset.NearMe.ToString() },
            { FilterPreset.MyNetwork, FilterPreset.MyNetwork.ToString() },
            { FilterPreset.ClaimsWithMe, FilterPreset.ClaimsWithMe.ToString() },
            { FilterPreset.Invitations, FilterPreset.Invitations.ToString() },
            { FilterPreset.RecentlyJoined, FilterPreset.RecentlyJoined.ToString() },
            { FilterPreset.InviteSenders, FilterPreset.InviteSenders.ToString() }
        };
        protected static readonly IReadOnlyDictionary<FilterPreset, string> PRESET_ICON = new Dictionary<FilterPreset, string>()
        {
            { FilterPreset.NearMe, "far fa-street-view" },
            { FilterPreset.MyNetwork, "far fa-chart-network" },
            { FilterPreset.ClaimsWithMe, "far fa-file-medical-alt" },
            { FilterPreset.Invitations, "far fa-envelope" },
            { FilterPreset.RecentlyJoined, "far fa-user-friends" },
            { FilterPreset.InviteSenders, "far fa-envelope" }
        };

        public FilterPreset Preset { get; set; }
        public bool HasCount { get; set; } = true;
        public bool ShouldQuery { get; set; } = true;
        public string Key => PRESET_KEY.ContainsKey(this.Preset) ? PRESET_KEY[this.Preset] : string.Empty;
        public string Title => PRESET_TITLE.ContainsKey(this.Preset) ? PRESET_TITLE[this.Preset] : string.Empty;
        public string IconClass => PRESET_ICON.ContainsKey(this.Preset) ? PRESET_ICON[this.Preset] : string.Empty;
    }
}
