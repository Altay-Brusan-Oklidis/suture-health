using System.ComponentModel.DataAnnotations;
using SutureHealth.Application;

namespace SutureHealth.AspNetCore.Areas.Admin.Models
{
    public class SettingsListItem
    {
        public SettingsListItem() { }

        public SettingsListItem(GenericSetting setting)
        {
            SettingId = setting.SettingId;
            Name = setting.Key;
            Value = setting.ItemType switch
            {
                Application.ItemType.Integer => setting.ItemInt.GetValueOrDefault().ToString(),
                Application.ItemType.Boolean => setting.ItemBool.GetValueOrDefault().ToString().ToLower(),
                _ => setting.ItemString
            };
            Active = setting.IsActive.GetValueOrDefault();
            Type = setting.ItemType.GetValueOrDefault(Application.ItemType.String).ToString();
        }

        public int SettingId { get; set; }
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string Value { get; set; }
        [Required]
        public string Type { get; set; }
        public bool Active { get; set; }
    }
}
