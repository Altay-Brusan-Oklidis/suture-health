using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SutureHealth.Notifications.AspNetCore.v0001.Mappings
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<NotificationStatus, Models.Notification>().ReverseMap();
            CreateMap<NotificationStatus, Models.NotificationStatus>()
                .IncludeBase<NotificationStatus, Models.Notification>()
                .ForMember(s => s.UniqueNotificationId, x => x.MapFrom(s => s.Id))
                .ReverseMap();
        }
    }
}
