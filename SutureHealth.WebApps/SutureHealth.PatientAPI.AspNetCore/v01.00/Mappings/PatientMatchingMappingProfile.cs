using AutoMapper;
using Domain = SutureHealth.Patients;

namespace SutureHealth.Patients.v0100.Mappings;

class PatientMatchingMappingProfile : Profile
{
    public PatientMatchingMappingProfile()
    {
        CreateMap<Models.PatientMatchingRequest, Domain.PatientMatchingRequest>();

        CreateMap<Linq.MatchLevel, Models.MatchLevel>();
        CreateMap<Linq.MatchingResult<Domain.Patient>, Models.MatchingResult>();
        CreateMap<Linq.MatchingRuleResult<Domain.Patient>, Models.MatchingRuleResult>();
        CreateMap<Linq.MatchingRule<Domain.Patient>, Models.MatchingRule>();
        CreateMap<Domain.PatientMatchingResponse, Models.PatientMatchingResponse>();
    }
}
