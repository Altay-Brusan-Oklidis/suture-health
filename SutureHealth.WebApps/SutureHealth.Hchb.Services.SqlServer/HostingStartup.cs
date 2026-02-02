using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: HostingStartup(typeof(SutureHealth.Hchb.Services.SqlServer.HostingStartup))]
namespace SutureHealth.Hchb.Services.SqlServer
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                services.AddHttpClient();                
                services.AddTransient<HchbWebDbContext>(provider => provider.GetService<SqlServerHchbWebDbContext>());
                services.AddDbContext<SqlServerHchbWebDbContext>(options =>
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
                    options.EnableSensitiveDataLogging(!context.HostingEnvironment.IsEnvironment("prod"));
                    options.ReplaceService<IModelCacheKeyFactory, SchemaAwareModelCacheKeyFactory>();
                });
                services.AddSingleton<IDbContextSchema>(new DbContextSchema(configuration["SqlDatabase:Schema"]));
                
            });
        }
    }
}