namespace SutureHealth
{
    public interface IAddress
    {
        string Line1 { get; set; }
        string Line2 { get; set; }
        string City { get; set; }
        string County { get; set; }
        string StateOrProvince { get; set; }
        string CountryOrRegion { get; set; }
        string PostalCode { get; set; }
    }

    public static class AddressExtensions
    {
        public static T Clone<T>(this IAddress address) where T : IAddress, new()
        {
            var clone = new T
            {
                Line1 = address.Line1,
                Line2 = address.Line2,
                City = address.City,
                CountryOrRegion = address.CountryOrRegion,
                County = address.County,
                PostalCode = address.PostalCode,
                StateOrProvince = address.StateOrProvince
            };
            return clone;
        }
    }
}
