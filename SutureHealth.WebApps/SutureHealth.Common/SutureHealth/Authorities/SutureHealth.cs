using System;
using System.Collections.Generic;
using System.Linq;

namespace SutureHealth
{
    namespace Authorities
    {
        public static class SutureHealth
        {
            public static bool IsUniqueExternalIdentifier(this IIdentifier identifier) => identifier.Type.EqualsIgnoreCase(KnownTypes.UniqueExternalIdentifier);
            public static IIdentifier GetUniqueExternalIdentifierOrDefault(this IEnumerable<IIdentifier> identifiers) => identifiers.FirstOrDefault(i => i.Type.EqualsIgnoreCase(KnownTypes.UniqueExternalIdentifier));
        }
    }
}
