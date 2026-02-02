using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(SutureHealth.Patients.Services.HostingStartup))]
namespace SutureHealth.Patients.Services
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddTransient<IPatientServicesProvider, PatientServicesProvider>();
            });
        }
    }
}
