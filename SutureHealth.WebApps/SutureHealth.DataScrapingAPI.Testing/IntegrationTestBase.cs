using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SutureHealth.DataScraping.Core.Services;
using SutureHealth.DataScraping.Services;
using SutureHealth.DataScraping.Services.SqlServer;
using SutureHealth.Diagnostics;
using SutureHealth.Extensions.Configuration;
using SutureHealth.Storage;
using System;

namespace SutureHealth.DataScrapingAPI.Testing
{
    public class IntegrationTestBase
    {
        protected IHost ApplicationHost { get;  }
        protected IDataScrapingServicesProvider DataScrapingService { get; }
        protected ITracingService Tracing { get; }
        protected IServiceProvider Services { get; }
        protected IConfiguration Configuration { get; }
        protected IStorageService StorageService { get; }

        protected DataScrapingDbContext DataScrapingDbContext;

        public IntegrationTestBase()
        {
            ApplicationHost = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var assemblies = new string[] {
                                        typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name,
                                        typeof(SutureHealth.DataScraping.Services.HostingStartup).Assembly.GetName().Name,
                                        typeof(SutureHealth.DataScraping.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                        typeof(SutureHealth.Patients.Services.HostingStartup).Assembly.GetName().Name,
                                        typeof(SutureHealth.Patients.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                  };
                    webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
                    webBuilder.ConfigureAppConfiguration((host, config) =>
                    {
                        config.AddJsonFile("appsettings.test.json")
                              .AddDefaultConfigurations(runtimeEnvironment: "test")
                              .Build();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddScoped<ITracingService, NullTracingService>();
                        services.AddScoped<IDataScrapingServicesProvider, DataScrapingServicesProvider>();

                        //Removing DbContexts manually in order to provide one unambigious in-memory db context.
                        services.RemoveAt(75);
                        services.RemoveAt(75);
                        services.RemoveAt(75);
                        services.RemoveAt(75);

                        services.AddTransient<DataScrapingDbContext>(provider => provider.GetService<SqlServerDataScrapingDbContext>());
                        services.AddDbContext<SqlServerDataScrapingDbContext>(options => { options.UseInMemoryDatabase("TestDB"); });
                       
                        services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());

                        
                    });
                }).Build();
            Services = ApplicationHost.Services;
            DataScrapingService = ApplicationHost.Services.GetRequiredService<IDataScrapingServicesProvider>();
            DataScrapingDbContext = ApplicationHost.Services.GetRequiredService<DataScrapingDbContext>();
            StorageService = ApplicationHost.Services.GetRequiredService<IStorageService>();
            Configuration = ApplicationHost.Services.GetRequiredService<IConfiguration>();            
        }       
    }
}
