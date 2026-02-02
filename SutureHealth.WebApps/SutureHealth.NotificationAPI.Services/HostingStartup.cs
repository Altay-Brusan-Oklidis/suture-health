using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SutureHealth.Notifications.Services;

[assembly: HostingStartup(typeof(SutureHealth.Notifications.Services.HostingStartup))]
namespace SutureHealth.Notifications.Services
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddTransient<INotificationService, NotificationService>();
            });
        }
    }
}
