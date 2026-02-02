using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SutureHealth.PatientAPI.Services.AdmitDischargeTransfer.Kno2;
using SutureHealth.Patients.Services.AdmitDischargeTransfer;
using System.Data.Common;

namespace SutureHealth.PatientAPI.Services.Kno2.Lambda.Testing;

internal class ServiceProviderMock : Mock<IServiceProvider>, IDisposable
{
    // We are managing the connection to the sqlite in-memory database by ourselves because when the connection gets opened,
    // a new database is created in memory. When the connection gets closed, the database is destroyed. We keep the connection
    // open until a test ends. Credits: https://www.meziantou.net/testing-ef-core-in-memory-using-sqlite.htm#testing-using-sqlite-be8897
    private DbConnection? dbConnection;

    public ServiceProviderMock(IKno2ApiClient kno2ApiClient)
    {
        // The first 3 Setups bellow are required to "mock" the GetRequiredService extension (static) method of
        // IServiceProvider. Credits go to https://stackoverflow.com/a/44337673/4543201
        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(Object);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);

        Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactory.Object);

        Setup(x => x.GetService(typeof(IKno2ApiClient))).Returns(kno2ApiClient);

        Setup(x => x.GetService(typeof(Kno2DbUnitOfWork))).Returns(() =>
        {
            var kno2DbContext = new Kno2DbContext(CreateOptions());
            kno2DbContext.Database.EnsureCreated();
            return new Kno2DbUnitOfWork(kno2DbContext);
        });

        dbConnection = new SqliteConnection("DataSource=:memory:");
        dbConnection.Open();
    }

    public void Dispose()
    {
        if (dbConnection is null)
        {
            return;
        }

        dbConnection.Dispose();
        dbConnection = null;
    }

    private DbContextOptions<Kno2DbContext> CreateOptions()
    {
        if (dbConnection is null)
        {
            throw new InvalidOperationException("DbConnection is null");
        }

        return new DbContextOptionsBuilder<Kno2DbContext>().UseSqlite(dbConnection).Options;
    }
}
