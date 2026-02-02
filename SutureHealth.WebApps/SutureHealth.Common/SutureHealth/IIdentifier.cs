using System.Collections.Generic;

namespace SutureHealth
{
    public static class KnownTypes
    {
        public const string MedicareBeneficiaryNumber = "mbi";
        public const string MedicaidNumber = "medicaid-number";
        public const string MedicaidState = "medicaid-state";
        public const string NationalProviderIdentifier = "npi";
        public const string SocialSecurityNumber = "ssn";
        public const string SocialSecuritySerial = "ssn4";
        public const string UniqueExternalIdentifier = "external-unique-identifier";
        public const string UniqueSutureIdentifier = "suture-unique-identifier";

        // Boolean identifiers
        public const string Medicare = "has-medicare";
        public const string MedicareAdvantage = "has-medicare-advantage";
        public const string Medicaid = "has-medicaid";
        public const string PrivateInsurance = "has-private-insurance";
        public const string SelfPay = "has-self-pay";
    }

    public interface IIdentifier
    {
        string Type { get; set; }
        string Value { get; set; }
    }

    public static class IIdentifierExtensions
    {
        public static void Add<T>(this ICollection<T> identifiers, string type, string value) where T : IIdentifier, new()
        {
            identifiers.Add(new T
            {
                Type = type,
                Value = value
            });
        }

        public static T Clone<T>(this IIdentifier identifier) where T : IIdentifier, new()
        {
            var clone = new T 
            { 
                Type = identifier.Type,
                Value = identifier.Value
            };
            return clone;
        }
    }
}
