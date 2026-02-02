using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SutureHealth.DataScraping;
using SutureHealth.DataScraping.Services;
using SutureHealth.Diagnostics;
using SutureHealth.Extensions.Configuration;
using System.Text.Json;
using static Amazon.Lambda.SQSEvents.SQSEvent;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SutureHealth.DataScraping.Services.Lambda;

public partial class Function
{
    protected IDataScrapingServicesProvider DataScrapingService { get; }
    protected IServiceProvider Services { get; }
    protected ITracingService TracingService { get; }
    public IMapper Mapper { get; }



    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        DotNetEnv.Env.Load();
        DotNetEnv.Env.TraversePath().Load();

        var host = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
        {
            var assemblies = new string[] {
                                    typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name,                                    
                                    typeof(SutureHealth.DataScraping.Services.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.DataScraping.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Patients.Services.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Patients.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                              };
            webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
            webBuilder.ConfigureAppConfiguration((host, config) => config.AddDefaultConfigurations().Build())
            .ConfigureServices((context, services) =>
            {
                services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());
                services.AddScoped<ITracingService, NullTracingService>();
                services.AddScoped<IDataScrapingServicesProvider, DataScrapingServicesProvider>();
            });
        }).Build();
        Services = host.Services;
        Mapper = Services.GetRequiredService<IMapper>();
        TracingService = Services.GetRequiredService<ITracingService>();
        DataScrapingService = Services.GetRequiredService<IDataScrapingServicesProvider>();
    }    
}