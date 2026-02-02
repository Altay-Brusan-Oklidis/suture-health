using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(SutureHealth.Notifications.Providers.SRFax.HostingStartup))]
namespace SutureHealth.Notifications.Providers.SRFax
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddTransient<INotificationProvider, SRFaxNotificationProvider>();
            });
        }
    }
}
