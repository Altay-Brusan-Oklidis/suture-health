using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SutureHealth.Patients.Helpers;
using System;

[assembly: HostingStartup(typeof(SutureHealth.PatientAPI.Services.AdmitDischargeTransfer.Kno2.HostingStartup))]

namespace SutureHealth.PatientAPI.Services.AdmitDischargeTransfer.Kno2;

public class HostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IKno2ApiClient>((provider) =>
            {
                var kno2Configuration = context.Configuration.GetSection("Kno2");

                var httpClient = HttpClientHelper.CreateHttpClient(
                    baseUri: new Uri(kno2Configuration["BaseUri"]),
                    defaultAccept: "application/json",
                    clientId: kno2Configuration["ClientId"],
                    clientSecret: kno2Configuration["ClientSecret"],
                    appId: kno2Configuration["AppId"],
                    authUri: new Uri(kno2Configuration["AuthUri"], UriKind.Relative));

                return new Kno2ApiClient(httpClient);
            });
        });
    }
}
