using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.Identity;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Controllers
{
    [AllowAnonymous]
    [Route("")]
    public class HomeController : Controller
    {
        protected IIdentityService IdentityServices { get; set; }
        protected ILogger<HomeController> Logger { get; }
        protected IApplicationService SecurityService { get; }
        protected SutureSignInManager SignInManager { get; }

        public HomeController
        (
            IIdentityService identityServices,
            IApplicationService securityService,
            SignInManager<MemberIdentity> signInManager,
            ILogger<HomeController> logger
        )
        {
            IdentityServices = identityServices;
            Logger = logger;
            SecurityService = securityService;
            SignInManager = signInManager as SutureSignInManager;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return Redirect(Url.DefaultLandingPage(CurrentUser));
        }

        [HttpGet("/SutureSignMvc/register")]
        public async Task<IActionResult> DefaultRoute
        (
            [FromQuery(Name = "pid")] Guid? publicId = null,
            [FromQuery] string userName = null,
            [FromQuery] bool forgotPassword = false,
            [FromQuery] string returnUrl = null
        )
        {
            var mustChangePassword = false;
            var mustRegisterAccount = false;
            if (CurrentUser != null)
            {
                mustChangePassword = CurrentUser.MustChangePassword;
                mustRegisterAccount = CurrentUser.MustRegisterAccount;
            }

            if (publicId.HasValue && publicId.Value != Guid.Empty)
            {
                await SignInManager.SignOutAsync();
                var member = await IdentityServices.GetMemberIdentityByPublicIdAsync(publicId.Value);
                if (member == null)
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
                else
                {
                    var publicIdentity = await IdentityServices.GetPublicIdentityByValueAsync(publicId.Value);
                    if (publicIdentity != null && publicIdentity.UseType == IdentityUseType.OneTime)
                    {
                        return RedirectToRoute("ChangePasswordHandler", new
                        {
                            userName = member.UserName,
                            publicId,
                            returnUrl
                        });
                    }
                    else if (publicIdentity != null && publicIdentity.UseType == IdentityUseType.PublicLogin)
                    {
                        return RedirectToRoute("MemberRegistration", new
                        {
                            publicId = publicId.Value,
                            userName = member.UserName,
                            returnUrl
                        });
                    }
                    else
                    { 
                        if (member.MustRegisterAccount)
                        {
                            return RedirectToRoute("MemberRegistration", new
                            {
                                publicId = publicId.Value,
                                userName = member.UserName,
                                returnUrl
                            });
                        }
                        else if(member.MustChangePassword)
                        {
                            return RedirectToRoute("ChangePasswordHandler", new
                            {
                                userName = member.UserName,
                                publicId,
                                returnUrl
                            });
                        }
                        else
                        {
                            return RedirectToPage("/Account/Login", new
                            {
                                area = "Identity",
                                publicId = publicId.Value,
                                userName = member.UserName,
                                returnUrl
                            });
                        }
                    }
                }
            }
            else if (mustRegisterAccount && CurrentUser != null)
            {
                return RedirectToRoute("PublicRegistration", new { 
                    publicId = publicId.Value
                });
            }
            else if (mustChangePassword && CurrentUser != null)
            {
                return RedirectToRoute("ChangePasswordHandler", new
                {
                    publicId = publicId.Value
                });
            }

            return Redirect(Url.DefaultLandingPage(CurrentUser));
        }
    }
}
