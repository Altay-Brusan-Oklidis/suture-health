using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using SutureHealth.Application.Services;
using System.Security.Claims;
using SutureHealth.Application;
using System.Threading.Tasks;
using System.Net;

namespace SutureHealth.AspNetCore.SwaggerGen.Attributes
{
    public class FeatureFlagAttribute : ActionFilterAttribute
    {
        private readonly string _featureFlagName;
        private readonly HttpStatusCode _statusCode;

        public FeatureFlagAttribute(string featureFlagName, HttpStatusCode statusCode = HttpStatusCode.Forbidden)
        {
            _featureFlagName = featureFlagName;
            _statusCode = statusCode;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var featureFlagsService = (IFeatureFlagsServices)context.HttpContext.RequestServices.GetService(typeof(IFeatureFlagsServices));

            if (featureFlagsService != null && !await featureFlagsService.IsFeatureEnabledForUser(_featureFlagName, Int32.Parse(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value)))
            {
                var errorMessage = $"Feature flag '{_featureFlagName}' not enabled for this user";
                context.Result = new ObjectResult(errorMessage)
                {
                    StatusCode = (int)_statusCode,
                };
            }
            await base.OnActionExecutionAsync(context, next);
        }
    }

}
