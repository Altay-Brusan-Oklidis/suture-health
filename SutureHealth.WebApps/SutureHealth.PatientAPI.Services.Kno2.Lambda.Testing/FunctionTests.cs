using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SutureHealth.Patients.ADT.Kno2;
using SutureHealth.Patients.Services.AdmitDischargeTransfer;
using Xunit;

namespace SutureHealth.PatientAPI.Services.Kno2.Lambda.Testing;

public class FunctionTests
{
    [Fact]
    public async Task FunctionAddsKno2MessageToDatabase()
    {
        // Arrange
        var kno2ApiClientMock = new Kno2ApiClientMock();
        using var serviceProviderMock = new ServiceProviderMock(kno2ApiClientMock.Object);
        var function = new Function(serviceProviderMock.Object);

        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new SQSEvent.SQSMessage
                {
                    Body = $"{{\"Id\":\"{Kno2ApiClientMock.Id}\",\"Url\":\"{Kno2ApiClientMock.Uri}\"}}"
                }
            }
        };

        var context = new TestLambdaContext();

        // Act
        await function.FunctionHandler(sqsEvent, context);

        // Assert
        kno2ApiClientMock.Verify(x => x.RequestMessageAsync(Kno2ApiClientMock.Uri), Times.Once);

        using var unitOfWork = serviceProviderMock.Object.GetRequiredService<Kno2DbUnitOfWork>();
        Assert.Single(unitOfWork.GetRepository<Message>().Get());
    }

    [Fact]
    public async Task CallingFunctionWithSameMessageMultipleTimesUpdatesTheDatabase()
    {
        // Arrange
        var kno2ApiClientMock = new Kno2ApiClientMock();
        using var serviceProviderMock = new ServiceProviderMock(kno2ApiClientMock.Object);
        var function = new Function(serviceProviderMock.Object);

        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new SQSEvent.SQSMessage
                {
                    Body = $"{{\"Id\":\"{Kno2ApiClientMock.Id}\",\"Url\":\"{Kno2ApiClientMock.Uri}\"}}"
                }
            }
        };

        var context = new TestLambdaContext();

        // Act
        await function.FunctionHandler(sqsEvent, context);
        await function.FunctionHandler(sqsEvent, context);

        // Assert
        kno2ApiClientMock.Verify(x => x.RequestMessageAsync(Kno2ApiClientMock.Uri), Times.Exactly(2));

        using var unitOfWork = serviceProviderMock.Object.GetRequiredService<Kno2DbUnitOfWork>();
        Assert.Single(unitOfWork.GetRepository<Message>().Get());
        Assert.False(unitOfWork.GetRepository<Message>().Get().First().IsNew);
    }
}
