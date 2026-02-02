using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SutureHealth.Application.EntityFrameworkCore.Members;

namespace SutureHealth.Application.Services.SqlServer
{
    public partial class SqlServerApplicationDbContext : IdentityDbContext
    {
        protected string EnvironmentName { get; }

        public SqlServerApplicationDbContext
        (
            DbContextOptions<SqlServerApplicationDbContext> options,
            IHostEnvironment environment = null
        ) : base(options)
        {
			EnvironmentName = environment?.EnvironmentName;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqlServerApplicationDbContext).Assembly, type => type != typeof(MemberAuditEventConfigurator))
						.ApplyConfiguration(new MemberAuditEventConfigurator(EnvironmentName));

        public override HierarchicalSetting GetHierarchicalSetting(string key, int memberId, int? organizationId = null)
            => Set<HierarchicalSetting>().FromSqlRaw(@"EXEC [dbo].[SelectHierarchicalSetting] @MemberId, @OrganizationId, @Key",
                new SqlParameter("@Key", key),
                new SqlParameter("@MemberId", memberId),
                new SqlParameter("@OrganizationId", organizationId.HasValue ? organizationId.Value : DBNull.Value))
            .AsEnumerable()
            .FirstOrDefault();
    }
}
