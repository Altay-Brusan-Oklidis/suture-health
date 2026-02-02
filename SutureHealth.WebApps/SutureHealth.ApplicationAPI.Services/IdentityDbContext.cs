using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SutureHealth.Application.Services
{
    public abstract class IdentityDbContext : ApplicationDbContext
    {
        public DbSet<MemberIdentity> MemberIdentities { get; set; }
        public DbSet<MemberHash> MemberHashes { get; set; }
        public DbSet<MemberToken> MemberTokens { get; set; }
        public DbSet<PublicIdentity> PublicIdentities { get; set; }
        public DbSet<MemberAuditEvent> MemberAuditEvents { get; set; }

        protected IdentityDbContext(DbContextOptions options) : base(options)
        { }

        public abstract Task<IdentityResult> CreateMemberIdentityAsync(MemberIdentity member, MemberIdentity creator, IEnumerable<int> relatedMembers, IEnumerable<OrganizationMember> organizationMembers);
        public abstract Task<PublicIdentity> CreatePublicIdentityAsync(MemberIdentity member, IdentityUseType identityType, DateTime? expirationDate, DateTime? effectiveDate);
        public abstract Task SetPublicIdentityActiveAsync(PublicIdentity identity, bool active);
        public abstract Task<IdentityResult> UpdateMemberIdentityAsync(MemberIdentity member, MemberIdentity updatedBy);
    }
}
