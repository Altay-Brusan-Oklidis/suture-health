using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SutureHealth.Documents.Services.SqlServer
{
    public partial class SqlServerDocumentDbContext : DocumentDbContext
    {
        public SqlServerDocumentDbContext(DbContextOptions<SqlServerDocumentDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            OnModelCreating(modelBuilder.Entity<TemplateConfiguration>());
            OnModelCreating(modelBuilder.Entity<AnnotationOCRMapping>());
            OnModelCreating(modelBuilder.Entity<OCRDocument>());
            OnModelCreating(modelBuilder.Entity<FacilityTemplateConfiguration>());
            OnModelCreating(modelBuilder.Entity<TemplateType>());
            OnModelCreating(modelBuilder.Entity<Template>());
            OnModelCreating(modelBuilder.Entity<TemplateAnnotation>());

            base.OnModelCreating(modelBuilder);
        }

        public override async Task CreateAnnotations(int destinationTemplateId, DataTable annotations, bool activateTemplate = false)
        {
            await this.Database.ExecuteSqlRawAsync("EXECUTE [dbo].[CreateAnnotations] @DestinationTemplateId, 1, @ActivateTemplate, @Annotations", new SqlParameter[]
            {
                new SqlParameter("@DestinationTemplateId", destinationTemplateId),
                new SqlParameter("@ActivateTemplate", activateTemplate),
                new SqlParameter("@Annotations", annotations)
                {
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[TemplateAnnotation]"
                }
            });
        }

        public override async Task CreateAnnotationsFromParentTemplateAsync(int destinationTemplateId)
        {
            await this.Database.ExecuteSqlRawAsync("EXECUTE [dbo].[CreateAnnotationsFromParentTemplate] @DestinationTemplateId, 1, NULL", new SqlParameter[]
            {
                new SqlParameter("@DestinationTemplateId", destinationTemplateId)
            });
        }

        public override async Task<int> CreateRequestTemplateAsync(int parentTemplateId, int senderOrganizationId, int senderMemberId, string storageKey)
        {
            var templateIdParameter = new SqlParameter("@TemplateId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            await Database.ExecuteSqlInterpolatedAsync($@"EXECUTE [dbo].[CreateRequestTemplate]  @ParentTemplateId = {parentTemplateId}, 
                                                                                                 @SubmitterOrganizationId = {senderOrganizationId},
                                                                                                 @SubmitterMemberId = {senderMemberId}, 
                                                                                                 @DataS3Key = {storageKey}, 
                                                                                                 @TemplateId = {templateIdParameter} OUT");
            return (int)templateIdParameter.Value;
        }

        public override async Task<IEnumerable<Template>> GetActiveTemplatesByOrganizationIdAsync(int organizationId)
        {
            var templates = await Templates.FromSqlInterpolated($"dbo.SelectActiveTemplatesByOrganizationId @OrganizationId = {organizationId}").ToArrayAsync();
            var templateIds = templates.Select(t => t.TemplateId);

            return await Templates.Include(t => t.TemplateType)
                                  .Where(t => templateIds.Any(id => t.TemplateId == id))
                                  .ToArrayAsync();
        }

        public override async Task<int> GetParentTemplateIdFromTemplateId(int templateId)
        {
            int parentTemplateId;
            var sqlConnection = this.Database.GetDbConnection();
            var isInitiallyClosed = sqlConnection.State == ConnectionState.Closed;

            if (isInitiallyClosed)
            {
                await sqlConnection.OpenAsync();
            }

            using (var sqlCommand = sqlConnection.CreateCommand() as SqlCommand)
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "dbo.SelectParentTemplateByTemplateId";
                sqlCommand.Parameters.AddWithValue("@TemplateId", templateId);

                parentTemplateId = Convert.ToInt32(await sqlCommand.ExecuteScalarAsync());
            }

            if (isInitiallyClosed)
            {
                sqlConnection.Close();
            }

            return parentTemplateId;
        }

        public override async Task<IEnumerable<FacilityTemplateConfiguration>> GetParentTemplatesByFacilityIdAsync(int facilityId) =>
            await FacilityTemplateConfigurations.FromSqlInterpolated($@"dbo.SelectParentTemplatesByFacilityId {facilityId}")
                                                .AsNoTracking()
                                                .ToArrayAsync();

        public override async Task<IEnumerable<TemplateType>> GetPdfTemplateTypesByOrganizationIdAsync(int organizationId)
            => await TemplateTypes.FromSqlInterpolated($@"dbo.SelectPdfTemplateTypesByOrganizationId {organizationId}")
                                  .AsNoTracking()
                                  .ToArrayAsync();

        public override async Task<int> CreateOrganizationTemplateAsync(int organizationId, string name, int templateTypeId, string storageKey, int memberId)
        {
            using (var connection = this.Database.GetDbConnection())
            using (var command = connection.CreateCommand() as SqlCommand)
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.CreateOrganizationTemplate";

                command.Parameters.AddWithValue("@OrganizationId", organizationId);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@TemplateTypeId", templateTypeId);
                command.Parameters.AddWithValue("@StorageKey", storageKey);
                command.Parameters.AddWithValue("@MemberId", memberId);

                await connection.OpenAsync();
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }

        public override async Task<Template> GetTemplateByRequestIdAsync(int requestId)
            => await Templates.FromSqlInterpolated($@"
SELECT t.* 
  FROM dbo.ServiceableRequest sr
 INNER JOIN dbo.Template t ON sr.TemplateId = t.TemplateId
 WHERE sr.[SutureSignRequestID] = {requestId}").AsNoTracking()
                              .Include(t => t.TemplateType)
                              .Include(t => t.Annotations)
                              .FirstOrDefaultAsync();
       
    }
}

