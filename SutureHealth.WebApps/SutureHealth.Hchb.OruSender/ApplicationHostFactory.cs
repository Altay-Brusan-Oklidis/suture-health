using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SutureHealth.Diagnostics;
using SutureHealth.Extensions.Configuration;
using SutureHealth.Hchb.Services;
using SutureHealth.Hchb.Services.SqlServer;

namespace SutureHealth.Hchb.OruSender;

public class ApplicationHostFactory
{
    private static ServiceProvider serviceProvider;

    public ApplicationHostFactory()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Ensure the path is set to current directory
            .AddJsonFile("appsettings.json");
        
        builder.AddDefaultConfigurations(runtimeEnvironment: "prod");
        
        var configuration = builder.Build();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging((loggingBuilder) =>
        {
            loggingBuilder.AddConfiguration(configuration);
            loggingBuilder.AddSimpleConsole(c =>
            {
                c.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
                c.IncludeScopes = true;
                c.SingleLine = true;
                c.UseUtcTimestamp = true;
            });
        });
        
        serviceCollection.AddAWSService<global::Amazon.S3.IAmazonS3>();
        serviceCollection.AddScoped<ITracingService, NullTracingService>();
        serviceCollection.AddTransient<HchbWebDbContext>(provider => 
            provider.GetService<SqlServerHchbWebDbContext>());
        
        serviceCollection.AddDbContext<SqlServerHchbWebDbContext>(options =>
        {
            var connString = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder()
            {
                DataSource = configuration["SqlDatabase:DataSource"],
                UserID = configuration["SqlDatabase:UserID"],
                Password = configuration["SqlDatabase:Password"],
                InitialCatalog = configuration["SqlDatabase:InitialCatalog:SutureSign"],
                ApplicationName = nameof(HchbServiceProvider),
                Pooling = true,
                Encrypt = true,
                TrustServerCertificate = true
            };

            options.UseSqlServer(connString.ToString(), sqlOptions =>
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery).CommandTimeout(60));
            options.EnableDetailedErrors();
            //options.EnableSensitiveDataLogging(!context.HostingEnvironment.IsEnvironment("prod"));
        });

        Services = serviceCollection.BuildServiceProvider();
    }
    
    /// <summary>
    /// Gets the services configured for the program (for example, using <see cref="M:HostBuilder.ConfigureServices(Action&lt;HostBuilderContext,IServiceCollection&gt;)" />).
    /// </summary>
    public IServiceProvider Services { get; }
    
    public static IHost Create()
    {
        var applicationHost = Host.CreateDefaultBuilder()
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
                webBuilder.ConfigureAppConfiguration((host, config) =>
                    {
                        config.AddJsonFile("appsettings.json")
                            .AddDefaultConfigurations(runtimeEnvironment: "prod")
                            .Build();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddAWSService<global::Amazon.S3.IAmazonS3>();
                        services.AddScoped<ITracingService, NullTracingService>();
                        services.AddTransient<HchbWebDbContext>(provider => 
                            provider.GetService<SqlServerHchbWebDbContext>());
                        services.AddDbContext<SqlServerHchbWebDbContext>(options =>
                        {
                            var configuration = context.Configuration;

                            var connString = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder()
                            {
                                DataSource = configuration["SqlDatabase:DataSource"],
                                UserID = configuration["SqlDatabase:UserID"],
                                Password = configuration["SqlDatabase:Password"],
                                InitialCatalog = configuration["SqlDatabase:InitialCatalog:SutureSign"],
                                ApplicationName = nameof(HchbServiceProvider),
                                Pooling = true,
                                Encrypt = true,
                                TrustServerCertificate = true
                            };

                            options.UseSqlServer(connString.ToString(), sqlOptions =>
                                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery).CommandTimeout(60));
                            options.EnableDetailedErrors();
                            options.EnableSensitiveDataLogging(!context.HostingEnvironment.IsEnvironment("prod"));
                        });

                    });
            })
            .Build();

        return applicationHost;
    } 
}