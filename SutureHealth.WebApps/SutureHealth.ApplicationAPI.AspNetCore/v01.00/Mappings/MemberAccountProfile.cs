using AutoMapper;
using SutureHealth.Application.v0100.Models;
using System.Linq;

namespace SutureHealth.Application.v0100.Mappings;

public class MemberAccountProfile : Profile
{
    public const string MEMBER_RELATIONSHIP = "MemberRelationship";
    public MemberAccountProfile()
    {
        CreateMap<SutureHealth.Application.MemberBase, Models.Member>();
        CreateMap<SutureHealth.Application.Member, Models.Member>();
        CreateMap<SutureHealth.Application.MemberIdentity, Models.Member>()
            .ForMember(dest => dest.HasSingleSigner, opt => opt.MapFrom<HasSingleSignerValueResolver>());
        CreateMap<SutureHealth.Application.MemberSetting, SettingDetail>();
    }

    public class HasSingleSignerValueResolver : AutoMapper.IValueResolver<MemberIdentity, Models.Member, bool>
    {
        public bool Resolve(MemberIdentity source, Models.Member destination, bool destMember, ResolutionContext context)
        {
            if (!context.Items.ContainsKey(MEMBER_RELATIONSHIP)) return false;
            if (context.Items[MEMBER_RELATIONSHIP] is not IQueryable<MemberRelationship> memberRelationship) return false;
            var membershipQuery = context.Items[MEMBER_RELATIONSHIP] as IQueryable<MemberRelationship>;

            var subSigners = membershipQuery.Where(mr => (bool)mr.IsActive && mr.SupervisorMemberId == source.MemberId)
                                .Where(mr => mr.Subordinate.CanSign)
                                .ToList();
            var supSigners = membershipQuery.Where(mr => (bool)mr.IsActive && mr.SubordinateMemberId == source.MemberId)
                                .Where(mr => mr.Supervisor.CanSign)
                                .ToList();

            var signerCount = subSigners.Count + supSigners.Count + (source.CanSign ? 1 : 0);

            if (signerCount < 2) return true;

            return false;
        }
    }

}


