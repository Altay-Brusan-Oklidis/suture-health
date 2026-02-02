using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SutureHealth.Security;
using SutureHealth.Storage;

[assembly: HostingStartup(typeof(SutureHealth.Application.Services.HostingStartup))]
namespace SutureHealth.Application.Services
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddTransient<IApplicationService, ApplicationServices<ApplicationDbContext>>();
                services.AddTransient<IApplicationSettingService, ApplicationServices<ApplicationDbContext>>();
                services.AddTransient<IIdentityService, IdentityServiceProvider>();
                services.AddTransient<IIntegratorService, ApplicationServices<ApplicationDbContext>>();
                services.AddTransient<IMemberService, ApplicationServices<ApplicationDbContext>>();
                services.AddTransient<IOrganizationService, ApplicationServices<ApplicationDbContext>>();
                services.AddTransient<IOrganizationMemberService, ApplicationServices<ApplicationDbContext>>();
                services.AddScoped<IBinaryStorageService, SimpleStorageService>();
                services.AddScoped<IImageProcessingService, ImageProcessingService>();

            });
        }
    }
}
