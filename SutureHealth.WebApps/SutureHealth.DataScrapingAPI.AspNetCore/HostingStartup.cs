using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(SutureHealth.DataScraping.AspNetCore.HostingStartup))]
namespace SutureHealth.DataScraping.AspNetCore
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

        public static void ConfigureAppServices(IServiceCollection services)
        {
        }
    }
}