using Microsoft.AspNetCore.Mvc.Filters;

namespace SutureHealth.AspNetCore.Mvc.Filters
{
    public class AddHeaderAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("Help", 
                $"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}{context.HttpContext.Request.PathBase}/swagger/index.html");
            base.OnResultExecuting(context);
        }
    }
}
