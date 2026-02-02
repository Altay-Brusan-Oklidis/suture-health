using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: HostingStartup(typeof(SutureHealth.Patients.Services.AdmitDischargeTransfer.SqlServer.HostingStartup))]

namespace SutureHealth.Patients.Services.AdmitDischargeTransfer.SqlServer;

public class HostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddDbContext<Kno2DbContext>(options =>
            {
                var configuration = context.Configuration;
                var connString = new global::Microsoft.Data.SqlClient.SqlConnectionStringBuilder()
                {
                    DataSource = configuration["SqlDatabase:DataSource"],
                    UserID = configuration["SqlDatabase:UserID"],
                    Password = configuration["SqlDatabase:Password"],
                    InitialCatalog = configuration["SqlDatabase:InitialCatalog:SutureHealthAPI"],
                    ApplicationName = "Kno2-MessagePersister",
                    Pooling = true,
                    Encrypt = true,
                    TrustServerCertificate = true
                };

                options.UseSqlServer(connString.ToString(), sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                                                                                    .CommandTimeout(60));
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging(!context.HostingEnvironment.IsEnvironment("prod"));
            }, ServiceLifetime.Transient);

            services.AddTransient<Kno2DbUnitOfWork>();
        });
    }
}
