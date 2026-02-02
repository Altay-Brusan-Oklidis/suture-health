using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Asp.Versioning;
using Humanizer;
using SutureHealth.Application.v0100.Models;
using SutureHealth.AspNetCore.Identity;
using ControllerBase = SutureHealth.AspNetCore.Mvc.ControllerBase;

namespace SutureHealth.Application.v0100.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.AuthorizedUser)]
[Route("auth")]
[ControllerName("ApiAuthentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration Configuration;
    private readonly IWebHostEnvironment Environment;
    private readonly ILogger<AuthenticationController> Logger;
    private readonly SutureSignInManager SignInManager;
    private readonly UserManager<MemberIdentity> UserManager;

    public AuthenticationController
    (
        IConfiguration configuration,
        IWebHostEnvironment environment,
        SignInManager<MemberIdentity> signInManager,
        UserManager<MemberIdentity> userManager,
        ILogger<AuthenticationController> logger
    )
    {
        Configuration = configuration;
        Environment = environment;
        Logger = logger;
        SignInManager = signInManager as SutureSignInManager;
        UserManager = userManager;
    }

    /// <summary>
    /// Log in using a username and password.
    /// </summary>
    /// <returns>The Security Credentials used for future requests.</returns>
    /// <response code="200">Successful login.</response>
    /// <response code="400">Message containing the reason for the failure.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [Produces("application/json")]
    public async Task<IActionResult> Login(LoginInputModel input)
    {
        if (ModelState.IsValid)
        {
            var result = await SignInManager.PasswordSignInAsync(input.Username, input.Password, input.RememberMe, lockoutOnFailure: true);
            if (result is SutureSignInResult success && result.Succeeded)
            {
                Logger.LogInformation("User logged in.");
                return Ok(new { Token = SignInManager.GenerateJwtToken(success.SutureUser) });
            }
            //else if (result.RequiresTwoFactor)
            //{
            //    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
            //}
            else if (result.IsLockedOut)
            {
                Logger.LogWarning("User account locked out.");
                if (result is SutureSignInResult lockedResult)
                {
                    ModelState.AddModelError(string.Empty, $"Your account is locked for {(DateTimeOffset.UtcNow - lockedResult.SutureUser.LockoutEnd.Value).Humanize()}.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Your account is locked out.");
                }
            }
            else if (result is SutureSignInResult failure && !string.IsNullOrWhiteSpace(failure.Message))
            {
                ModelState.AddModelError(string.Empty, failure.Message);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
        }

        return BadRequest(ModelState);
    }
}