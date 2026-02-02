using AutoMapper;

namespace SutureHealth.DataScraping.Mappings
{
    public class ScrapedPatientDetailProfile : Profile
    {
        public ScrapedPatientDetailProfile()
        {
            CreateMap<ScrapedPatientDetail, ScrapedPatientDetailHistory>();
        }
    }
}
