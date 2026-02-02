using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(SutureHealth.Patients.AspNetCore.HostingStartup))]
namespace SutureHealth.Patients.AspNetCore
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddAutoMapper(typeof(HostingStartup).Assembly);
                services.AddControllers().AddApplicationPart(typeof(HostingStartup).Assembly);
            });
        }
    }
}
