using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(SutureHealth.Hchb.Services.HostingStartup))]
namespace SutureHealth.Hchb.Services
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                services.AddTransient<IHchbServiceProvider, HchbServiceProvider>();
            });
        }
    }
}
