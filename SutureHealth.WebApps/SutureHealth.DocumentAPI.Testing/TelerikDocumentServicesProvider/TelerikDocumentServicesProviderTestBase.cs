using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Hosting;
using SutureHealth.Extensions.Configuration;
using SutureHealth.Documents.Services;

namespace SutureHealth.Documents.Testing
{
    [TestClass]
    public partial class TelerikDocumentServicesProviderTestBase
    {
        private IHost ApplicationHost { get; }
        private IDocumentServicesProvider DocumentService { get; set; }

        public TelerikDocumentServicesProviderTestBase()
        {
            ApplicationHost = Host.CreateDefaultBuilder()
                                  .ConfigureWebHostDefaults(webBuilder =>
                                  {
                                      var assemblies = new string[] {
                                          typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Documents.Services.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Documents.Services.SqlServer.HostingStartup).Assembly.GetName().Name
                                      };
                                      webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
                                      webBuilder.ConfigureAppConfiguration((host, config) =>
                                      {
                                          config.AddJsonFile("appsettings.test.json", true)
                                                .AddDefaultConfigurations(runtimeEnvironment: "ci")
                                                .Build();
                                      });
                                      webBuilder.ConfigureServices(services => { });
                                  })
                                  .Build();
        }

        [TestInitialize]
        public void Initialize()
        {
            DocumentService = ApplicationHost.Services.GetRequiredService<IDocumentServicesProvider>();
        }
    }
}
