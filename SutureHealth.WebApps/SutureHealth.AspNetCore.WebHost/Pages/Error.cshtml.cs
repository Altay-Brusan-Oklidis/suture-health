using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Models;
using System.Diagnostics;

namespace SutureHealth.AspNetCore.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    [AllowAnonymous]
    public class ErrorModel : BasePageModel
    {
        public string ErrorMessage { get; set; }
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public bool ShowDetailedError { get; set; } = false;

        private readonly ILogger<ErrorModel> Logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            Logger = logger;
        }

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error is ApplicationUserException applicationException)
            {
                ErrorMessage = applicationException.Message;
                ModelState.Merge(applicationException.ModelState);
                ShowDetailedError = true;
            }
        }
    }
}