using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Application.Members;
using SutureHealth.Application.Organizations;

namespace SutureHealth.Application.Services.SqlServer
{
    public partial class SqlServerApplicationDbContext : IdentityDbContext
    {
        public override async Task<int> CreateOrganizationAsync(CreateOrganizationRequest request, int createdByMemberId)
        {
            int organizationId;

            request.Phone = request.Phone != null ? Regex.Replace(request.Phone, @"[^0-9]+", string.Empty) : null;

            using (var command = Database.GetDbConnection().CreateCommand() as SqlCommand)
            {
                command.CommandText = "dbo.CreateOrganization";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Name", request.Name);
                command.Parameters.AddWithValue("@OtherDesignation", request.OtherDesignation);
                command.Parameters.AddWithValue("@AddressLine1", request.AddressLine1);
                command.Parameters.AddWithValue("@AddressLine2", request.AddressLine2);
                command.Parameters.AddWithValue("@City", request.City);
                command.Parameters.AddWithValue("@StateOrProvince", request.StateOrProvince);
                command.Parameters.AddWithValue("@PostalCode", request.PostalCode);
                command.Parameters.AddWithValue("@Phone", request.Phone);
                command.Parameters.AddWithValue("@Fax", request.Fax);
                command.Parameters.AddWithValue("@NPI", request.Npi);
                command.Parameters.AddWithValue("@MedicareNumber", request.MedicareNumber);
                command.Parameters.AddWithValue("@CreatedByMemberId", createdByMemberId);
                command.Parameters.AddWithValue("@OrganizationTypeId", request.OrganizationTypeId.HasValue ? request.OrganizationTypeId.Value : (object)DBNull.Value);
                command.Parameters.AddWithValue("@CompanyId", request.CompanyId.HasValue ? request.CompanyId.Value : (object)DBNull.Value);

                await command.Connection.OpenAsync();
                organizationId = (int)await command.ExecuteScalarAsync();
                command.Connection.Close();
            }

            return organizationId;
        }

        public override async Task<bool> ToggleOrganizationActiveStatusAsync(int organizationId, int updatedByMemberId)
        {
            bool isActive;

            using (var command = Database.GetDbConnection().CreateCommand() as SqlCommand)
            {
                command.CommandText = "dbo.ToggleOrganizationActiveStatus";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@OrganizationId", organizationId);
                command.Parameters.AddWithValue("@UpdatedByMemberId", updatedByMemberId);

                await command.Connection.OpenAsync();
                isActive = (bool)await command.ExecuteScalarAsync();
                command.Connection.Close();
            }

            return isActive;
        }

        public override async Task UpdateOrganizationAsync(int organizationId, UpdateOrganizationRequest request, int updatedByMemberId)
        {
            request.Phone = request.Phone != null ? Regex.Replace(request.Phone, @"[^0-9]+", string.Empty) : null;

            using (var command = Database.GetDbConnection().CreateCommand() as SqlCommand)
            {
                command.CommandText = "dbo.UpdateOrganization";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@OrganizationId", organizationId);
                command.Parameters.AddWithValue("@Name", request.Name);
                command.Parameters.AddWithValue("@OtherDesignation", request.OtherDesignation);
                command.Parameters.AddWithValue("@AddressLine1", request.AddressLine1);
                command.Parameters.AddWithValue("@AddressLine2", request.AddressLine2);
                command.Parameters.AddWithValue("@City", request.City);
                command.Parameters.AddWithValue("@StateOrProvince", request.StateOrProvince);
                command.Parameters.AddWithValue("@PostalCode", request.PostalCode);
                command.Parameters.AddWithValue("@Phone", request.Phone);
                command.Parameters.AddWithValue("@Fax", request.Fax);
                command.Parameters.AddWithValue("@NPI", request.Npi);
                command.Parameters.AddWithValue("@MedicareNumber", request.MedicareNumber);
                command.Parameters.AddWithValue("@UpdatedByMemberId", updatedByMemberId);
                command.Parameters.AddWithValue("@OrganizationTypeId", request.OrganizationTypeId.HasValue ? request.OrganizationTypeId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@SetCompanyId", request.CompanyIdSpecified);
                command.Parameters.AddWithValue("@CompanyId", request.CompanyId.HasValue ? request.CompanyId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@SetDateClosed", request.ClosedAtSpecified);
                command.Parameters.AddWithValue("@DateClosed", request.ClosedAt.HasValue ? request.ClosedAt.Value : DBNull.Value);

                await command.Connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                command.Connection.Close();
            }
        }

        public override async Task<bool> IsSubscribedToInboxMarketingAsync(int organizationId) {
          bool isSubscribed;
          using var command = Database.GetDbConnection().CreateCommand() as SqlCommand;

          command.CommandText = "dbo.IsSubscribedToInboxMarketing";
          command.CommandType = CommandType.StoredProcedure;

          command.Parameters.AddWithValue("@OrganizationId", organizationId);

          await command.Connection.OpenAsync();
          isSubscribed = (bool) await command.ExecuteScalarAsync();
          command.Connection.Close();

          return isSubscribed;
        }

        public override async Task<bool> UpdateInboxMarketingSubscriptionAsync(int organizationId, bool active) {
          bool result;
          using var command = Database.GetDbConnection().CreateCommand() as SqlCommand;
          command.CommandText = "dbo.UpdateInboxMarketingSubscription";
          command.CommandType = CommandType.StoredProcedure;

          command.Parameters.AddWithValue("@OrganizationId", organizationId);
          command.Parameters.AddWithValue("@Active", active);

          await command.Connection.OpenAsync();
          
          result = (bool) await command.ExecuteScalarAsync();
          command.Connection.Close();

          return result;
        }
        
        public override async Task CreateOrganizationImage(OrganizationImage organizationImage)
        {
            await OrganizationImages.AddAsync(organizationImage);
            await SaveChangesAsync();
        }

        public override async Task DeleteOrganizationImages(OrganizationImage[] organizationImages)
        {
            OrganizationImages.RemoveRange(organizationImages);
            await SaveChangesAsync();
        }
    }
}
