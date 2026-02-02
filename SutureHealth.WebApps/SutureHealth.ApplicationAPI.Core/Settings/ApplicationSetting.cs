namespace SutureHealth.Application
{
    public enum ItemType
    {
        Boolean = 1,
        Integer = 2,
        String = 3,
    }

    public class GenericSetting
    {
        public int SettingId { get; set; }
        public int? ParentId { get; set; }
        public string Key { get; set; }
        public bool? ItemBool { get; set; }
        public int? ItemInt { get; set; }
        public string ItemString { get; set; }
        public bool? IsActive { get; set; }
        public ItemType? ItemType { get; set; }
    }

    public class ApplicationSetting : GenericSetting
    { }

    public class MemberSetting : GenericSetting
    {
        public virtual Member Parent { get; set; }
    }

    public class OrganizationSetting : GenericSetting
    {
        public virtual Organization Parent { get; set; }
    }

    public static class GenericSettingExtensions
    {
        public static void CopyTo(this GenericSetting source, GenericSetting destination)
        {
            destination.SettingId = source.SettingId;
            destination.IsActive = source.IsActive;
            destination.ItemBool = source.ItemBool;
            destination.ItemInt = source.ItemInt;
            destination.ItemString = source.ItemString;
            destination.ItemType = source.ItemType;
            destination.Key = source.Key;
            destination.ParentId = source.ParentId;
        }
    }
}