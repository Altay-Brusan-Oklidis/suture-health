using ITfoxtec.Identity.Saml2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.AspNetCore.Models;
using SutureHealth.AspNetCore.Services;

namespace SutureHealth.AspNetCore.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [MemberRegistrationHandler]
    public class LogoutModel : BasePageModel
    {
        private readonly SutureSignInManager _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IOptionsMonitor<Saml2Configuration> _saml2OptionsMonitor;

        public LogoutModel(
            SutureSignInManager signInManager,
            ILogger<LogoutModel> logger,
            IOptionsMonitor<Saml2Configuration> saml2OptionsMonitor)
        {
            _signInManager = signInManager;
            _logger = logger;
            _saml2OptionsMonitor = saml2OptionsMonitor;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            _logger.LogInformation("User logged out.");

            await User.LogOutAsync(_saml2OptionsMonitor, _signInManager);

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }

            return LocalRedirect("/");
        }
    }
}
