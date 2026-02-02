using System.Linq;
using System.Threading.Tasks;

namespace SutureHealth.Application.Services
{
    public interface IApplicationSettingService
    {
        Task DeleteApplicationSettingAsync(int settingId);
        IQueryable<ApplicationSetting> GetApplicationSettings();
        Task<ApplicationSetting> SetApplicationSettingAsync(int settingId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active = true);
        Task<ApplicationSetting> AddApplicationSettingAsync(string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active = true);

        Task DeleteMemberSettingAsync(int settingId);
        IQueryable<MemberSetting> GetMemberSettings();
        IQueryable<MemberSetting> GetMemberSettings(int memberId);
        Task<MemberSetting> SetMemberSettingAsync(int settingId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active = true);
        Task<MemberSetting> AddMemberSettingAsync(int memberId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active = true);

        Task DeleteOrganizationSettingAsync(int settingId);
        IQueryable<OrganizationSetting> GetOrganizationSettings();
        IQueryable<OrganizationSetting> GetOrganizationSettings(int organizationId);
        Task<OrganizationSetting> SetOrganizationSettingAsync(int settingId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active = true);
        Task<OrganizationSetting> AddOrganizationSettingAsync(int organizationId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active = true);

        HierarchicalSetting GetHierarchicalSetting(string key, int memberId, int? organizationId = null);
        bool ShowLegacyNavBar(bool isSender, int memberId, bool contentOnly = false);
    }
}
