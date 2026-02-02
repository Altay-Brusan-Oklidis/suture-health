using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.Application.Services;
using SutureHealth.Application;

namespace SutureHealth.AspNetCore.Mvc.Attributes;

public class RequireMemberAttribute : ActionFilterAttribute
{
    public RequireMemberAttribute()
    {
        this.Order = 100;
    }

    public bool RequireAuthorizedAdministrator { get; set; } = false;
    public bool RequireAuthenticatedMember { get; set; } = false;

    protected bool RequireAuthorization => RequireAuthorizedAdministrator || RequireAuthenticatedMember;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        StatusCodeResult result = null;
        if 
        (
            context.HttpContext.Request.RouteValues.TryGetValue("memberId", out var memberIdRouteValue) && 
            int.TryParse((string)memberIdRouteValue, out var memberId) &&
            context.Controller is Controller baseController
        )
        {
            var applicationService = context.HttpContext.RequestServices.GetRequiredService<IApplicationService>();
            var userManager = context.HttpContext.RequestServices.GetRequiredService<SutureUserManager>();
            var currentUser = baseController.CurrentUser;

            MemberBase member = null;
            if (context.ActionArguments.TryGetValue("member", out object memberArg))
            {
                member = memberArg switch
                {
                    Member => await applicationService.GetMemberByIdAsync(memberId),
                    MemberIdentity => await userManager.FindByIdAsync(memberId.ToString()),
                    _ => null
                };
            }

            if (member != null)
            {
                context.ActionArguments["member"] = member;
                if (RequireAuthorization)
                {
                    if (!currentUser.IsApplicationAdministrator())
                    {
                        if (RequireAuthorizedAdministrator)
                        {
                            //var adminOrgs = await (from o in applicationService.GetOrganizationMembers(true)
                            //                       join mo in applicationService.GetOrganizationMembersByMemberId(memberId) on o.OrganizationId equals mo.OrganizationId
                            //                       select o).Distinct()
                            //                                .ToArrayAsync();

                            var organizationMembers = applicationService.GetOrganizationMembersByMemberId(member.MemberId);
                            var associatedMembers = applicationService.GetOrganizationMembersByOrganizationId(organizationMembers.Select(om => om.OrganizationId).Distinct().ToArray());
                            var adminOrgs = associatedMembers.Where(om => om.MemberId == currentUser.Id && om.IsAdministrator).Select(om => om.OrganizationId);
                            var memberOrgs = associatedMembers.Where(om => om.MemberId == member.MemberId).Select(om => om.OrganizationId);

                            if (!adminOrgs.Join(memberOrgs, aom => aom, mom => mom, (aom, mom) => aom).Any())
                                result = new StatusCodeResult(401);
                        }
                    }

                    if (RequireAuthenticatedMember && member.MemberId != currentUser.Id)
                    {
                        result = new StatusCodeResult(401);
                    }
                }
            }
            else
            {
                result = new StatusCodeResult(404);
            }
        }

        if (result == null)
            await base.OnActionExecutionAsync(context, next);
        else
            context.Result = result;
    }
}
