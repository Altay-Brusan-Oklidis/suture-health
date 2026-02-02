using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SutureHealth.AspNetCore.Areas.Identity.Models;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace SutureHealth.AspNetCore.Areas.Identity.Components
{
    public class OrganizationSelector : ViewComponent
    {
        protected IApplicationService SecurityService { get; }
        protected SutureUserManager UserManager { get; }

        public OrganizationSelector
        (
            IApplicationService securityService,
            SutureUserManager userManager
        )
        {
            SecurityService = securityService;
            UserManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string routeName, bool contentOnly = false)
        {
            var organizationId = this.Request.RouteValues.TryGetValue("organizationId", out var organization) ? int.Parse(organization.ToString()) : (int?)null;
            var sutureUser = await UserManager.GetUserAsync(UserClaimsPrincipal);
            var user = await SecurityService.GetMemberByIdAsync(sutureUser.Id);
            var organizationMembers = await SecurityService.GetOrganizationMembersByMemberId(sutureUser.Id).ToArrayAsync();

            return View(new OrganizationSelectorViewModel()
            {
                Organizations = organizationMembers.Where(om => om.IsActive)
                                                   .OrderBy(om => om.Organization.Name)
                                                   .ThenBy(om => om.Organization.OtherDesignation)
                                                   .Select(om => new SelectListItem(om.Organization.Name + (!string.IsNullOrWhiteSpace(om.Organization.OtherDesignation) ? $" ({om.Organization.OtherDesignation})" : string.Empty),
                                                                                                                  Url.RouteUrl(routeName, new { organizationId = om.OrganizationId, contentOnly }),
                                                                                                                  om.OrganizationId == organizationId)),
            });
        }
    }
}
