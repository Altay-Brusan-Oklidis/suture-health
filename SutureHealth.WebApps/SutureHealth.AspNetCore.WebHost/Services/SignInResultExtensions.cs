using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SutureHealth.AspNetCore.Identity;

namespace SutureHealth.AspNetCore.Services;

public static class SignInResultExtensions
{
    public static IActionResult ToActionResult(this Microsoft.AspNetCore.Identity.SignInResult result, ModelStateDictionary modelState, ILogger logger, IUrlHelper urlHelper, IActionResult onFailure, string returnUrl = null)
    {
        if (result is SutureSignInResult success && result.Succeeded)
        {
            logger.LogInformation("User logged in.");

            if (success.SutureUser.MemberType == Application.MemberType.ApplicationAdmin)
                returnUrl = null;

            return new RedirectResult(returnUrl ?? urlHelper.DefaultLandingPage(success.SutureUser));
        }
        else if (result.IsLockedOut)
        {
            logger.LogWarning("User account locked out.");
            if (result is SutureSignInResult lockedResult)
            {
                modelState.AddModelError(string.Empty, $"Your account is locked for {(DateTimeOffset.UtcNow - lockedResult.SutureUser.LockoutEnd.Value).Humanize()}.");
            }
            else
            {
                modelState.AddModelError(string.Empty, "Your account is locked out.");
            }
        }
        else if (result is SutureSignInResult failure && !string.IsNullOrWhiteSpace(failure.Message))
        {
            modelState.AddModelError(string.Empty, failure.Message);
        }
        else
        {
            modelState.AddModelError(string.Empty, "Invalid Login Attempt");
        }

        return onFailure;
    }
}
