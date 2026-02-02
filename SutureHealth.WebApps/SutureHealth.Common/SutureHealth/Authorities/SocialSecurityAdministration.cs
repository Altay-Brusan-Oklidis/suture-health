using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SutureHealth.Authorities
{
    public static class SocialSecurityAdministration
    {
        public static bool IsSocialSecurityIdentifier(this IIdentifier identifier) => 
            identifier.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber) || identifier.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial);

        public static IIdentifier GetSocialSecurityNumberOrDefault(this IEnumerable<IIdentifier> identifiers) => identifiers.FirstOrDefault(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber));
        public static IIdentifier GetSocialSecuritySerialOrDefault(this IEnumerable<IIdentifier> identifiers) => identifiers.FirstOrDefault(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial));

        public static string SanitizeSocialSecurityNumber(string socialSecurityNumber) => Regex.Replace(socialSecurityNumber ?? string.Empty, @"\D+", string.Empty);
    }
}
