using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(SutureHealth.Notifications.Providers.SimpleEmailService.HostingStartup))]
namespace SutureHealth.Notifications.Providers.SimpleEmailService
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddAWSService<Amazon.SimpleEmail.IAmazonSimpleEmailService>();
                services.AddScoped<INotificationProvider, SimpleEmailServiceNotificationProvider>();
            });
        }
    }
}
