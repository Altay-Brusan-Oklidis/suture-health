using Amazon.Runtime.Internal;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;


namespace SutureHealth.Application.Services.SqlServer
{
    public partial class SqlServerFeatureFlagsDbContext : FeatureFlagsDbContext
    {
        public SqlServerFeatureFlagsDbContext(DbContextOptions<SqlServerFeatureFlagsDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqlServerFeatureFlagsDbContext).Assembly);
            modelBuilder.ApplyApplicationBaseDbContextConfigurations();
        }
        public DbSet<FeatureFlag> FeatureFlags { get; set; }
        public DbSet<FeatureFlagAudit> FeatureFlagAudit { get; set; }
        public DbSet<FeatureFlagsUsers> FeatureFlagsUsers { get; set; }

        public override async Task<List<FeatureFlagDto>> GetFeatureFlagsByUserId(int loggedInUserId)
        {
            var featureFlagDtos = await FeatureFlags.Where(x => x.Active)
                .Select(featureFlag => new FeatureFlagDto
                {
                    Id = featureFlag.Id.ToString(),
                    Name = featureFlag.Name,
                    Enabled = !featureFlag.HasCohort || FeatureFlagsUsers.Any(userFlag =>
                        userFlag.FeatureFlagId == featureFlag.Id && userFlag.UserId == loggedInUserId)
                })
                .ToListAsync();
            return featureFlagDtos;
        }

        public override async Task<FeatureFlag> GetFeatureFlagsByFlagId(int featureFlagId)
            => await FeatureFlags.FirstOrDefaultAsync(ff => ff.Id == featureFlagId);        

        public override IQueryable<FeatureFlag> GetFeatureFlags()
         => FeatureFlags;


    }
}
