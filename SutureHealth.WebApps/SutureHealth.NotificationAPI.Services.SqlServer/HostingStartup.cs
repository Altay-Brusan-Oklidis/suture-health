using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: HostingStartup(typeof(SutureHealth.Notifications.Services.SqlServer.HostingStartup))]
namespace SutureHealth.Notifications.Services.SqlServer
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddHttpClient();
                services.AddTransient<NotificationDbContext>(provider => provider.GetService<SqlServerNotificationDbContext>());
                services.AddDbContext<SqlServerNotificationDbContext>(options =>
                {
                    var configuration = context.Configuration;
                    var connString = new global::Microsoft.Data.SqlClient.SqlConnectionStringBuilder()
                    {
                        DataSource = configuration["SqlDatabase:DataSource"],
                        UserID = configuration["SqlDatabase:UserID"],
                        Password = configuration["SqlDatabase:Password"],
                        InitialCatalog = configuration["SqlDatabase:InitialCatalog:SutureHealthAPI"],
                        ApplicationName = nameof(NotificationService),
                        Pooling = true,
                        Encrypt = true,
                        TrustServerCertificate = true
                    };

                    options.UseSqlServer(connString.ToString(), sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                                                                                        .CommandTimeout(60));
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging(!context.HostingEnvironment.IsEnvironment("prod"));
                }, ServiceLifetime.Transient);
            });
        }
    }
}
