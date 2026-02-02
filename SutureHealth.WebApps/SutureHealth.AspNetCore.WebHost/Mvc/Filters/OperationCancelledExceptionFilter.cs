using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SutureHealth.AspNetCore.Filters;
public class OperationCancelledExceptionFilter : ExceptionFilterAttribute
{
    private readonly ILogger Logger;

    public OperationCancelledExceptionFilter(ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<OperationCancelledExceptionFilter>();
    }
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException)
        {                 
            Logger.LogDebug("Request({RequestPath}) was cancelled", context.HttpContext.Request.Path);
            context.ExceptionHandled = true;
            context.Result = new StatusCodeResult(400);
        }
    }
}