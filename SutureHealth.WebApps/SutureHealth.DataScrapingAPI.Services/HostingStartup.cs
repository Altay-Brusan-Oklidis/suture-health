using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SutureHealth.DataScraping.Services;
using SutureHealth.Patients.Services;
using SutureHealth.Storage;

[assembly: HostingStartup(typeof(SutureHealth.DataScraping.Services.HostingStartup))]
namespace SutureHealth.DataScraping.Services
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddAutoMapper(typeof(HostingStartup).Assembly);
                services.AddTransient<IDataScrapingServicesProvider, DataScrapingServicesProvider>();
                services.AddTransient<IPatientServicesProvider, PatientServicesProvider>();                
            });
        }
    }
}
