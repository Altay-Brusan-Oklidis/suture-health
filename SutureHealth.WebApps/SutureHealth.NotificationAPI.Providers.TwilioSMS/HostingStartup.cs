using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(SutureHealth.Notifications.Providers.TwilioSMS.HostingStartup))]
namespace SutureHealth.Notifications.Providers.TwilioSMS
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddScoped<INotificationProvider, TwilioSMSProvider>();
                services.AddScoped<INotificationProvider, TwilioTextToSpeechProvider>();
            });
        }
    }
}
