using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SutureHealth.Notifications;
using SutureHealth.Notifications.Providers;
using SutureHealth.Notifications.Providers.TwilioSMS;
using SutureHealth.Notifications.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
//using WebApplicationFactory = SutureHealth.AspNetCore.Mvc.Testing.WebApplicationFactory<SutureHealth.AspNetCore.WebHost.Startup>;

namespace SutureHealth.AspNetCore.WebHost.Testing
{
    public abstract class IntegrationTestBase
    {
        protected HttpClient HttpClient { get; init; }
        protected Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Startup> ApplicationFactory { get; set; }

        protected Mock<IEmailNotificationProvider> EmailNotificationProvider { get; set; }
        protected Mock<ITelephoneNumberNotificationProvider> PhoneNotificationProvider { get; set; }

        protected IntegrationTestBase()
        {
            ApplicationFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices((context, services) =>
                    {
                        services.Configure<NotificationOptions>(opts => opts.QueueProcessing = false);

                        // don't send any mails out.
                        // services.AddSingleton<IEmailNotificationProvider, SimpleEmailServiceNotificationProvider>();

                        var emailProvider = new ServiceDescriptor(typeof(IEmailNotificationProvider), typeof(SimpleEmailServiceNotificationProvider), ServiceLifetime.Singleton);
                        services.Remove(emailProvider);
                        services.AddTransient<INotificationProvider>(services =>
                        {
                            var emailer = new Mock<IEmailNotificationProvider>();
                            emailer.SetupGet(e => e.Channel).Returns(Channel.Email);
                            emailer.Setup(e => e.CreateDestination(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                                   .Returns(string.Empty);
                            emailer.Setup(e => e.SendNotificationAsync(It.IsAny<NotificationStatus>()))
                                   .Returns(Task.CompletedTask);
                            EmailNotificationProvider = emailer;
                            return emailer.Object;
                        });

                        // don't send any mails out.
                        // services.AddSingleton<ITelephoneNumberNotificationProvider, TwilioSMSProvider>();
                        // services.AddSingleton<ITelephoneNumberNotificationProvider, TwilioTextToSpeechProvider>();

                        ServiceDescriptor telephoneDescriptor;
                        Func<Guid, Channel, IServiceProvider, ITelephoneNumberNotificationProvider> phoneProvider = (providerId, channel, services) =>
                        {
                            var phoner = new Mock<ITelephoneNumberNotificationProvider>();
                            phoner.SetupGet(e => e.ProviderId).Returns(providerId);
                            phoner.SetupGet(e => e.Channel).Returns(channel);
                            phoner.Setup(e => e.SendNotificationAsync(It.IsAny<NotificationStatus>()))
                                    .Returns(Task.CompletedTask);
                            PhoneNotificationProvider = phoner;
                            return phoner.Object;
                        };

                        telephoneDescriptor = new ServiceDescriptor(typeof(ITelephoneNumberNotificationProvider), typeof(TwilioSMSProvider), ServiceLifetime.Singleton);
                        services.Remove(telephoneDescriptor);
                        Guid sms = Guid.NewGuid();
                        services.AddTransient<INotificationProvider>(services => phoneProvider(sms, Channel.Sms, services));

                        telephoneDescriptor = new ServiceDescriptor(typeof(ITelephoneNumberNotificationProvider), typeof(TwilioTextToSpeechProvider), ServiceLifetime.Singleton);
                        services.Remove(telephoneDescriptor);
                        Guid tts = Guid.NewGuid();
                        services.AddTransient<INotificationProvider>(services => phoneProvider(tts, Channel.TextToSpeech, services));
                    });
                });

            //HttpClient = ApplicationFactory.CreateClient(new WebApplicationFactoryClientOptions
            //{
            //    BaseAddress = new Uri("https://app.dev.suturesign.com")
            //});
        }

        protected async Task AuthenticateAsync()
        {
            var response = await HttpClient.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new AspNetCore.Areas.Identity.Pages.Account.LoginModel.InputModel
            {
                //Username = "",
                //Password = ""
            }.ToDictionary<string>()));
        }
    }
}