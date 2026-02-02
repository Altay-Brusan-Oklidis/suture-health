using Amazon.SimpleSystemsManagement.Model.Internal.MarshallTransformations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.Application.v0100.Models;
using System.Collections.Generic;
using System.Linq;

namespace SutureHealth.Application.v0100.Mappings;

public class OrganizationMappingProfile : AutoMapper.Profile
{
    public const string CONTEXT_CURRENT_USER = "CurrentUser";
    public const string ORGANIZATION_LOGOS = "Logos";
    public const string ADMIN_ORGANIZATON_IDS = "AdminOrganizationIds";
    public const string IS_SUBSCRIBED_TO_MARKETING = "MarketingPromoSubscription";
    public OrganizationMappingProfile()
    {
        CreateMap<SutureHealth.Application.Organization, Models.Organization>();
        CreateMap<SutureHealth.Application.Organization, CustomOrganization>()
            .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom<IsPrimaryValueResolver>())
            .ForMember(dest => dest.IsCurrentUserAdmin, opt => opt.MapFrom<IsCurrentUserAdminValueResolver>())
            .ForMember(dest => dest.Logo, opt => opt.MapFrom<LogoValueResolver>())
            .ForMember(dest => dest.IsSubscribedToInboxMarketing, opt => opt.MapFrom<IsSubscribedToInboxMarketingValueResolver>());
        CreateMap<OrganizationContact, Models.OrganizationContact>()
           .ForMember(m => m.Type, opt => opt.MapFrom(source => source.Type))
           .ForMember(m => m.Value, opt => opt.MapFrom(source => source.Value));
        CreateMap<SutureHealth.Application.OrganizationSetting, SettingDetail>();
    }

    public class IsPrimaryValueResolver : AutoMapper.IValueResolver<SutureHealth.Application.Organization, CustomOrganization, bool>
    {
        public bool Resolve(SutureHealth.Application.Organization source, CustomOrganization destination, bool destMember, ResolutionContext context)
        {
            if (context.Items[CONTEXT_CURRENT_USER] is not MemberIdentity currentUser) return false;
            var primOrgId = currentUser.PrimaryOrganizationId;
            if (source.OrganizationId == primOrgId) return true;

            return false;
        }
    }

    public class LogoValueResolver : AutoMapper.IValueResolver<SutureHealth.Application.Organization, CustomOrganization, byte[]>
    {
        public byte[] Resolve(SutureHealth.Application.Organization source, CustomOrganization destination, byte[] destMember, ResolutionContext context)
        {
            if (context.Items[ORGANIZATION_LOGOS] is not Dictionary<int, byte[]> listOfOrganizationLogo) return null;
            if (listOfOrganizationLogo == null) return null;
            if (listOfOrganizationLogo.ContainsKey(source.OrganizationId)) return listOfOrganizationLogo[source.OrganizationId];

            return null;
        }
    }

    public class IsCurrentUserAdminValueResolver : AutoMapper.IValueResolver<SutureHealth.Application.Organization, CustomOrganization, bool>
    {
        public bool Resolve(SutureHealth.Application.Organization source, CustomOrganization destination, bool destMember, ResolutionContext context)
        {
            if (context.Items[ADMIN_ORGANIZATON_IDS] is not int[] adminOrganizationIds) return false;
            if (adminOrganizationIds == null) return false;
            if (adminOrganizationIds.Any(id => id == source.OrganizationId)) return true;            

            return false;
        }
    }

    public class IsSubscribedToInboxMarketingValueResolver : AutoMapper.IValueResolver<SutureHealth.Application.Organization, CustomOrganization, bool>
    {
        public bool Resolve(SutureHealth.Application.Organization source, CustomOrganization destination, bool destMember, ResolutionContext context)
        {
            if (context.Items[IS_SUBSCRIBED_TO_MARKETING] is not BillableEntity[] MarketingPromoSubscription) return false;
            if (MarketingPromoSubscription == null) return false;
            if (MarketingPromoSubscription.Any(mps => (mps.OrganizationId == source.OrganizationId) && mps.IsSubscribed)) return true;

            return false;
        }
    }

}

