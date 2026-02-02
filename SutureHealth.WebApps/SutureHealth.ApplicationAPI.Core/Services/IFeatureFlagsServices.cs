using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Application.Services;

public interface IFeatureFlagsServices
{
    Task<List<FeatureFlagDto>> GetFeatureFlagsByUserId(int loggedInUserId);
    Task<FeatureFlag> GetFeatureFlagByFlagId(int featureFlagId);
    IQueryable<FeatureFlag> GetFeatureFlags();
    Task<bool> IsFeatureEnabledForUser(string featureName, int loggedInUserId);
    Task UpdateFeatureFlag(FeatureFlag featureFlag, int memberId);
    Task UpdateFeatureFlags(FeatureFlag[] featureFlags, int memberId);    
}
