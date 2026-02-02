using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SutureHealth.Application.Services
{
    public interface IIdentityService : IApplicationService
    {
        Task<IdentityResult> CreateAsync(MemberIdentity member, MemberIdentity creator, IEnumerable<int> relatedMembers, IEnumerable<OrganizationMember> organizationMembers);
        Task<PublicIdentity> CreatePublicIdentityAsync(MemberIdentity member, IdentityUseType identityType, DateTime? expirationDate = null, DateTime? effectiveDate = null);

        Task<MemberIdentity> GetMemberIdentityByIdAsync(int memberID);
        Task<MemberIdentity> GetMemberIdentityByEmailAddressAsync(string emailAddress);
        Task<MemberIdentity> GetMemberIdentityByNameAsync(string userName);
        Task<MemberIdentity> GetMemberIdentityByPublicIdAsync(Guid publicId, bool includeExpired = false);
        Task<PublicIdentity> GetPublicIdentityByValueAsync(Guid publicId);
        
        IQueryable<MemberHash> GetHashes(MemberIdentity member, string hashProvider, string name);
        IQueryable<MemberIdentity> GetMemberIdentities();
        IQueryable<MemberToken> GetTokens(MemberIdentity member, string loginProvider, string name);
        IQueryable<MemberAuditEvent> GetAuditEvents(int memberId);

        Task LogAuditEventAsync(MemberIdentity member, AuditEvents eventType, string eventDescription = null, bool success = true);
        Task RemoveTokenAsync(MemberIdentity member, string loginProvider, string name);
        
        Task SetHashAsync(MemberIdentity member, string hashProvider, string name, string value);
        Task SetPublicIdentityActiveAsync(PublicIdentity identity, bool active);
        Task SetTokenAsync(MemberIdentity member, string loginProvider, string name, string value);

        Task<IdentityResult> UpdateAsync(MemberIdentity member, MemberIdentity updatedBy);
    }
}
