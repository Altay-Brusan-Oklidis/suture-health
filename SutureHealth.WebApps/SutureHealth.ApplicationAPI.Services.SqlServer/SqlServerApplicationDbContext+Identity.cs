using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Application;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SutureHealth.Application.Services.SqlServer
{
    public partial class SqlServerApplicationDbContext : IdentityDbContext
    {
        public async Task<IdentityResult> AddOrUpdateMemberIdentityAsync
        (
            MemberIdentity member, 
            MemberIdentity updatedBy, 
            IEnumerable<int> relatedMembers, 
            IEnumerable<OrganizationMember> organizationMembers
        )
        {
            using var command = Database.GetDbConnection().CreateCommand() as SqlCommand;
            var relatedMembersTable = GetNewIntegerKeyDataTable();
            var organizationMembersTable = GetNewOrganizationMemberUpsertDataTable();

            command.CommandText = "dbo.UpsertMember";
            command.CommandType = CommandType.StoredProcedure;

            var entry = Entry(member);
            entry.DetectChanges();
            if (entry.State == EntityState.Added || entry.State == EntityState.Detached || entry.State == EntityState.Modified)
            {
                foreach (var prop in entry.Properties.Where(p => entry.State != EntityState.Modified || p.IsModified))
                {
                    switch (prop.Metadata.Name)
                    {
                        case nameof(MemberIdentity.AccessFailedCount):
                        case nameof(MemberIdentity.CanSign):
                        case nameof(MemberIdentity.Email):
                        case nameof(MemberIdentity.FirstName):
                        case nameof(MemberIdentity.LastName):
                        case nameof(MemberIdentity.LastLoggedInAt):
                        case nameof(MemberIdentity.LockoutEnd):
                        case nameof(MemberIdentity.MemberTypeId):
                        case nameof(MemberIdentity.MustChangePassword):
                        case nameof(MemberIdentity.MustRegisterAccount):
                        case nameof(MemberIdentity.NPI):
                        case nameof(MemberIdentity.ProfessionalSuffix):
                        case nameof(MemberIdentity.SecurityStamp):
                        case nameof(MemberIdentity.SigningName):
                        case nameof(MemberIdentity.Suffix):
                        case nameof(MemberIdentity.UserName):
                            command.Parameters.AddWithValue(prop.Metadata.Name, prop.CurrentValue == null ? DBNull.Value : prop.CurrentValue);
                            break;
                        case nameof(MemberIdentity.PasswordHash):
                            command.Parameters.AddWithValue(prop.Metadata.Name, prop.CurrentValue == null ? DBNull.Value : prop.CurrentValue);
                            if (prop.EntityEntry.State == EntityState.Modified && !prop.EntityEntry.Property(nameof(MemberIdentity.LockoutEnd)).IsModified)
                            {
                                command.Parameters.AddWithValue(nameof(MemberIdentity.LockoutEnd), DBNull.Value);
                            }
                            break;
                        case nameof(MemberIdentity.EmailConfirmed):
                            command.Parameters.AddWithValue(prop.Metadata.Name, prop.CurrentValue);
                            if (prop.EntityEntry.State == EntityState.Modified && !prop.EntityEntry.Property(nameof(MemberIdentity.Email)).IsModified)
                                command.Parameters.AddWithValue(nameof(MemberIdentity.Email), member.Email);
                            break;
                        case nameof(MemberIdentity.MobileNumberConfirmed):
                            command.Parameters.AddWithValue(prop.Metadata.Name, prop.CurrentValue);
                            if (prop.EntityEntry.State == EntityState.Modified && !prop.EntityEntry.Property(nameof(MemberIdentity.MobileNumber)).IsModified)
                                command.Parameters.AddWithValue("MobilePhone", member.MobileNumber.RemoveEverythingButNumbers());
                            break;
                        case nameof(MemberIdentity.MobileNumber):
                            command.Parameters.AddWithValue("MobilePhone", prop.CurrentValue.ToString().RemoveEverythingButNumbers());
                            break;
                        case nameof(MemberIdentity.OfficeNumber):
                            command.Parameters.AddWithValue("OfficePhone", prop.CurrentValue.ToString().RemoveEverythingButNumbers());
                            break;
                        case nameof(MemberIdentity.OfficeExtension):
                            command.Parameters.AddWithValue("OfficePhoneExtension", prop.CurrentValue);
                            break;
                        default:
                            break;
                    }
                }

                foreach (var id in relatedMembers ?? Array.Empty<int>())
                {
                    relatedMembersTable.Rows.Add(id);
                }

                foreach (var organizationMember in organizationMembers ?? Array.Empty<OrganizationMember>())
                {
                    organizationMembersTable.Rows.Add(organizationMember.OrganizationId,
                                                      organizationMember.IsAdministrator,
                                                      organizationMember.IsBillingAdministrator,
                                                      organizationMember.IsPrimary,
                                                      organizationMember.IsActive);
                }

                command.Parameters.Add(new SqlParameter("@RelatedMemberIds", relatedMembersTable)
                {
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[IntegerKey]"
                });
                command.Parameters.Add(new SqlParameter("@OrganizationMembers", organizationMembersTable)
                {
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[OrganizationMemberUpsert]"
                });

                var memberId = new SqlParameter()
                {
                    ParameterName = "MemberId",
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.InputOutput,
                    Value = entry.State == EntityState.Modified ? member.Id : DBNull.Value
                };

                command.Parameters.Add(memberId);
                command.Parameters.AddWithValue("UpdatedByMemberId", updatedBy.Id);

                await command.Connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                command.Connection.Close();

                member.MemberId = (int)memberId.Value;
                await entry.ReloadAsync();
            }

            return IdentityResult.Success;
        }

        public async override Task<IdentityResult> CreateMemberIdentityAsync(MemberIdentity member, MemberIdentity creator, IEnumerable<int> relatedMembers, IEnumerable<OrganizationMember> organizationMembers)
            => await AddOrUpdateMemberIdentityAsync(member, creator, relatedMembers, organizationMembers);

        public override async Task<PublicIdentity> CreatePublicIdentityAsync(MemberIdentity member, IdentityUseType identityType, DateTime? expirationDate, DateTime? effectiveDate)
        {
            var pidId = new SqlParameter("PublicIdentityId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var pidValue = new SqlParameter("PublicIdentity", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };

            effectiveDate ??= DateTime.UtcNow.Date;
            expirationDate ??= (identityType == IdentityUseType.OneTime ? DateTime.UtcNow.AddHours(18) : DateTime.UtcNow.AddYears(1));

            await Database.ExecuteSqlRawAsync(@"EXECUTE [dbo].[CreatePublicIdentity] @userId = {0}, @identityType = {1}, @expirationDate = {2}, @effectiveDate = {3}, @PublicIdentityId = {4} OUTPUT, @PublicIdentity = {5} OUTPUT",
                member.Id, identityType, expirationDate.Value, effectiveDate.Value, pidId, pidValue);

            return new PublicIdentity
            {
                Active = true,
                EffectiveDate = effectiveDate.Value,
                ExpirationDate = expirationDate.Value,
                MemberId = member.Id,
                PublicIdentityId = (int)pidId.Value,
                UseType = IdentityUseType.OneTime,
                Value = (Guid)pidValue.Value,
            };
        }

        public async override Task SetPublicIdentityActiveAsync(PublicIdentity identity, bool active)
            => await Database.ExecuteSqlInterpolatedAsync($@"EXECUTE [dbo].[SetPublicIdentityActive] @publicIdentityId = {identity.PublicIdentityId}, @active = {active}");

        public async override Task<IdentityResult> UpdateMemberIdentityAsync(MemberIdentity member, MemberIdentity updatedBy)
            => await AddOrUpdateMemberIdentityAsync(member, updatedBy, default, default);
    }
}