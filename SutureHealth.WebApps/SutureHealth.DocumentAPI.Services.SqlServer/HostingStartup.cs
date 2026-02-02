using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SutureHealth.Documents.Services.SqlServer;

[assembly: HostingStartup(typeof(SutureHealth.Documents.Services.SqlServer.HostingStartup))]
namespace SutureHealth.Documents.Services.SqlServer
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<DocumentDbContext, SqlServerDocumentDbContext>(options =>
                {
                    var configuration = context.Configuration;
                    var connString = new global::Microsoft.Data.SqlClient.SqlConnectionStringBuilder()
                    {
                        DataSource = configuration["SqlDatabase:DataSource"],
                        UserID = configuration["SqlDatabase:UserID"],
                        Password = configuration["SqlDatabase:Password"],
                        InitialCatalog = configuration["SqlDatabase:InitialCatalog:SutureHealthAPI"],
                        ApplicationName = nameof(SqlServerDocumentDbContext),
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
