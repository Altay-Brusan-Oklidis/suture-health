using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using SutureHealth.Diagnostics;
using SutureHealth.Extensions.Configuration;
using SutureHealth.Notifications;
using SutureHealth.Notifications.Providers.CustomerIO;

namespace SutureHealth.NotificationAPI.Testing.Integration
{
    public class CustomerIo
    {
        private IHost ApplicationHost { get; set; }
        private INotificationProvider CustomerIoProvider { get; set; }

        public CustomerIo()
        {
            ApplicationHost = Host.CreateDefaultBuilder()
                                  .ConfigureWebHostDefaults(webBuilder =>
                                  {
                                      var assemblies = new string[] {
                                          typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Notifications.Services.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Notifications.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Notifications.Providers.CustomerIO.HostingStartup).Assembly.GetName().Name
                                      };

                                      webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
                                      webBuilder.ConfigureAppConfiguration((host, config) =>
                                      {
                                          config.AddJsonFile("appsettings.test.json")
                                                .AddDefaultConfigurations()
                                                .Build();
                                      })
                                      .ConfigureServices(services =>
                                      {
                                          services.AddScoped<ITracingService, NullTracingService>();
                                      });
                                  })
                                  .Build();

            CustomerIoProvider = ApplicationHost.Services.GetServices<INotificationProvider>().OfType<CustomerIoNotificationProvider>().First();
        }

        [Fact]
        [Trait("TestCategory", "Development")]
        [Trait("TestCategory", "Integration")]
        public async Task SendTextEmail()
        {
            var notification = new NotificationStatus()
            {
                DestinationUri = "mailto:mculotta@suturehealth.com",
                Subject = "Integration Test",
                SourceText = "Test message contents"
            };

            await CustomerIoProvider.SendNotificationAsync(notification);

            Assert.True(notification.Success, notification.Message);
        }
    }
}
