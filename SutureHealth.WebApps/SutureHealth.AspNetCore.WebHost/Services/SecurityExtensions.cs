using Microsoft.EntityFrameworkCore;
using SutureHealth.AspNetCore.Areas.Network;

namespace SutureHealth.Application.Services
{
    public static class SecurityServicesExtensions
    {
        public static async Task<DateTimeOffset> InvokeUserLastAccessDateAsync(this IApplicationService securityService, int memberId, bool getOnly = false)
        {
            if (securityService == null)
            {
                throw new ArgumentNullException(nameof(securityService));
            }

            DateTimeOffset defaultDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(90));
            var setting = await securityService.GetMemberSettings(memberId).FirstOrDefaultAsync(s => s.Key == NetworkConfiguration.NETWORK_PAGE_USER_KEY);
            var existingDate = (setting?.IsActive ?? false) && long.TryParse(setting?.ItemString, out var ticks) ? new DateTimeOffset(ticks, TimeSpan.Zero) : defaultDate;

            if (existingDate < defaultDate)
            {
                existingDate = defaultDate;
            }

            if (!getOnly)
            {
                if (setting != null)
                {
                    await securityService.SetMemberSettingAsync(setting.SettingId, NetworkConfiguration.NETWORK_PAGE_USER_KEY, null, null, DateTimeOffset.UtcNow.Ticks.ToString(), Application.ItemType.String);
                }
                else
                {
                    await securityService.AddMemberSettingAsync(memberId, NetworkConfiguration.NETWORK_PAGE_USER_KEY, null, null, DateTimeOffset.UtcNow.Ticks.ToString(), Application.ItemType.String);
                }
            }

            return existingDate;
        }

        public static async Task<bool> IsNetworkEnabledAsync(this IApplicationService securityService, Organization organization)
        {
            if (securityService == null)
            {
                throw new ArgumentNullException(nameof(securityService));
            }

            // FULL ROLL OUT
            if (await securityService.GetApplicationSettings()
                                     .Where(s => s.Key == "network-page" && s.IsActive == true)
                                     .Select(s => s.ItemBool ?? false)
                                     .FirstOrDefaultAsync())
            {
                return true;
            }

            // TARGETED ROLL OUT
            if (organization != null && await securityService.GetApplicationSettings()
                                                             .Where(s => s.Key == "network-page-targeted-rollout" && s.IsActive == true)
                                                             .Select(s => s.ItemBool ?? false)
                                                             .FirstOrDefaultAsync())
            {
                var setting = await securityService.GetApplicationSettings()
                                                   .Where(s => s.Key == NetworkConfiguration.TARGETED_ROLLOUT_ALLOWED_STATES_KEY && s.IsActive == true)
                                                   .Select(s => s.ItemString)
                                                   .FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(setting))
                {
                    var states = setting.Split(',');
                    return states.Contains(organization.StateOrProvince, StringComparer.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        public static IQueryable<MemberRelationship> Supervisors(this Member member, IApplicationService securityServices)
            => securityServices.GetSupervisorsForMemberId(member.MemberId);
        public static IQueryable<MemberRelationship> Subordinates(this Member member, IApplicationService securityServices)
            => securityServices.GetSubordinatesForMemberId(member.MemberId);
    }
}
