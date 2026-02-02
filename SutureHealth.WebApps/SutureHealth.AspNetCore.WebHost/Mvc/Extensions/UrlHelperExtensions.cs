using SutureHealth.Application;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string DefaultLandingPage(this IUrlHelper helper, MemberIdentity sutureUser)
        {
            if (sutureUser != null)
            {
                if (sutureUser.IsApplicationAdministrator())
                {
                    return helper.Page("/Dashboard", new { area = "Admin" });
                }
                else if (sutureUser.IsUserSigningMember())
                {
                    return helper.Content("~/request/sign");
                }
                else if (sutureUser.IsUserSender())
                {
                    return helper.Content("~/request/send");
                }
            }

            return helper.Page("/Account/Login", new { area = "Identity" });
        }
    }
}
