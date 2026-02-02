using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Authorities
{
    public static class InsuranceAndPayment
    {
        public static bool IsThereSelfPayIdentifier(this IEnumerable<IIdentifier> identifiers) =>
            identifiers.FirstOrDefault(i => i.Type.EqualsIgnoreCase(KnownTypes.SelfPay)) != null;
        public static bool IsTherePrivateInsuranceIdentifier(this IEnumerable<IIdentifier> identifiers) =>
            identifiers.FirstOrDefault(i => i.Type.EqualsIgnoreCase(KnownTypes.PrivateInsurance)) != null;
    }
}
