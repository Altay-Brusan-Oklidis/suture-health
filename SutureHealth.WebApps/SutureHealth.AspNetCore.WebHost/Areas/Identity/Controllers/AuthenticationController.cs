using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using System.Security.Claims;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.AspNetCore.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace SutureHealth.AspNetCore.Areas.Identity.Controllers
{
    [AllowAnonymous]
    [Area("Identity")]
    [Route("Authentication")]
    public class AuthenticationController : Controller
    {
        private readonly IOptionsMonitor<Saml2Configuration> _saml2OptionsMonitor;
        private readonly SutureUserManager _userManager;
        private readonly SutureSignInManager _signInManager;
        private readonly ILogger<AuthenticationController> _logger;
        protected readonly IApplicationService ApplicationService;


        public AuthenticationController(
            IOptionsMonitor<Saml2Configuration> saml2OptionsMonitor,
            SutureUserManager userManager,
            SutureSignInManager signInManager,
            IApplicationService applicationService,
            ILogger<AuthenticationController> logger)
        {
            _saml2OptionsMonitor = saml2OptionsMonitor;
            ApplicationService = applicationService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [TempData]
        public string TempErrorMessage { get; set; }

        [HttpGet("Login/{provider}", Name = "SsoLogin")]
        public Task<IActionResult> LoginExternal([FromRoute]string provider, [FromQuery]string returnUrl)
        {
            var binding = new Saml2RedirectBinding();
            const string relayStateReturnUrl = "ReturnUrl";
            _ = binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });
            var result = binding.Bind(new Saml2AuthnRequest(_saml2OptionsMonitor.Get(provider))).ToActionResult();
            return Task.FromResult(result);
        }

        [HttpPost("AssertionConsumerService/{provider}", Name = "AssertionConsumerService")]
        public async Task<IActionResult> AssertionConsumerService([FromRoute] string provider)
        {
            var binding = new Saml2PostBinding();
            var saml2AuthnResponse = new Saml2AuthnResponse(_saml2OptionsMonitor.Get(provider));

            _ = binding.ReadSamlResponse(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                _logger.LogWarning($"SAML Response status: {saml2AuthnResponse.Status}");
                TempErrorMessage = "Invalid Login Attempt";
                return SutureSignInResult.Failed(null).ToActionResult(ModelState, _logger, Url, RedirectToPage("/Account/Login", new { area = "Identity", }));
            }

            _ = binding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnResponse);
            var principal = await saml2AuthnResponse.CreateSession(HttpContext, claimsTransform: (claimsPrincipal) => Transform(claimsPrincipal, provider));
            var member = await _userManager.GetUserWithSamlClaimsAsync(principal);
            var relayStateQuery = binding.GetRelayStateQuery();
            const string relayStateReturnUrl = "ReturnUrl";
            _ = relayStateQuery.TryGetValue(relayStateReturnUrl, out var returnUrl);
            if (member == null)
            {
                await HttpContext.SignOutAsync(Saml2Constants.AuthenticationScheme);
                _logger.LogWarning($"SAML login attempt with non matching member.");
                TempErrorMessage = "Invalid Login Attempt";
                return SutureSignInResult.Failed(null).ToActionResult(ModelState, _logger, Url, RedirectToPage("/Account/Login", new { area = "Identity", returnUrl }), returnUrl);
            }

            // Member is already signed-in becase CreateSession is called above.
            // Calling SignInAsync to set the SessionManager.Expiration cookie.
            var signInResult = await _signInManager.SignInAsync(member, _ => Task.CompletedTask);
            if (!(signInResult is SutureSignInResult && signInResult.Succeeded))
            {
                await HttpContext.SignOutAsync(Saml2Constants.AuthenticationScheme);
            }

            var externalUser = "isExternalUser";
            var isExternalUser =  await ApplicationService.GetMemberSettings(member.MemberId)
                                           .Where(s => s.Key == externalUser && s.IsActive == true)
                                           .FirstOrDefaultAsync();

            if (isExternalUser == null || isExternalUser.ItemBool != true)
            {
                await ApplicationService.AddMemberSettingAsync(member.MemberId, externalUser, true, null, null, ItemType.Boolean);
            }


            return signInResult.ToActionResult(ModelState, _logger, Url, RedirectToPage("/Account/Login", new { area = "Identity", returnUrl }), returnUrl);
        }

        [HttpPost("LoggedOut/{provider}", Name = "LoggedOut")]
        public IActionResult LoggedOut([FromRoute] string provider)
        {
            var binding = new Saml2PostBinding();
            binding.Unbind(Request.ToGenericHttpRequest(), new Saml2LogoutResponse(_saml2OptionsMonitor.Get(provider)));

            return Redirect(Url.Content("~/"));
        }

        [Route("SingleLogout/{provider}", Name = "SingleLogout")]
        public async Task<IActionResult> SingleLogout([FromRoute] string provider)
        {
            Saml2StatusCodes status;
            var requestBinding = new Saml2PostBinding();
            var config = _saml2OptionsMonitor.Get(provider);
            var logoutRequest = new Saml2LogoutRequest(config, User);
            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), logoutRequest);
                status = Saml2StatusCodes.Success;
                await logoutRequest.DeleteSession(HttpContext);
            }
            catch (Exception exc)
            {
                // log exception
                _logger.LogError("SingleLogout error: " + exc.ToString());
                status = Saml2StatusCodes.RequestDenied;
            }

            var responsebinding = new Saml2PostBinding();
            responsebinding.RelayState = requestBinding.RelayState;
            var saml2LogoutResponse = new Saml2LogoutResponse(config)
            {
                InResponseToAsString = logoutRequest.IdAsString,
                Status = status,
            };
            return responsebinding.Bind(saml2LogoutResponse).ToActionResult();
        }

        private ClaimsPrincipal Transform(ClaimsPrincipal incomingPrincipal, string provider)
        {
            var claims = new List<Claim>();
            claims.AddRange(incomingPrincipal.Claims);
            claims.Add(new Claim(".IdentityProvider", provider));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, incomingPrincipal.Identity.AuthenticationType, ClaimTypes.NameIdentifier, ClaimTypes.Role)
            {
                BootstrapContext = ((ClaimsIdentity)incomingPrincipal.Identity).BootstrapContext
            });
        }
    }
}