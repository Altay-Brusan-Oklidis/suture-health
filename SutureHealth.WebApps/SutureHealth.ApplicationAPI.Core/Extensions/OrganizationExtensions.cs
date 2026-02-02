using System.Linq;

namespace SutureHealth.Application
{
    public static class OrganizationExtensions
    {
        public static bool IsValidForSendingRequests(this Organization organization)
            => !string.IsNullOrWhiteSpace(organization.NPI) &&
                    !string.IsNullOrWhiteSpace(organization.Contacts?.FirstOrDefault(c => c.Type == ContactType.Phone)?.Value) &&
                    !string.IsNullOrWhiteSpace(organization.Contacts?.FirstOrDefault(c => c.Type == ContactType.Fax)?.Value) &&
                    !string.IsNullOrWhiteSpace(organization.AddressLine1) && !string.IsNullOrWhiteSpace(organization.City) &&
                    !string.IsNullOrWhiteSpace(organization.StateOrProvince) && !string.IsNullOrWhiteSpace(organization.PostalCode);
    }
}
