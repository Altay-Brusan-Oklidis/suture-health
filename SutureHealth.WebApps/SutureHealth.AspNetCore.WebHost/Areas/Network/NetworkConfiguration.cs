using SutureHealth.AspNetCore.Areas.Network.Models.Listing;
using SutureHealth.Providers;

namespace SutureHealth.AspNetCore.Areas.Network
{
    public static class NetworkConfiguration
    {
        public const string NETWORK_PAGE_USER_KEY = "NetworkLastAccessDate";
        public const string TARGETED_ROLLOUT_ALLOWED_STATES_KEY = "AllowedInviteSenderState";
        public const int PROVIDER_SERVICE_RADIUS_CAP = 256;
        public const int RECENTLY_JOINED_DAYS_PAST = 90;
        public const int LISTING_PAGE_SIZE = 25;

        /*
        FilterSpecialties = new Dictionary<string, ProviderEntityMapping>()
        {
            { "0", new ProviderEntityMapping() { Name = "Cardiology" } },
            { "1", new ProviderEntityMapping() { Name = "Internal Medicine" } },
            { "2", new ProviderEntityMapping() { Name = "Nephrology" } },
            { "3", new ProviderEntityMapping() { Name = "Oncology" } }
        };
        */

        public static IReadOnlyDictionary<string, ProviderEntityMapping> FilterAccountTypes { get; } = new Dictionary<string, ProviderEntityMapping>()
        {
            { "Community", new ProviderEntityMapping() { Name = "Community (Limited Access)", Mapping = pe => pe.SutureCustomerType == SutureCustomerType.Community } },
            { "Enterprise", new ProviderEntityMapping() { Name = "Enterprise (Full Access)", Mapping = pe => pe.SutureCustomerType == SutureCustomerType.Enterprise } },
            { "Extended", new ProviderEntityMapping() { Name = "Extended (Non-member)", Mapping = pe => pe.SutureCustomerType == SutureCustomerType.NonMember } }
        };

        public static IReadOnlyDictionary<string, ProviderEntityMapping> FilterOrganizations { get; } = new Dictionary<string, ProviderEntityMapping>()
        {
            { "AssistedLivingFacility", new ProviderEntityMapping() { Name = "Assisted Living Facility", Mapping = pe => pe.ServiceTypes.Any(st => new int[] { 110, 111, 112 }.Contains(st.ServiceId)) } },
            { "HomeHealth", new ProviderEntityMapping() { Name = "Home Health", Mapping = pe => pe.ServiceTypes.Any(st => new int[] { 113, 122, 202, 101, 114 }.Contains(st.ServiceId)) } },
            { "Hospice", new ProviderEntityMapping() { Name = "Hospice", Mapping = pe => pe.ServiceTypes.Any(st => new int[] { 206, 123, 102, 118 }.Contains(st.ServiceId)) } },
            { "Hospital", new ProviderEntityMapping() { Name = "Hospital", Mapping = pe => pe.ServiceTypes.Any(st => st.ServiceId == 208) } },
            { "MedicalEquipment", new ProviderEntityMapping() { Name = "Medical Equipment", Mapping = pe => pe.ServiceTypes.Any(st => new int[] {106, 108, 205, 103, 104, 105, 107 }.Contains(st.ServiceId)) } },
            { "MedicalPractice", new ProviderEntityMapping() { Name = "Medical Practice", Mapping = pe => pe.ServiceTypes.Any(st => st.ServiceId == 203) } },
            { "NursingHome", new ProviderEntityMapping() { Name = "Nursing Home", Mapping = pe => pe.ServiceTypes.Any(st => new [] { 115, 116, 117 }.Contains(st.ServiceId)) } },
            { "SkilledNursingFacility", new ProviderEntityMapping() { Name = "Skilled Nursing Facility", Mapping = pe => pe.ServiceTypes.Any(st => st.ServiceId == 109) } }
        };

        public static IReadOnlyDictionary<string, ProviderEntityMapping> FilterClinicians { get; } = new Dictionary<string, ProviderEntityMapping>()
        {
            { "NursePractitioner", new ProviderEntityMapping() { Name = "Nurse Practitioner", Mapping = pe => pe.SutureUserTypeId == 2001 } },
            { "Physician", new ProviderEntityMapping() { Name = "Physician", Mapping = pe => pe.SutureUserTypeId == 2000 } },
            { "PhysicianAssistant", new ProviderEntityMapping() { Name = "Physician Assistant", Mapping = pe => new [] { 2002, 2008, 2012, 2014, 2015 }.Contains(pe.SutureUserTypeId.Value) } },
        };
    }
}
