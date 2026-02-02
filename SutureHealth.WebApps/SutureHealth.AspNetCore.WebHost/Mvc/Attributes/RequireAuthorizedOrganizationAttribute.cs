using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace SutureHealth.AspNetCore.Mvc.Attributes
{
    public class RequireAuthorizedOrganizationAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.Request.RouteValues.TryGetValue("organizationId", out var organizationIdRouteValue) && int.TryParse((string)organizationIdRouteValue, out var organizationId))
            {
                var securityService = context.HttpContext.RequestServices.GetRequiredService<IApplicationService>();
                var userManager = context.HttpContext.RequestServices.GetRequiredService<SutureUserManager>();
                var authorizedUser = await userManager.GetUserAsync(context.HttpContext.User);
                var organization = null as Organization;

                if (authorizedUser.IsApplicationAdministrator() || await securityService.IsMemberSurrogateSenderAsync(authorizedUser))
                {
                    organization = await securityService.GetOrganizationByIdAsync(organizationId);
                }
                else
                {
                    var authorizedOrg = await securityService.GetOrganizationMembersByMemberId(authorizedUser.Id)
                                                             .Include(om => om.Organization)
                                                             .Where(om => om.IsActive && organizationId == om.OrganizationId)
                                                             .FirstOrDefaultAsync();
                    if (authorizedOrg != null)
                    {
                        organization = authorizedOrg.Organization;
                    }
                }

                if (organization != null)
                {
                    context.ActionArguments["organization"] = organization;

                    await base.OnActionExecutionAsync(context, next);
                    return;
                }

                context.Result = new StatusCodeResult(400);
                return;
            }

            context.Result = new StatusCodeResult(500);
        }
    }
}
