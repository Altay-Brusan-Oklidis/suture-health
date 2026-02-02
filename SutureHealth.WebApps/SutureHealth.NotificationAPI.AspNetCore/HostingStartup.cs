using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SutureHealth.Notifications.AspNetCore;
using SutureHealth.Notifications.Services;
using System.Text.Json;

[assembly: HostingStartup(typeof(SutureHealth.Notifications.AspNetCore.HostingStartup))]
namespace SutureHealth.Notifications.AspNetCore
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddAutoMapper(typeof(HostingStartup).Assembly);
                services.AddControllers()
                        .AddApplicationPart(typeof(HostingStartup).Assembly);
            });
        }
    }
}
