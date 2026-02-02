using SutureHealth.Extensions.Configuration;

namespace SutureHealth.AspNetCore.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath().Load();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, loggingBuilder) =>
                {
                    loggingBuilder.AddConfiguration(context.Configuration);
                    loggingBuilder.AddSimpleConsole(c =>
                    {
                        c.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
                        c.IncludeScopes = true;
                        c.SingleLine = true;
                        c.UseUtcTimestamp = true;
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var assemblies = new string[] {
                        typeof(SutureHealth.AspNetCore.Identity.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Application.AspNetCore.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Application.Services.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Application.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Documents.Services.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Documents.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Notifications.Services.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Notifications.Providers.SRFax.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Notifications.Providers.SimpleEmailService.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Notifications.Providers.CustomerIO.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Notifications.Providers.TwilioSMS.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Notifications.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Patients.AspNetCore.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Patients.Services.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Patients.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Providers.Services.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Providers.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Reporting.Services.Generation.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Reporting.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Requests.AspNetCore.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Requests.Services.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Requests.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Revenue.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Visits.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Hchb.Services.HostingStartup).Assembly.GetName().Name,
                        typeof(SutureHealth.Hchb.Services.SqlServer.HostingStartup).Assembly.GetName().Name
                    };

                    webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
                    webBuilder.ConfigureAppConfiguration((host, config) => config.AddDefaultConfigurations().Build());
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(options => { options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5); });
                });
    }
}
