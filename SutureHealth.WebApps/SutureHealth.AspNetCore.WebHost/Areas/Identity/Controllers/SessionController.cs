using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SutureHealth.AspNetCore.Mvc.Attributes;
using SutureHealth.AspNetCore.WebHost;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class SessionController : Controller
    {
        [EnableCors(Startup.CorsLegacyPolicy)]
        [ChangePasswordHandler]
        [MemberRegistrationHandler]
        [HttpPost("Ping", Name = "SessionPing")]
        public async Task<JsonResult> SessionPingAsync([FromServices] IConfiguration configuration)
        {
            var cookieName = configuration["SutureHealth:Authentication:Cookie:Name"] ?? "SutureHealth.AuthenticationCookie";
            var cookieExpiration = DateTime.TryParse(Request.Cookies[$"{cookieName}.SessionManager.Expiration"], out var expiration) ? 
                    expiration.ToUniversalTime() : 
                    DateTime.UtcNow.Add(TimeSpan.TryParse(configuration["SutureHealth:Authentication:Cookie:Timeout"], out var timespan) ? timespan : TimeSpan.FromHours(18));

            return Json(new { timeout = (cookieExpiration - DateTime.UtcNow).TotalMilliseconds });
        }
    }
}
