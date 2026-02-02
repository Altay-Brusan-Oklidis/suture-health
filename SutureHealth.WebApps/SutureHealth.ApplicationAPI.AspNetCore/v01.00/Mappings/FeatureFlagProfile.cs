namespace SutureHealth.Application.v0100.Mappings;

public class FeatureFlagProfile : AutoMapper.Profile
{
    public FeatureFlagProfile()
    {
        CreateMap<Models.FeatureFlag, FeatureFlag>();
    }
}
