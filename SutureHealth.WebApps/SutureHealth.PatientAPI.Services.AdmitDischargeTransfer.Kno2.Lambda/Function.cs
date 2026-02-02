using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SutureHealth.Extensions.Configuration;
using SutureHealth.PatientAPI.Services.AdmitDischargeTransfer.Kno2;
using SutureHealth.Patients.ADT.Kno2;
using SutureHealth.Patients.Services.AdmitDischargeTransfer;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SutureHealth.PatientAPI.Services.Kno2.Lambda;

public class Function
{
    private IServiceProvider Services { get; }

    private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    public Function()
    {
        var host = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
        {
            var assemblies = new string[] {
                typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name!,
                typeof(SutureHealth.Patients.Services.AdmitDischargeTransfer.SqlServer.HostingStartup).Assembly.GetName().Name!,
                typeof(SutureHealth.PatientAPI.Services.AdmitDischargeTransfer.Kno2.HostingStartup).Assembly.GetName().Name!
            };
            webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
            webBuilder.ConfigureAppConfiguration((host, config) => config.AddDefaultConfigurations().Build())
                      .ConfigureServices((context, services) =>
                      {
                          services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());
                      });
        })
        .Build();

        Services = host.Services;
    }

    public Function(IServiceProvider services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Processing SQS MessageId: {message.MessageId}");
            var kno2WebhookMessage = JsonSerializer.Deserialize<Kno2WebhookMessage>(message.Body, jsonSerializerOptions);
            if (kno2WebhookMessage is null)
            {
                context.Logger.LogInformation($"Deserialization of message Body returned null. Body: {message.Body}");
                return;
            }

            context.Logger.LogInformation($"Kno2 message id: {kno2WebhookMessage.Id}");

            var kno2ApiClient = Services.GetRequiredService<IKno2ApiClient>();
            string kno2MessageJson = await kno2ApiClient.RequestMessageAsync(new Uri(kno2WebhookMessage.Url));
            if (kno2MessageJson is null)
            {
                context.Logger.LogInformation($"Kno2ApiClient.RequestMessageAsync returned null. Uri: {kno2WebhookMessage.Url}");
                return;
            }

            var kno2Message = JsonSerializer.Deserialize<Message>(kno2MessageJson, jsonSerializerOptions);
            if (kno2Message is null)
            {
                context.Logger.LogInformation($"Deserialization of kno2MessageJson returned null. kno2MessageJson: {kno2MessageJson}");
                return;
            }

            using var kno2DbUnitOfWork = Services.GetRequiredService<Kno2DbUnitOfWork>();
            var repository = kno2DbUnitOfWork.GetRepository<Message>();
            var messageId = repository.GetAsQueryable().Where(m => m.ObfuscatedId == kno2Message.ObfuscatedId).Select(m => m.Id).FirstOrDefault();
            if (messageId == default)
            {
                repository.Insert(kno2Message);
            }
            else
            {
                kno2Message.Id = messageId;
                repository.Update(kno2Message);
            }

            await kno2DbUnitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex.ToString());
            throw;
        }
    }
}
