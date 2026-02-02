using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Application.Members;
using SutureHealth.Application.Organizations;

namespace SutureHealth.Application.Services
{
    public abstract class ApplicationDbContext : ApplicationBaseDbContext
    {
        protected ApplicationDbContext(DbContextOptions options) : base(options)
        { }

        public abstract Task<int> CreateOrganizationAsync(CreateOrganizationRequest request, int createdByMemberId);
        public abstract Task<IEnumerable<OrganizationMember>> GetSigningOrganizationMembersAsync(string searchText, string organizationStateOrProvinceFilter, int count);
        public abstract Task<bool> ToggleOrganizationActiveStatusAsync(int organizationId, int updatedByMemberId);
        public abstract Task<bool> ToggleMemberActiveStatusAsync(int memberId);
        public abstract Task UpdateOrganizationAsync(int organizationId, UpdateOrganizationRequest request, int updatedByMemberId);
        public abstract Task<int> UpsertMemberAsync(int? memberId, UpdateMemberRequest request, int updatedByMemberId);
        public abstract HierarchicalSetting GetHierarchicalSetting(string key, int memberId, int? organizationId = null);
        public abstract Task CreateMemberImage(MemberImage memberImage);
        public abstract Task CreateMemberImages(MemberImage[] memberImages);
        public abstract Task DeleteMemberImages(MemberImage[] memberImages);
        public abstract Task CreateOrganizationImage(OrganizationImage organizationImage);
        public abstract Task DeleteOrganizationImages(OrganizationImage[] memberImages);
        public abstract Task<bool> IsSubscribedToInboxMarketingAsync(int organizationId);
        public abstract Task<bool> UpdateInboxMarketingSubscriptionAsync(int organizationId, bool active);
    }
}
