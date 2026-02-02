using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SutureHealth.Notifications;

[assembly: HostingStartup(typeof(SutureHealth.Notifications.Providers.CustomerIO.HostingStartup))]
namespace SutureHealth.Notifications.Providers.CustomerIO
{
    public class HostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddScoped<INotificationProvider, CustomerIoNotificationProvider>();
            });
        }
    }
}
