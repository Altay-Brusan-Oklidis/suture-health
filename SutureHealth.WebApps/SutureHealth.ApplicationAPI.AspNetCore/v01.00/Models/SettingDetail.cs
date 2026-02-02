
namespace SutureHealth.Application.v0100.Models;

    public class SettingDetail
    {
        public int ParentId { get; set; }
        public string Key { get; set; }
        public bool? ItemBool { get; set; }
        public int? ItemInt { get; set; }
        public string ItemString { get; set; }
        public ItemType? ItemType { get; set; }
    }