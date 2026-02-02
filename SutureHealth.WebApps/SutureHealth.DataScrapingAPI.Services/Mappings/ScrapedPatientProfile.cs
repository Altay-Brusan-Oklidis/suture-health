using AutoMapper;

namespace SutureHealth.DataScraping.Mappings
{
    public class ScrapedPatientProfile : Profile
    {
        public ScrapedPatientProfile()
        {
            CreateMap<ScrapedPatient, ScrapedPatientHistory>();
        }
    }
}
