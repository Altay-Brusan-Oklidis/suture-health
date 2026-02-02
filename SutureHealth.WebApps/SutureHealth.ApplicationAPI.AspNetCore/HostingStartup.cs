using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SutureHealth.AspNetCore.Identity;

[assembly: HostingStartup(typeof(SutureHealth.Application.AspNetCore.HostingStartup))]
namespace SutureHealth.Application.AspNetCore
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