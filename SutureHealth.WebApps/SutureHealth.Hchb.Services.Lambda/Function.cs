using Amazon.Lambda.Core;
using Amazon.Lambda.KinesisEvents;
using AngleSharp.Html;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Slack.Webhooks;
using SutureHealth.DataStream;
using SutureHealth.Diagnostics;
using SutureHealth.Extensions.Configuration;
using SutureHealth.Hchb.Services.SqlServer;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SutureHealth.Hchb.Services.Lambda;

public class Function
{
    protected IHchbServiceProvider HchbService { get; }
    public Function()
    {
        DotNetEnv.Env.Load();
        DotNetEnv.Env.TraversePath().Load();

        var host = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
        {
            var assemblies = new string[] {
                                    typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Hchb.Services.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Hchb.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Patients.Services.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Patients.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Requests.Services.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Requests.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Documents.Services.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Documents.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Application.Services.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Application.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Reporting.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Reporting.Services.Generation.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Notifications.Services.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.Notifications.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                    typeof(SutureHealth.AspNetCore.Identity.HostingStartup).Assembly.GetName().Name,
                              };
            webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
            webBuilder.ConfigureAppConfiguration((host, config) => config.AddDefaultConfigurations().Build())
            .ConfigureServices((context, services) =>
            {
                services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());
                services.AddScoped<ITracingService, NullTracingService>();
            });
        }).Build();
        HchbService = host.Services.GetRequiredService<IHchbServiceProvider>();        
    }

    public async Task FunctionHandler(KinesisEvent kinesisEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"Beginning to process {kinesisEvent.Records.Count} records...");

        try
        {
            foreach (var record in kinesisEvent.Records.Select(record => record.Kinesis))
            {
                string recordData = GetRecordContents(record);
                context.Logger.LogInformation(recordData);
                context.Logger.LogInformation(record.PartitionKey);
                string messageType = record.PartitionKey.ToUpper();

                string result = "";
                int logId = -1;

                Hl7 type = Hl7.NOTDEFINED;
                if (Enum.TryParse<Hl7>(messageType, true, out type))
                {
                    switch (type)
                    {
                        case Hl7.ADT:
                            logId = HchbService.LogMessage(messageType, recordData);
                            if (HchbService.IsProcessedMessage(logId) != true)
                            {
                                result = await HchbService.ProcessAdtMessage(recordData, logId);
                                HchbService.LogReason(logId, result);
                            }
                            break;

                        case Hl7.MDM:
                            logId = HchbService.LogMessage(messageType, recordData);
                            if (HchbService.IsProcessedMessage(logId) != true)
                            {
                                result = await HchbService.ProcessMdmMessage(recordData, logId);
                                HchbService.LogReason(logId, result);
                            }
                            break;

                        case Hl7.ORU:
                            result = await HchbService.ProcessOruMessage(recordData);
                            break;
                    }
                }
                else
                {
                    await HchbService.SendNotificationAsync("Invalid Message received", recordData);
                    result = "It is not supported HL7 Message.";
                }

                context.Logger.LogInformation(result);
            }

        }
        catch(Exception e)
        {
            context.Logger.LogError(e.StackTrace);
        }

        context.Logger.LogInformation("Stream processing complete.");
    }

    private string GetRecordContents(KinesisEvent.Record streamRecord)
    {
        using (var reader = new StreamReader(streamRecord.Data, Encoding.ASCII))
        {
            return reader.ReadToEnd();
        }
    }
}