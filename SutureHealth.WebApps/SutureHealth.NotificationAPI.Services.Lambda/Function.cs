using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using SutureHealth.Extensions.Configuration;
using SutureHealth.Diagnostics;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SutureHealth.Notifications.Services.Lambda
{
    public partial class Function
    {
        protected IHost ApplicationHost { get; }
        protected INotificationService NotificationService { get; }
        protected ILogger<Function> Logger { get; }
        protected IConfiguration Configuration { get; }
        protected IServiceProvider Services { get => ApplicationHost.Services; }
        protected ITracingService Tracing { get; }
        protected IHttpClientFactory HttpClientFactory { get; }

        public Function() : this((host, config) => config.AddDefaultConfigurations().Build())
        {

        }

        protected Function(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath().Load();

            ApplicationHost = Host.CreateDefaultBuilder()
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   var assemblies = new string[] {
                       typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name,
                       typeof(SutureHealth.Notifications.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                       typeof(SutureHealth.Notifications.Services.HostingStartup).Assembly.GetName().Name,
                       typeof(SutureHealth.Notifications.Providers.SimpleEmailService.HostingStartup).Assembly.GetName().Name,
                       typeof(SutureHealth.Notifications.Providers.CustomerIO.HostingStartup).Assembly.GetName().Name,
                       typeof(SutureHealth.Notifications.Providers.SRFax.HostingStartup).Assembly.GetName().Name,
                       typeof(SutureHealth.Notifications.Providers.TwilioSMS.HostingStartup).Assembly.GetName().Name
                   };

                   webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies))
                             .ConfigureAppConfiguration((host, config) => configureDelegate(host, config))
                             .ConfigureServices((context, services) =>
                             {
                                 if (context.HostingEnvironment.IsEnvironment("ci"))
                                 {
                                     services.AddScoped<ITracingService, NullTracingService>();
                                 }
                                 else
                                 {
                                     AWSSDKHandler.RegisterXRayForAllServices();
                                     services.AddScoped<ITracingService, XrayTracingService>();
                                 }
                                 
                                 services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());
                                 services.AddOptions<NotificationOptions>()
                                         .Bind(context.Configuration.GetSection("SutureHealth:NotificationServicesProvider"))
                                         .Configure(options =>
                                         {
                                             options.GenerateCallbackUrl = notification =>
                                             {
                                                 var providerId = notification.ProviderId;
                                                 if (!providerId.HasValue)
                                                 {
                                                     var provider = Services.GetServices<INotificationProvider>()
                                                                            .FirstOrDefault(s => s.Channel == notification.Channel);
                                                     notification.ProviderId = provider.ProviderId;
                                                     notification.ProviderType = provider.GetType().FullName;
                                                 }

                                                 return options.CallbackUrl.Interpolate(notification);
                                             };
                                         });
                             });
               })
               .ConfigureLogging((context, loggingBuilder) =>
               {
                   loggingBuilder.AddConfiguration(context.Configuration);
                   loggingBuilder.AddSimpleConsole(c =>
                   {
                       c.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
                       c.IncludeScopes = true;
                   });
                   loggingBuilder.AddLambdaLogger(context.Configuration.GetLambdaLoggerOptions());
               })
               .Build();

            Configuration = Services.GetRequiredService<IConfiguration>();
            HttpClientFactory = Services.GetRequiredService<IHttpClientFactory>();
            Logger = Services.GetRequiredService<ILogger<Function>>();
            NotificationService = Services.GetRequiredService<INotificationService>();
            Tracing = Services.GetRequiredService<ITracingService>();
        }
    }
}