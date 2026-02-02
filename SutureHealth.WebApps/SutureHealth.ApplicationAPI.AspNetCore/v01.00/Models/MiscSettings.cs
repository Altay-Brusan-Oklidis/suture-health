using SutureHealth.Application.v0100.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Application.v01._00.Models
{
    public class MiscSettings
    {
        public bool? ShowNewInbox { get; set; }
        public bool? AllowOtherToSignFromScreen { get; set; }
        public int? DocumentViewDuration { get; set; }
        public int? InboxPreference { get; set; }
        public string LastSavedForLaterDate { get; set; }
        public void FindMiscSettings(IEnumerable<SettingDetail> memberSettings,
            IEnumerable<SettingDetail> orgSettings,
            IEnumerable<SettingDetail> companySettings,
            IEnumerable<SettingDetail> appSettings)
        {
            var showNewInbox = GetBoolHierarchicalSetting(nameof(ShowNewInbox), memberSettings, orgSettings, appSettings);
            var allowOtherToSignFromScreen = GetAllowOtherToSignFromScreen(memberSettings, orgSettings, companySettings, appSettings);
            var documentViewDuration = GetIntHierarchicalSetting(nameof(DocumentViewDuration), memberSettings, orgSettings, appSettings);
            var inboxpreference = GetIntHierarchicalSetting(nameof(InboxPreference), memberSettings, orgSettings, appSettings);
            var last = GetStringHierarchicalSetting(nameof(LastSavedForLaterDate), memberSettings, orgSettings, appSettings);

            ShowNewInbox = showNewInbox != null && showNewInbox.ItemBool.Value;
            AllowOtherToSignFromScreen = allowOtherToSignFromScreen != null && allowOtherToSignFromScreen.ItemBool.Value;
            DocumentViewDuration = documentViewDuration != null ? documentViewDuration.ItemInt.Value : 2000;
            InboxPreference = inboxpreference != null ? inboxpreference.ItemInt.Value : 0;
            LastSavedForLaterDate = last != null ? last.ItemString : null;
        }

        private SettingDetail GetAllowOtherToSignFromScreen(IEnumerable<SettingDetail> userSettings,
                                                            IEnumerable<SettingDetail> orgSettings,
                                                            IEnumerable<SettingDetail> companySettings,
                                                            IEnumerable<SettingDetail> appSettings)
        {
            var settingName = "AllowOtherToSignFromScreen";
            var setting = userSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();
            if (setting == null || setting.ItemBool == null)
                setting = orgSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();
            if (setting == null || setting.ItemBool == null)
                setting = companySettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();
            if (setting == null || setting.ItemBool == null)
                setting = appSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();

            return setting;
        }

        private SettingDetail GetBoolHierarchicalSetting(string settingName, IEnumerable<SettingDetail> userSettings,
            IEnumerable<SettingDetail> orgSettings,
            IEnumerable<SettingDetail> appSettings)
        {
            var setting = userSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();
            if (setting == null || setting.ItemBool == null)
                setting = orgSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();            
            if (setting == null || setting.ItemBool == null)
                setting = appSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault(); 

            return setting;
        }

        private SettingDetail GetIntHierarchicalSetting(string settingName, IEnumerable<SettingDetail> userSettings,
            IEnumerable<SettingDetail> orgSettings,
            IEnumerable<SettingDetail> appSettings)
        {
            var setting = userSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();
            if (setting == null || setting.ItemInt == null)
                setting = orgSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();
            if (setting == null || setting.ItemInt == null)
                setting = appSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();

            return setting;
        }

        private SettingDetail GetStringHierarchicalSetting(string settingName, IEnumerable<SettingDetail> userSettings,
           IEnumerable<SettingDetail> orgSettings,
           IEnumerable<SettingDetail> appSettings)
        {
            var setting = userSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();
            if (setting == null || setting.ItemString == null)
                setting = orgSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();
            if (setting == null || setting.ItemString == null)
                setting = appSettings.Where(s => s.Key.ToLower() == settingName.ToLower()).FirstOrDefault();

            return setting;
        }

        public static bool HasDuplicateOrgSettings(IEnumerable<SettingDetail> orgSettings)
                => orgSettings.GroupBy(s => s.Key).Where(s => s.Count() > 1 && s.Key != "Services").Any();

        public static List<SettingDetail> GetDuplicateOrgSettings(IEnumerable<SettingDetail> orgSettings)
        {
            var settingList = orgSettings.ToList();
            var result = settingList.GroupBy(s => s.Key.ToLower()).Where(s => s.Count() > 1 && s.Key != "Services")
                             .SelectMany(s => s.ToList()).ToList();

            return result;
        }

        public static List<SettingDetail> RemoveDuplicateOrgSettings(IEnumerable<SettingDetail> orgSettings, int primOrgId)
        {
            var duplicateList = GetDuplicateOrgSettings(orgSettings);
            var settingList = orgSettings.ToList();
            var result = new List<SettingDetail>();

            foreach (var setting in settingList)
            {
                var different = !duplicateList.Any(d => d.Key.ToLower() == setting.Key.ToLower());
                var found = duplicateList.Any(d => d.Key.ToLower() == setting.Key.ToLower() &&
                                d.ParentId == primOrgId && setting.ParentId == primOrgId);

                if (found || different)
                    result.Add(setting);
            }

            return result;
        }
    }
}
