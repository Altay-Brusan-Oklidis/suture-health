using SutureHealth.Application.v0100.Models;

namespace SutureHealth.Application.v0100.Mappings;

public class ApplicationProfile : AutoMapper.Profile
{
    public ApplicationProfile()
    {
        CreateMap<SutureHealth.Application.ApplicationSetting, SettingDetail>();       
    }
}
