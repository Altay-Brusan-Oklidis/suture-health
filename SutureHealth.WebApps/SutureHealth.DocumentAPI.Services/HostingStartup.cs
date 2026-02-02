using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SutureHealth.Storage;
using SutureHealth.Imaging;

[assembly: HostingStartup(typeof(SutureHealth.Documents.Services.HostingStartup))]
namespace SutureHealth.Documents.Services
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddScoped<TextractService>();
                services.AddSingleton<IBinaryStorageService, SimpleStorageService>();
                services.AddTransient<IDocumentServicesProvider, TelerikDocumentServicesProvider>();
            });
        }
    }
}
