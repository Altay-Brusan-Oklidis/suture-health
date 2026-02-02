using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Areas.Network.Models;
using SutureHealth.Application.Services;
using SutureHealth.Providers.Services;
using SutureHealth.AspNetCore.Mvc.Attributes;
using Microsoft.AspNetCore.Cors;
using SutureHealth.AspNetCore.WebHost;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Network.Controllers
{
    [Area("Network")]
    [Route("Network/Badge")]
    [IgnoreLoginHandler]
    public class BadgeController : Controller
    {
        IApplicationService SecurityService { get; }
        INetworkServicesProvider ProviderService { get; }

        public BadgeController
        (
            IApplicationService securityService,
            INetworkServicesProvider providerService
        )
        {
            SecurityService = securityService;
            ProviderService = providerService;
        }

        [EnableCors(Startup.CorsLegacyPolicy)]
        [HttpGet("", Name = "NetworkBadge")]
        [IgnoreAntiforgeryToken]
        [IgnoreLoginHandler]
        public async Task<IActionResult> Index()
        {
            if (CurrentUser.IsApplicationAdministrator())
            {
                return Json(new BadgeJsonModel() { NetworkNavigationEnabled = false, CallToActionEnabled = false });
            }

            long count = 0;
            var networkEnabled = false;
            var organization = await SecurityService.GetOrganizationByIdAsync(CurrentUser.PrimaryOrganizationId);
            if (organization != null && (await SecurityService.IsNetworkEnabledAsync(organization)))
            {
                var lastInvocationDate = await SecurityService.InvokeUserLastAccessDateAsync(CurrentUser.Id, true);
                networkEnabled = true;
                count = await this.ProviderService.CountProvidersByPreset(null,
                                                                          CurrentUser.IsUserSigningMember() ? CurrentUser.Id : null,
                                                                          CurrentUser.IsUserSender() ? organization.OrganizationId : null,
                                                                          organization.PostalCode,
                                                                          CurrentUser.IsUserSigningMember() ? 50 : 100,
                                                                          lastInvocationDate.DateTime);
            }

            return Json(new BadgeJsonModel()
            {
                RecentlyJoinedCount = count,
                NetworkUrl = Url.RouteUrl("NetworkPreset", new { preset = count > 0 ? FilterPreset.RecentlyJoined.ToString() : FilterPreset.NearMe.ToString() }),
                NetworkNavigationEnabled = networkEnabled,
                CallToActionEnabled = !CurrentUser.IsPayingClient && CurrentUser.IsUserSigningMember() && networkEnabled,
            });
        }
    }
}
