namespace SutureHealth.Application
{
    public class HierarchicalSetting : GenericSetting
    {
        private new int SettingId { get; set; }
        private new int? ParentId { get; set; }
    }
}
