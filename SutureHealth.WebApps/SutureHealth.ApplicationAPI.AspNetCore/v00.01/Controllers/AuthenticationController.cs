using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Asp.Versioning;
using ControllerBase = SutureHealth.AspNetCore.Mvc.ControllerBase;

namespace SutureHealth.Application.v0001.Controllers
{
    [ApiController]
    [ApiVersion("0.1")]
    [ApiVersion("1.0")]
    [Route("auth")]
    [ControllerName("ApiAuthentication")]
    public class AuthenticationController : ControllerBase
    {
        protected IConfiguration Configuration { get; }

        public AuthenticationController
        (
            IConfiguration configuration
        )
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Log in using a username and password for OAuth authentication
        /// </summary>
        /// <returns></returns>
        [HttpGet("login")]
        public async Task<IActionResult> Login()
        {
            return Redirect($@"https://{Configuration["SutureHealth:Authentication:AuthBaseHost"]}/login?client_id={Configuration["SutureHealth:Authentication:ClientId"]}&response_type=code&scope=openid+profile&redirect_uri={Configuration["SutureHealth:Authentication:CallbackUrl"]}");
        }

        /// <summary>
        /// Exchanges an OAuth authorization code for refresh and access tokens
        /// </summary>
        /// <returns></returns>
        [HttpGet("token")]
        public async Task<IActionResult> Token([FromQuery] string code)
        {
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response;

                httpClient.DefaultRequestHeaders.Authorization = GetOAuthAuthenticationHeader();

                response = await httpClient.PostAsync($"https://{Configuration["SutureHealth:Authentication:AuthBaseHost"]}/oauth2/token", new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "grant_type", "authorization_code" },
                    { "redirect_uri", Configuration["SutureHealth:Authentication:CallbackUrl"] },
                    { "code", code }
                }));

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest();
                }

                return FilterOAuthResult(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Returns a new OAuth access token given a valid refresh token
        /// </summary>
        /// <returns></returns>
        [HttpGet("refreshtoken")]
        public async Task<IActionResult> RefreshToken([FromQuery] string refresh_token)
        {
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response;

                httpClient.DefaultRequestHeaders.Authorization = GetOAuthAuthenticationHeader();

                response = await httpClient.PostAsync($"https://{Configuration["SutureHealth:Authentication:AuthBaseHost"]}/oauth2/token", new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refresh_token }
                }));

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest();
                }

                return FilterOAuthResult(await response.Content.ReadAsStringAsync());
            }
        }

        [HttpGet("policy/{policyName?}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UserPolicy([FromServices] Microsoft.AspNetCore.Authorization.IAuthorizationService authService, string policyName = null)
        {
            IEnumerable<string> policies;

            if (string.IsNullOrWhiteSpace(policyName))
            {
                policies = typeof(AuthorizationPolicies).GetFields(BindingFlags.Static | BindingFlags.Public)
                                 .Where(x => x.IsLiteral && !x.IsInitOnly)
                                 .Select(x => x.GetValue(null)).Cast<string>();
            }
            else
            {
                policies = new string[] { policyName };
            }

            return Ok((await Task.WhenAll(policies.Select(async p =>
            {
                try
                {
                    return new KeyValuePair<string, bool?>(p, (await authService.AuthorizeAsync(this.User, null, p)).Succeeded);
                }
                catch
                {
                    return new KeyValuePair<string, bool?>(p, null);
                }
            }))).ToDictionary(p => p.Key, p => p.Value));
        }

        protected JsonResult FilterOAuthResult(string contentBody)
        {
            var oAuthResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(contentBody);

            oAuthResponse.Remove("id_token");

            return new JsonResult(oAuthResponse);
        }

        protected AuthenticationHeaderValue GetOAuthAuthenticationHeader()
        {
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($@"{Configuration["SutureHealth:Authentication:ClientId"]}:{Configuration["SutureHealth:Authentication:ClientSecret"]}")));
        }
    }
}
