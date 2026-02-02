using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Application.Members;

namespace SutureHealth.Application.Services.SqlServer
{
    public partial class SqlServerApplicationDbContext : IdentityDbContext
    {
        public override async Task<int> UpsertMemberAsync(int? memberId, UpdateMemberRequest request, int updatedByMemberId)
        {
            request.OfficePhone = request.OfficePhone != null ? Regex.Replace(request.OfficePhone, @"[^0-9]+", string.Empty) : null;
            request.MobilePhone = request.MobilePhone != null ? Regex.Replace(request.MobilePhone, @"[^0-9]+", string.Empty) : null;

            using (var command = Database.GetDbConnection().CreateCommand() as SqlCommand)
            {
                var relatedMembersTable = GetNewIntegerKeyDataTable();
                var organizationMembersTable = GetNewOrganizationMemberUpsertDataTable();

                command.CommandText = "dbo.UpsertMember";
                command.CommandType = CommandType.StoredProcedure;

                foreach (var id in request.RelatedMemberIds ?? Array.Empty<int>())
                {
                    relatedMembersTable.Rows.Add(id);
                }
                foreach (var organizationMember in request.OrganizationMembers ?? Array.Empty<UpdateMemberRequest.OrganizationMember>())
                {
                    organizationMembersTable.Rows.Add(organizationMember.OrganizationId,
                                                      organizationMember.IsAdministrator,
                                                      organizationMember.IsBillingAdministrator,
                                                      organizationMember.IsPrimary,
                                                      organizationMember.IsActive);
                }

                command.Parameters.AddWithValue("@MemberId", memberId.HasValue ? memberId.Value : (object)DBNull.Value);
                command.Parameters.AddWithValue("@FirstName", request.FirstName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastName", request.LastName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Suffix", request.Suffix ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ProfessionalSuffix", request.ProfessionalSuffix ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Email", request.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MobilePhone", request.MobilePhone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@OfficePhone", request.OfficePhone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@OfficePhoneExtension", !string.IsNullOrWhiteSpace(request.OfficePhone) ? request.OfficePhoneExtension ?? string.Empty : (object)DBNull.Value);
                command.Parameters.AddWithValue("@UserName", request.UserName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MemberTypeId", request.MemberTypeId.HasValue ? request.MemberTypeId.Value : (object)DBNull.Value);
                command.Parameters.AddWithValue("@Npi", request.Npi ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SigningName", request.SigningName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CanSign", request.CanSign.HasValue ? request.CanSign.Value : (object)DBNull.Value);
                command.Parameters.AddWithValue("@UpdatedByMemberId", updatedByMemberId);

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

                await command.Connection.OpenAsync();
                memberId = (int)(await command.ExecuteScalarAsync());
                command.Connection.Close();
            }

            return memberId.Value;
        }

        public override async Task<bool> ToggleMemberActiveStatusAsync(int memberId)
        {
            bool isActive;

            using (var command = Database.GetDbConnection().CreateCommand() as SqlCommand)
            {
                command.CommandText = "dbo.ToggleMemberActiveStatus";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@MemberId", memberId);

                await command.Connection.OpenAsync();
                isActive = (bool)await command.ExecuteScalarAsync();
                command.Connection.Close();
            }

            return isActive;
        }

        protected static DataTable GetNewIntegerKeyDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            return table;
        }

        protected static DataTable GetNewOrganizationMemberUpsertDataTable()
        {
            var table = new DataTable();

            table.Columns.Add("OrganizationId", typeof(int));
            table.Columns.Add("IsAdministrator", typeof(bool));
            table.Columns.Add("IsBillingAdministrator", typeof(bool));
            table.Columns.Add("IsPrimary", typeof(bool));
            table.Columns.Add("IsActive", typeof(bool));

            return table;
        }

        public override async Task CreateMemberImage(MemberImage memberImage)
        {
            await MemberImages.AddAsync(memberImage);
            await SaveChangesAsync();
        }

        public override async Task CreateMemberImages(MemberImage[] memberImages)
        {
            await MemberImages.AddRangeAsync(memberImages);
            await SaveChangesAsync();
        }

        public override async Task DeleteMemberImages(MemberImage[] memberImages)
        {
            MemberImages.RemoveRange(memberImages);
            await SaveChangesAsync();
        }
    }
}
