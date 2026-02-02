using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SutureHealth.Diagnostics;
using SutureHealth.Storage;

namespace SutureHealth.Application.Services
{
    public class IdentityServiceProvider : ApplicationServices<IdentityDbContext>, IIdentityService
    {
        private IHttpContextAccessor HttpContextAccessor { get; init; }

        public IdentityServiceProvider
        (
            IdentityDbContext authorizationContext,
            IHttpContextAccessor httpContextAccessor,
            ILogger<IIdentityService> logger,
            ITracingService tracer,
            IStorageService storageService,
            IConfiguration configurationService
        ) : base(authorizationContext, logger, tracer, storageService, configurationService)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        async Task<IdentityResult> IIdentityService.CreateAsync(MemberIdentity member, MemberIdentity creator, IEnumerable<int> relatedMembers, IEnumerable<OrganizationMember> organizationMembers)
            => await ApplicationContext.CreateMemberIdentityAsync(member, creator, relatedMembers, organizationMembers);

        async Task<PublicIdentity> IIdentityService.CreatePublicIdentityAsync(MemberIdentity member, IdentityUseType identityType, DateTime? expirationDate, DateTime? effectiveDate)
            => await ApplicationContext.CreatePublicIdentityAsync(member, identityType, expirationDate, effectiveDate);

        async Task<MemberIdentity> IIdentityService.GetMemberIdentityByIdAsync(int memberID)
            => await ApplicationContext.MemberIdentities.Where(m => m.MemberId == memberID).SingleOrDefaultAsync();

        async Task<MemberIdentity> IIdentityService.GetMemberIdentityByEmailAddressAsync(string emailAddress)
            => await ApplicationContext.MemberIdentities.Where(m => m.Email == emailAddress).SingleOrDefaultAsync();

        async Task<MemberIdentity> IIdentityService.GetMemberIdentityByNameAsync(string userName)
            => await ApplicationContext.MemberIdentities.Where(m => m.UserName.ToUpper() == userName.ToUpper()).SingleOrDefaultAsync();

        async Task<MemberIdentity> IIdentityService.GetMemberIdentityByPublicIdAsync(Guid publicId, bool includeExpired)
        {
            var query = from pi in ApplicationContext.PublicIdentities
                        where pi.Active && pi.EffectiveDate < DateTime.UtcNow && pi.Value == publicId
                        select pi;

            if (!includeExpired)
            {
                query = query.Where(pi => pi.ExpirationDate > DateTime.UtcNow);
            }

            return await query.Join(ApplicationContext.MemberIdentities, pi => pi.MemberId, mi => mi.MemberId, (pi, mi) => new { PublicIdentity = pi, MemberIdentity = mi })
                              .OrderByDescending(join => join.PublicIdentity.PublicIdentityId)
                              .Select(join => join.MemberIdentity)
                              .FirstOrDefaultAsync();
        }

        IQueryable<MemberIdentity> IIdentityService.GetMemberIdentities()
            => ApplicationContext.MemberIdentities.AsNoTracking();

        IQueryable<MemberHash> IIdentityService.GetHashes(MemberIdentity member, string hashProvider, string name)
            => ApplicationContext.MemberHashes.Where(h => h.HashProvider == hashProvider && h.Name == name && h.MemberId == member.Id);

        IQueryable<MemberToken> IIdentityService.GetTokens(MemberIdentity member, string loginProvider, string name)
            => ApplicationContext.MemberTokens.Where(mt => mt.MemberId == member.Id && mt.TokenProvider == loginProvider && mt.Name == name);

        IQueryable<MemberAuditEvent> IIdentityService.GetAuditEvents(int memberId)
            => ApplicationContext.MemberAuditEvents.AsNoTracking()
                                                   .Where(e => e.MemberId == memberId);

        async Task<PublicIdentity> IIdentityService.GetPublicIdentityByValueAsync(System.Guid publicId)
            => await ApplicationContext.PublicIdentities.FirstOrDefaultAsync(i => i.Value == publicId);

        async Task IIdentityService.LogAuditEventAsync(MemberIdentity member, AuditEvents eventType, string eventDescription, bool success)
        {
            await ApplicationContext.AddAsync(new MemberAuditEvent
            {
                MemberId = member.Id,
                AuditEventId = eventType,
                AuditEventName = eventDescription ?? eventType.GetEnumDescription(),
                AuditDate = DateTime.UtcNow,
                Succeeded = success,
                IpAddress = HttpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString()
            });
            await ApplicationContext.SaveChangesAsync();
        }

        async Task IIdentityService.RemoveTokenAsync(MemberIdentity member, string loginProvider, string name)
        {
            ApplicationContext.RemoveRange(ApplicationContext.MemberTokens.Where(mt => mt.MemberId == member.Id && mt.TokenProvider == loginProvider && mt.Name == name));
            await ApplicationContext.SaveChangesAsync();
        }

        async Task IIdentityService.SetHashAsync(MemberIdentity member, string hashProvider, string name, string value)
        {
            await ApplicationContext.MemberHashes.AddAsync(new MemberHash
            {
                CreatedAt = DateTime.Now,
                HashProvider = hashProvider,
                MemberId = member.Id,
                Name = name,
                Value = value
            });
            await ApplicationContext.SaveChangesAsync();
        }

        async Task IIdentityService.SetPublicIdentityActiveAsync(PublicIdentity identity, bool active)
            => await ApplicationContext.SetPublicIdentityActiveAsync(identity, active);

        async Task IIdentityService.SetTokenAsync(MemberIdentity member, string loginProvider, string name, string value)
        {
            var memberToken = ApplicationContext.MemberTokens.FirstOrDefault(mt => mt.MemberId == member.Id && mt.TokenProvider == loginProvider && mt.Name == name);
            if (memberToken != null)
            {
                memberToken.Value = value;
            }
            else
            {
                ApplicationContext.MemberTokens.Add(new MemberToken
                {
                    MemberId = member.Id,
                    Name = name,
                    TokenProvider = loginProvider,
                    Value = value
                });
            }

            await ApplicationContext.SaveChangesAsync();
        }

        async Task<IdentityResult> IIdentityService.UpdateAsync(MemberIdentity member, MemberIdentity updatedBy)
            => await ApplicationContext.UpdateMemberIdentityAsync(member, updatedBy);
    }
}
