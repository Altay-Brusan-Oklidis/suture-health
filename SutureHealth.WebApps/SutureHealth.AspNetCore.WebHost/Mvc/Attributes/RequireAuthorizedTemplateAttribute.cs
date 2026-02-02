using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.Documents.Services;
using SutureHealth.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace SutureHealth.AspNetCore.Mvc.Attributes
{
    public class RequireAuthorizedTemplateAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.Request.RouteValues.TryGetValue("templateId", out var templateIdRouteValue) && int.TryParse((string)templateIdRouteValue, out var templateId))
            {
                var securityService = context.HttpContext.RequestServices.GetRequiredService<IApplicationService>();
                var documentService = context.HttpContext.RequestServices.GetRequiredService<IDocumentServicesProvider>();
                var userManager = context.HttpContext.RequestServices.GetRequiredService<SutureUserManager>();
                var authorizedUser = await userManager.GetUserAsync(context.HttpContext.User);
                var template = await documentService.GetTemplateByIdAsync(templateId);
                var isAuthorized = true;

                if (template == null)
                {
                    context.Result = new StatusCodeResult(404);
                    return;
                }

                if (!(authorizedUser.IsApplicationAdministrator() || await securityService.IsMemberSurrogateSenderAsync(authorizedUser.Id)))
                {
                    isAuthorized = await securityService.GetOrganizationMembersByMemberId(authorizedUser.Id)
                                                        .AnyAsync(om => om.IsActive && om.Organization.IsActive && om.OrganizationId == template.OrganizationId);
                }

                if (isAuthorized)
                {
                    context.ActionArguments["template"] = template;
                    await base.OnActionExecutionAsync(context, next);
                    return;
                }

                context.Result = new StatusCodeResult(401);
                return;
            }

            context.Result = new StatusCodeResult(500);
        }
    }
}
