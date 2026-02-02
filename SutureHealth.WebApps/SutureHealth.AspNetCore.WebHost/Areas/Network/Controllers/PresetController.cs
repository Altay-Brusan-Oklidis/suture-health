using Microsoft.AspNetCore.Mvc;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.Areas.Network.Models;
using SutureHealth.AspNetCore.Areas.Network.Models.Preset;
using SutureHealth.Providers;
using SutureHealth.Providers.Services;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Network.Controllers
{
    [Area("Network")]
    [Route("Network")]
    public class PresetController : Controller
    {
        protected IApplicationService SecurityService { get; }
        protected INetworkServicesProvider ProviderService { get; }

        public PresetController
        (
            IApplicationService securityService,
            INetworkServicesProvider providerService
        )
        {
            SecurityService = securityService;
            ProviderService = providerService;
        }

        [HttpPost]
        [Route("PresetCount", Name = "NetworkPresetCount")]
        public async Task<ActionResult> PresetCount([FromBody] FilterPreset[] presets)
        {
            var facility = await SecurityService.GetOrganizationByIdAsync(CurrentUser.PrimaryOrganizationId);
            var results = new Dictionary<FilterPreset, long>();
            var tasks = presets.ToDictionary(k => k, k => null as System.Threading.Tasks.Task);
            if (facility != null && !string.IsNullOrWhiteSpace(facility.PostalCode))
            {
                foreach (var preset in presets)
                {
                    switch (preset)
                    {
                        case FilterPreset.MyNetwork:
                            tasks[preset] = this.GetMyNetworkPresetQuery(CurrentUser, facility.OrganizationId);
                            break;
                        case FilterPreset.ClaimsWithMe:
                            if (CurrentUser.IsUserSigningMember() && CurrentUser.IsUserSigner())
                            {
                                tasks[preset] = this.ProviderService.CountProvidersByPreset(preset: PresetNetworkFilterType.ClaimsWithMe,
                                                                                            signerUserId: CurrentUser.Id,
                                                                                            senderFacilityId: null,
                                                                                            targetPostalCode: null,
                                                                                            distance: null,
                                                                                            fromDate: null);
                            }
                            else if (CurrentUser.IsUserSigningMember())
                            {
                                tasks[preset] = this.ProviderService.CountProvidersByPreset(preset: PresetNetworkFilterType.ClaimsWithMe,
                                                                                            signerUserId: CurrentUser.Id,
                                                                                            senderFacilityId: null,
                                                                                            targetPostalCode: null,
                                                                                            distance: null,
                                                                                            fromDate: null);
                            }
                            else
                            {
                                tasks[preset] = this.ProviderService.CountProvidersByPreset(preset: PresetNetworkFilterType.ClaimsWithMe,
                                                                                            signerUserId: null,
                                                                                            senderFacilityId: facility.OrganizationId,
                                                                                            targetPostalCode: null,
                                                                                            distance: null,
                                                                                            fromDate: null);
                            }
                            break;
                        case FilterPreset.Invitations:
                            tasks[preset] = this.ProviderService.CountProvidersByPreset(preset: PresetNetworkFilterType.Invitations,
                                                                                        signerUserId: CurrentUser.Id,
                                                                                        senderFacilityId: null,
                                                                                        targetPostalCode: null,
                                                                                        distance: null,
                                                                                        fromDate: null);
                            break;
                        case FilterPreset.RecentlyJoined:
                        case FilterPreset.NearMe:
                            var proximity = 50;
                            var createdDate = new DateTime?();

                            if (preset == FilterPreset.RecentlyJoined)
                            {
                                proximity = CurrentUser.IsUserSigningMember() ? 50 : 100;
                                createdDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(90));
                            }

                            tasks[preset] = this.ProviderService.CountProvidersByPreset(preset: null,
                                                                                        signerUserId: null,
                                                                                        senderFacilityId: null,
                                                                                        targetPostalCode: facility.PostalCode,
                                                                                        distance: proximity,
                                                                                        fromDate: createdDate);
                            break;
                        default:
                            break;
                    }
                }

                if (tasks.Values.Any(t => t != null))
                {
                    try
                    {
                        await System.Threading.Tasks.Task.WhenAll(tasks.Values.Where(t => t != null));
                    }
                    catch { }
                    foreach (var completed in tasks)
                    {
                        if (completed.Value.Status != TaskStatus.Faulted)
                        {
                            var queryOperationResult = (completed.Value as System.Threading.Tasks.Task<int>)?.Result;
                            results.Add(completed.Key, queryOperationResult.GetValueOrDefault());
                        }
                        else
                        {
                            results.Add(completed.Key, 0);
                        }
                    }
                }
            }

            return Json(new PresetCount()
            {
                Presets = results.Select(p => new PresetCount.PresetCountItem()
                {
                    Key = p.Key.ToString(),
                    Count = p.Value,
                    DisplayCount = p.Value.ToString("n0")
                })
            });
        }

        protected async Task<int> GetMyNetworkPresetQuery(MemberIdentity sutureUser, int organizationId)
        {
            if (sutureUser.IsUserSigningMember() && sutureUser.IsUserSigner() && string.IsNullOrEmpty(sutureUser.NPI))
            {
                return await this.ProviderService.CountProvidersByPreset(preset: PresetNetworkFilterType.MyNetwork,
                                                                         signerUserId: sutureUser.Id,
                                                                         senderFacilityId: null,
                                                                         targetPostalCode: null,
                                                                         distance: null,
                                                                         fromDate: DateTime.UtcNow.Subtract(TimeSpan.FromDays(365)));
            }
            else if (sutureUser.IsUserSigningMember())
            {
                return await this.ProviderService.CountProvidersByPreset(preset: PresetNetworkFilterType.MyNetwork,
                                                                         signerUserId: sutureUser.Id,
                                                                         senderFacilityId: null,
                                                                         targetPostalCode: null,
                                                                         distance: null,
                                                                         fromDate: DateTime.UtcNow.Subtract(TimeSpan.FromDays(365)));
            }
            else
            {
                return await this.ProviderService.CountProvidersByPreset(preset: PresetNetworkFilterType.MyNetwork,
                                                                         signerUserId: null,
                                                                         senderFacilityId: organizationId,
                                                                         targetPostalCode: null,
                                                                         distance: null,
                                                                         fromDate: DateTime.UtcNow.Subtract(TimeSpan.FromDays(365)));
            }
        }
    }
}
