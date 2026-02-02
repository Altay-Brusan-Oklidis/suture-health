using System;
using System.Collections.Generic;
using System.Linq;

namespace SutureHealth.Authorities
{
    public static class CentersForMedicareMedicaidServices
    {
        public static class MedicaidAuthorities
        {
            public const string Alabama = "us.alabama";
            public const string Alaska = "us.alaska";
            public const string Arizona = "us.arizona";
            public const string Arkansas = "us.arkansas";
            public const string California = "us.california";
            public const string Colorado = "us.colorado";
            public const string Connecticut = "us.connecticut";
            public const string Delaware = "us.delaware";
            public const string Florida = "us.florida";
            public const string Georgia = "us.georgia";
            public const string Hawaii = "us.hawaii";
            public const string Idaho = "us.idaho";
            public const string Illinois = "us.illinois";
            public const string Indiana = "us.indiana";
            public const string Iowa = "us.iowa";
            public const string Kansas = "us.kansas";
            public const string Kentucky = "us.kentucky";
            public const string Louisiana = "us.louisiana";
            public const string Maine = "us.maine";
            public const string Maryland = "us.maryland";
            public const string Massachusetts = "us.massachusetts";
            public const string Michigan = "us.michigan";
            public const string Minnesota = "us.minnesota";
            public const string Mississippi = "us.mississippi";
            public const string Missouri = "us.missouri";
            public const string Montana = "us.montana";
            public const string Nebraska = "us.nebraska";
            public const string Nevada = "us.nevada";
            public const string NewHampshire = "us.newhampshire";
            public const string NewJersey = "us.newjersey";
            public const string NewMexico = "us.newmexico";
            public const string NewYork = "us.newyork";
            public const string NorthCarolina = "us.northcarolina";
            public const string NorthDakota = "us.northdakota";
            public const string Ohio = "us.ohio";
            public const string Oklahoma = "us.oklahoma";
            public const string Oregon = "us.oregon";
            public const string Pennsylvania = "us.pennsylvania";
            public const string RhodeIsland = "us.rhodeisland";
            public const string SouthCarolina = "us.southcarolina";
            public const string SouthDakota = "us.southdakota";
            public const string Tennessee = "us.tennessee";
            public const string Texas = "us.texas";
            public const string Utah = "us.utah";
            public const string Vermont = "us.vermont";
            public const string Virginia = "us.virginia";
            public const string Washington = "us.washington";
            public const string WestVirginia = "us.westvirginia";
            public const string Wisconsin = "us.wisconsin";
            public const string Wyoming = "us.wyoming";
        }

        public static IIdentifier GetMedicareMedicareBeneficiaryNumberOrDefault(this IEnumerable<IIdentifier> identifiers) =>
            identifiers.FirstOrDefault(i =>
                i.Type.EqualsIgnoreCase(KnownTypes.MedicareBeneficiaryNumber)
            );
        public static IIdentifier GetMedicaidNumberOrDefault(this IEnumerable<IIdentifier> identifiers) =>
            identifiers.FirstOrDefault(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicaidNumber));

        public static IIdentifier GetMedicaidStateOrDefault(this IEnumerable<IIdentifier> identifiers) =>
            identifiers.FirstOrDefault(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicaidState));

        public static bool IsThereMedicareAdvantage(this IEnumerable<IIdentifier> identifiers) =>
            identifiers.FirstOrDefault(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicareAdvantage)) != null;
        public static bool IsNPIType(this IIdentifier identifier) =>
            identifier.Type.EqualsIgnoreCase(KnownTypes.NationalProviderIdentifier);

        public static bool IsMedicareBeneficiaryNumber(this IIdentifier identifier) =>
            identifier.Type.EqualsIgnoreCase(KnownTypes.MedicareBeneficiaryNumber);

        public static bool IsMedicaidType(this IIdentifier identifier) =>
            identifier.Type.EqualsAnyIgnoreCase(new string[] { KnownTypes.Medicaid, KnownTypes.MedicaidNumber, KnownTypes.MedicaidState });
    }
}
