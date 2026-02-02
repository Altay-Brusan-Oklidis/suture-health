using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: HostingStartup(typeof(SutureHealth.Application.Services.SqlServer.HostingStartup))]
namespace SutureHealth.Application.Services.SqlServer
{
    public class HostingStartup : IHostingStartup
    {
        void IHostingStartup.Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<SqlServerApplicationDbContext>(options =>
                {
                    var configuration = context.Configuration;
                    var connString = new global::Microsoft.Data.SqlClient.SqlConnectionStringBuilder()
                    {
                        DataSource = configuration["SqlDatabase:DataSource"],
                        UserID = configuration["SqlDatabase:UserID"],
                        Password = configuration["SqlDatabase:Password"],
                        InitialCatalog = configuration["SqlDatabase:InitialCatalog:SutureHealthAPI"],
                        ApplicationName = nameof(ApplicationServices<ApplicationDbContext>),
                        Pooling = true,
                        Encrypt = true,
                        TrustServerCertificate = true
                    };

                    options.UseSqlServer(connString.ToString(), opts => opts.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                           .EnableDetailedErrors()
                           .EnableSensitiveDataLogging(!context.HostingEnvironment.IsEnvironment("prod"));
                }, ServiceLifetime.Transient);

                services.AddTransient<IFeatureFlagsServices, FeatureFlagsServices>();
                services.AddScoped<ApplicationDbContext>(provider => provider.GetService<SqlServerApplicationDbContext>());
                services.AddScoped<IdentityDbContext>(provider => provider.GetService<SqlServerApplicationDbContext>());

                services.AddDbContext<FeatureFlagsDbContext, SqlServerFeatureFlagsDbContext>(options =>
                {
                    var configuration = context.Configuration;
                    var connString = new global::Microsoft.Data.SqlClient.SqlConnectionStringBuilder()
                    {
                        DataSource = configuration["SqlDatabase:DataSource"],
                        UserID = configuration["SqlDatabase:UserID"],
                        Password = configuration["SqlDatabase:Password"],
                        InitialCatalog = configuration["SqlDatabase:InitialCatalog:SutureSign"],
                        ApplicationName = nameof(SqlServerFeatureFlagsDbContext),
                        Pooling = true,
                        Encrypt = true,
                        TrustServerCertificate = true
                    };

                    options.UseSqlServer(connString.ToString(), sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging(!context.HostingEnvironment.IsEnvironment("prod"));
                }, ServiceLifetime.Transient);
            });
        }
    }
}
