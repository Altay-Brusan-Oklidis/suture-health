using AutoMapper;
using SutureHealth.DataScraping.v0100.Models;

using Domain = SutureHealth.DataScraping;

namespace SutureHealth.DataScrapingAPI.v0100.Mappings
{
    class DataScrapingProfile : Profile
    {

        public DataScrapingProfile()
        {
            //Mapping from Model to Domain
            CreateMap<ScrapPatientHtmlRequest, Domain.ScrapPatientHtmlRequest>();
        }                      
    }
}
