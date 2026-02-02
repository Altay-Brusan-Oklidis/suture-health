using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Application;
using SutureHealth.Application.Services;

namespace SutureHealth.Application.Services
{
    public abstract class FeatureFlagsDbContext : DbContext
    {
        protected FeatureFlagsDbContext(DbContextOptions options) : base(options)
        { }
        public abstract Task<List<FeatureFlagDto>> GetFeatureFlagsByUserId(int loggedInUserId);
        public abstract Task<FeatureFlag> GetFeatureFlagsByFlagId(int featureFlagId);
        
        public abstract IQueryable<FeatureFlag> GetFeatureFlags();
    }
}
