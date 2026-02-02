using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SutureHealth.Diagnostics;
using SutureHealth.Extensions.Configuration;
using SutureHealth.Notifications.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SutureHealth.Notifications.Testing.Integration
{
    [TestClass]
    public class CreateNotificationQueueHandlerTests
    {
        private IHost ApplicationHost { get; set; }
        private INotificationService NotificationServices { get; set; }

        public TestContext TestContext { get; set; }

        public CreateNotificationQueueHandlerTests() 
        {
            ApplicationHost = Host.CreateDefaultBuilder()
                                  .ConfigureWebHostDefaults(webBuilder =>
                                  {
                                      var assemblies = new string[] {
                                          typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Notifications.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Notifications.Services.HostingStartup).Assembly.GetName().Name
                                      };

                                      webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
                                      webBuilder.ConfigureAppConfiguration((host, config) =>
                                      {
                                          config.AddJsonFile("appsettings.test.json")
                                                .AddDefaultConfigurations()
                                                .Build();
                                      })
                                      .ConfigureServices(services =>
                                      {
                                          services.AddScoped<ITracingService, NullTracingService>();
                                      });
                                  })
                                  .Build();

            this.NotificationServices = ApplicationHost.Services.GetRequiredService<INotificationService>();
        }

        [TestCategory("Integration")]
        [TestCategory("Development")]
        [TestMethod]
        public async Task ProcessIncomingQueue()
        {
            var queue = ApplicationHost.Services.GetRequiredService<IAmazonSQS>();
            var queueName = "https://sqs.us-east-1.amazonaws.com/267058832120/NotificationAPI-NotificationServiceProvider-CreateNotificationQueue-CI";

            var response = await queue.ReceiveMessageAsync(new Amazon.SQS.Model.ReceiveMessageRequest
            {
                QueueUrl = queueName
            });

            var function = new SutureHealth.Notifications.Services.Lambda.Function();
            if (response.Messages.Any())
            {
                await function.CreateNotificationQueueHandler(new SQSEvent()
                {
                    Records = response.Messages.Select(m => new SQSEvent.SQSMessage
                    {
                        Body = m.Body
                    }).ToList()
                });
            }
        }

        [TestCategory("Integration")]
        [TestCategory("Development")]
        [TestMethod]
        public async Task ProcessMessage()
        {
            var function = new SutureHealth.Notifications.Services.Lambda.Function();
            await function.CreateNotificationQueueHandler(new SQSEvent()
            {
                Records = new System.Collections.Generic.List<SQSEvent.SQSMessage> {
                    new SQSEvent.SQSMessage
                    {
                        Body = "42CDDFF1-A6ED-41AB-91C4-C453BEE85B84"
                    }
                }
            });
        }

        [TestCategory("Integration")]
        [TestCategory("Development")]
        [TestMethod]
        public void StringInterpolate()
        {

            //var callback = "https://ci.api.suture.health/v0.1/Notifications/{NotificationStatus.Id}/callback/{NotificationStatus.ProviderId}";
            var configuration = ApplicationHost.Services.GetRequiredService<IConfiguration>();
            var callback = configuration["NotificationServicesProvider::CallbackUrl"];
            var status = new NotificationStatus 
            { 
                Id = Guid.NewGuid(),
                ProviderId = Guid.NewGuid()
            };
            var s = callback.Interpolate(status);
            TestContext.WriteLine(s);
        }
    }
}
