using AutoMapper;
using SutureHealth.Requests;
using System.Linq;
using System.Text.RegularExpressions;

using Domain = SutureHealth.Patients;

namespace SutureHealth.Patients.v0100.Mappings
{
    /// <summary>
    /// 
    /// </summary>
    public class PatientMappingProfile : Profile
    {
        public const string CPO_ENTRIES = "CpoEntries";
        public PatientMappingProfile()
        {
            CreateMap<Domain.Patient, Models.Patient>()
                .ForMember(m => m.Id, opt => opt.MapFrom(source => source.PatientId))
                .ForMember(m => m.Gender, opt => opt.MapFrom(source => (Models.Gender)(int)source.Gender));
            CreateMap<Domain.Patient, Models.PatientListItem>()
                .ForMember(m => m.TotalCpoTimeThisMonth,
                opt => opt.MapFrom<TotalCpoTimeValueResolver>());

            CreateMap<Domain.PatientIdentifier, Models.Identifier>()
                .ForMember(m => m.Id, opt => opt.MapFrom(source => source.Value));
            //.ForMember(m => m.Created, opt => opt.MapFrom(source => source.CreatedAt))
            //.ForMember(m => m.Updated, opt => opt.MapFrom(source => source.UpdatedAt));

            // Mapping from model to domain
            CreateMap<Models.Patient, Domain.Patient>()
                .ForMember(m => m.Birthdate, opt => opt.MapFrom(source => source.Birthdate.Date));

            CreateMap<Models.Identifier, Domain.PatientIdentifier>()
               .ForMember(m => m.Type, opt => opt.MapFrom(source => source.Type))
               .ForMember(m => m.Value, opt => opt.MapFrom(source => source.Id));

            CreateMap<Models.ContactInfo, Domain.PatientContact>()
               .ForMember(m => m.Type, opt => opt.MapFrom(source => source.Type))
               .ForMember(m => m.Value, opt => opt.MapFrom(source => source.Value));

            CreateMap<Models.Address, Domain.PatientAddress>()
              .ForMember(m => m.Id, opt => opt.Ignore())
              .ForMember(m => m.ParentId, opt => opt.Ignore())
              .ForMember(m => m.Parent, opt => opt.Ignore());

            CreateMap<Domain.PatientAddress, Models.Address>()
               //.ForMember(m => m.Created, opt => opt.MapFrom(source => source.CreatedAt))
               //.ForMember(m => m.Updated, opt => opt.MapFrom(source => source.UpdatedAt))
               .ForMember(m => m.PostalCode, opt => opt.MapFrom(source => source.PostalCode != null ?
                    Regex.Replace(source.PostalCode, @"(?<zip5>[\d]{5})(?:\-?(?<zipplus4>[\d]{1,4}))?",
                        m => m.Groups["zipplus4"].Success ?
                            $"{m.Groups["zip5"].Value}-{m.Groups["zipplus4"].Value}"
                            : source.PostalCode
                    ) : ""));

            CreateMap<Domain.PatientContact, Models.ContactInfo>();
            //.ForMember(m => m.Created, opt => opt.MapFrom(source => source.CreatedAt))
            //.ForMember(m => m.Updated, opt => opt.MapFrom(source => source.UpdatedAt));
        }
        public class TotalCpoTimeValueResolver : AutoMapper.IValueResolver<Domain.Patient, Models.PatientListItem, int>
        {
            public int Resolve(Domain.Patient source, Models.PatientListItem destination, int destMember, ResolutionContext context)
            {
                if (!context.Items.ContainsKey(CPO_ENTRIES)) return 0;
                if (context.Items[CPO_ENTRIES] is not CpoEntry[]) return 0;
                var cpoEntries = context.Items[CPO_ENTRIES] as CpoEntry[];

                var patientEntries = cpoEntries.Where(cpo => cpo.PatientId == source.PatientId).ToList();
                var totalCPO = patientEntries.Sum(cpo => cpo.Minutes);

                return totalCPO;
            }
        }

    }

}
