using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Application.Services
{
    public class FeatureFlagsServices : IFeatureFlagsServices
    {
        protected FeatureFlagsDbContext FeatureFlagsDbContext { get; }

        public FeatureFlagsServices(FeatureFlagsDbContext featureFlagsDbContext)
        {
            FeatureFlagsDbContext = featureFlagsDbContext;
        }

        public async Task<List<FeatureFlagDto>> GetFeatureFlagsByUserId(int loggedInUserId)
        {
            var featureFlags = await FeatureFlagsDbContext.GetFeatureFlagsByUserId(loggedInUserId);
            return featureFlags;
        }

        public async Task<FeatureFlag> GetFeatureFlagByFlagId(int featureFlagId)
            => await FeatureFlagsDbContext.GetFeatureFlagsByFlagId(featureFlagId);

        public IQueryable<FeatureFlag> GetFeatureFlags()
        {
            return FeatureFlagsDbContext.GetFeatureFlags();
        }

        public async Task<bool> IsFeatureEnabledForUser(string featureName, int loggedInUserId)
        {
            var featureFlags = await GetFeatureFlagsByUserId(loggedInUserId);
            bool isfeatureEnabled = featureFlags.Any(flag => flag.Name == featureName && flag.Enabled);
            return isfeatureEnabled;
        }       

        public async Task UpdateFeatureFlag(FeatureFlag featureFlag, int memberId)
        {
            featureFlag.UpdatedBy = memberId;
            FeatureFlagsDbContext.Update(featureFlag);
            await FeatureFlagsDbContext.SaveChangesAsync();
        }

        public async Task UpdateFeatureFlags(FeatureFlag[] featureFlags, int memberId)
        {            
            FeatureFlagsDbContext.UpdateRange(featureFlags);
            await FeatureFlagsDbContext.SaveChangesAsync();
        }        
    }


}
