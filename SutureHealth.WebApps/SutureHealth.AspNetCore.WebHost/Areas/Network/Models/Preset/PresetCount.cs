namespace SutureHealth.AspNetCore.Areas.Network.Models.Preset
{
    public class PresetCount
    {
        public IEnumerable<PresetCountItem> Presets { get; set; }

        public class PresetCountItem
        {
            public string Key { get; set; }
            public long Count { get; set; }
            public string DisplayCount { get; set; }
        }
    }
}
