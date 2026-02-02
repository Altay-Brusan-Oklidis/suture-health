using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SutureHealth.AspNetCore.Identity;
using System.Security.Claims;

namespace SutureHealth.AspNetCore.Services;

public static class ClaimsPrincipalExtensions
{
    public static async Task LogOutAsync(this ClaimsPrincipal user, IOptionsMonitor<Saml2Configuration> optionsMonitor, SutureSignInManager signInManager)
    {
        // Firstly, try to sign out from the SAML2 if the user logged in with SSO.
        var idP = user.Claims.FirstOrDefault(c => c.Type == ".IdentityProvider")?.Value;
        var options = idP.IsNullOrEmpty() ? null : optionsMonitor.Get(idP);
        if (options != null)
        {
            _ = await signInManager.Saml2SignOutAsync(options);
        }

        // Then, do a normal sign out.
        await signInManager.SignOutAsync();
    }
}
