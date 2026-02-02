using SutureHealth.Hchb.Services.Testing.MessageBuilder;
using SutureHealth.Patients.Services;
using SutureHealth.Patients;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SutureHealth.Diagnostics;
using SutureHealth.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Patients.Services.SqlServer;
using NUnit.Framework;
using SutureHealth.Hchb.Services.Testing.Utility;
using SutureHealth.Hchb.Services.Testing.Model.Address;
using Newtonsoft.Json;
using SutureHealth.Hchb.Services.Testing.Model.Patient;
using Amazon.S3.Model;
using Kendo.Mvc.UI;
using SutureHealth.Hchb.Services.Testing.Mock;
using Kendo.Mvc.Extensions;

namespace SutureHealth.Hchb.Services.Testing
{


    [TestFixture]
    public class SlachChannelFeedbackTest
    {
        private static int organizationId = 2000;
        protected ADTMessageBuilder? adtMessageBuilder;
        protected PatientDbContext? patientDbContext;
        protected IPatientServicesProvider? patientServicesProvider;
        protected IHchbServiceProvider? hchbServiceProvider;
        protected Application.Organization? organization;

        [SetUp]
        public void Setup()
        {

            (patientServicesProvider, patientDbContext, hchbServiceProvider) = CreateServicesProvider();

            organization = new Application.Organization
            {
                OrganizationId = organizationId
            };
        }

        [TearDown]
        public void Clean()
        {
            patientDbContext?.SaveChanges();
            patientDbContext?.Dispose();
        }


        [Category("Integration")]
        [Test]
        public async Task SendInvalidFacilityShouldSuccessfullyCreateNewSlackMessage()
        {
            //Init
            var mirthMessageModel = GetRandomAdtMesage(new string[] { "ssn" }, new string[] { "externalId", "admissionId", "EpisodeId" });
            mirthMessageModel.MessageType = "A04";
            string serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            string returnstring = string.Empty;
            //Act
            try
            {
                
                Assert.IsNotNull(hchbServiceProvider);
                if (hchbServiceProvider != null)
                {
                    returnstring = await hchbServiceProvider.ProcessAdtMessage(serializedMessage); 
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Send Message failed due to: ", e.Message);
            }

            //Asset       
            Assert.IsTrue(returnstring.IsCaseInsensitiveEqual("Patient's facility is not exist."));
            
        }
                
        private static (IPatientServicesProvider patientServicesProvider,
                PatientDbContext patientDbContext,
                IHchbServiceProvider hchbServiceProvider ) CreateServicesProvider()
        {

            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath().Load();
            var applicationHost = Host.CreateDefaultBuilder()
                                  .ConfigureWebHostDefaults(webBuilder =>
                                  {
                                      var assemblies = new string?[] {
                                          typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Application.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Application.Services.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Notifications.Services.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Notifications.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Providers.Services.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Providers.Services.SqlServer.HostingStartup).Assembly.GetName().Name,


                                          typeof(SutureHealth.Hchb.Services.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Hchb.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Patients.Services.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Patients.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Requests.Services.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Requests.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Documents.Services.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Documents.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Reporting.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Reporting.Services.Generation.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.AspNetCore.Identity.HostingStartup).Assembly.GetName().Name,
                                      };

                                      webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
                                      webBuilder.ConfigureAppConfiguration((host, config) =>
                                      {
                                          config.AddDefaultConfigurations(runtimeEnvironment: "ci")
                                                  .Build();
                                      })
                                      .ConfigureServices((context, services) =>
                                      {
                                          services.AddScoped<ITracingService, NullTracingService>();
                                          services.AddDbContext<PatientDbContext, SqlServerPatientDbContext>(options =>
                                          {
                                              var configuration = context.Configuration;
                                              var connString = new global::Microsoft.Data.SqlClient.SqlConnectionStringBuilder()
                                              {
                                                  DataSource = DotNetEnv.Env.GetString("SqlDatabase__DataSource"),
                                                  UserID = DotNetEnv.Env.GetString("SqlDatabase__UserID"),
                                                  Password = DotNetEnv.Env.GetString("SqlDatabase__Password"),
                                                  InitialCatalog = DotNetEnv.Env.GetString("SqlDatabase__InitialCatalog"),
                                                  ApplicationName = nameof(PatientServicesProvider),
                                                  ConnectTimeout = 5,
                                                  MinPoolSize = 5,
                                                  Pooling = true,
                                                  Encrypt = true,
                                                  TrustServerCertificate = true
                                              };
                                              options.UseSqlServer(connString.ToString())
                                                  .EnableDetailedErrors()
                                                  .EnableSensitiveDataLogging();
                                          }, ServiceLifetime.Transient);
                                          services.AddScoped<IPatientServicesProvider, PatientServicesProvider>();

                                      });
                                  })
                                  .Build();

            var patientServicesProvider = applicationHost.Services.GetRequiredService<IPatientServicesProvider>();
            var patientDbContext = applicationHost.Services.GetRequiredService<PatientDbContext>();
            var hchbServiceProvider = applicationHost.Services.GetRequiredService<IHchbServiceProvider>();
            return (patientServicesProvider, patientDbContext, hchbServiceProvider);
        }

        private MirthAdtMessageModel GetRandomAdtMesage(IEnumerable<string>? identifiers = null, IEnumerable<string>? hchbIdentifiers = null)
        {
            MirthAdtMessageModel mirthMessageModel = new MirthAdtMessageModel();
            mirthMessageModel.controlId = DateTime.Now.ToString("yyyyMMdd") + "R" + Utilities.GetRandomDecimalString(12);
            PatientModelMock patient = new PatientModelMock();
            patient.FirstName = Utilities.GetRandomNameOrFamilyName("FirstName");
            patient.LastName = Utilities.GetRandomNameOrFamilyName("LastName");
            patient.birthDate = Utilities.GetRandomDateTime(new DateTime(1950, 01, 01)).ToString("yyy-MM-dd");

            GenderType gender = Utilities.GetRandomEnumElement<GenderType>();
            if (gender == GenderType.M) patient.Gender = "M";
            else patient.Gender = "F";

            AddressBuilder addressBuilder = new AddressBuilder();
            addressBuilder.Build();
            patient.AddressLine1 = addressBuilder.GetAddress().StreetAddress;
            patient.City = addressBuilder.GetAddress().City;
            patient.State = addressBuilder.GetAddress().State;
            patient.PostalCode = addressBuilder.GetAddress().ZipCode;
            patient.identifiers = new List<IdentifierModelMock>();
            mirthMessageModel.Patient = patient;

            mirthMessageModel.BranchCode = "16";//Utilities.GetShortListRandomBranch().Code;
            
            if(identifiers != null)
            foreach (var item in identifiers)
            {
                if (item == "ssn")
                {
                    string ssn = Utilities.GetRandomSSN();
                    patient.identifiers.Add(new IdentifierModelMock() { Type = "ssn", Value = ssn });
                    patient.identifiers.Add(new IdentifierModelMock() { Type = "ssn4", Value = ssn.Substring(7, 4) });

                    continue;
                }
                if (item == "ssn4")
                {
                    patient.identifiers.Add(new IdentifierModelMock() { Type = "ssn4", Value = Utilities.GetRandomSSN().Substring(7, 4) });
                    continue;
                }
                if (item == "mbi")
                {
                    patient.identifiers.Add(new IdentifierModelMock()
                    {
                        Type = "mbi",
                        Value = Utilities.GetRandomMBI()
                    });
                    patient.identifiers.Add(new IdentifierModelMock()
                    {
                        Type = "has-medicare",
                        Value = "True"
                    });
                    continue;
                }
                if (item == "medicaid-number")
                {
                    patient.identifiers.Add(new IdentifierModelMock() { Type = "medicaid-number", Value = Utilities.GetRandomDecimalString(9) });
                    patient.identifiers.Add(new IdentifierModelMock() { Type = "has-medicare", Value = "True" });
                    continue;
                }
                if (item == "medicaid-state")
                {
                    patient.identifiers.Add(new IdentifierModelMock() { Type = "medicaid-state", Value = "TE" });
                    continue;
                }
                if (item == "has-private-insurance")
                {
                    Random rnd = new Random();
                    patient.identifiers.Add(new IdentifierModelMock() { Type = "has-private-insurance", Value = rnd.Next(0, 10) > 5 ? "True" : "False" });
                    continue;
                }
                if (item == "has-self-pay")
                {
                    Random rnd = new Random();
                    patient.identifiers.Add(new IdentifierModelMock() { Type = "has-self-pay", Value = rnd.Next(0, 10) > 5 ? "True" : "False" });
                    continue;
                }
            }

            mirthMessageModel.HchbPatientModelMock = new HchbPatientModelMock()
            {
                admissionId = Utilities.GetRandomDecimalString(5),
                episodeId = Utilities.GetRandomDecimalString(5),
                externalId = string.Empty,
                hchbId = string.Empty,
                patientId = Utilities.GetRandomDecimalString(5),
                status = "CURRENT",
                physicianFirstName = Utilities.GetRandomNameOrFamilyName("FirstName"),
                physicianLastName = Utilities.GetRandomNameOrFamilyName("LastName"),
                physicianNpi = Utilities.GetRandomDecimalString(10),
            };

            if (hchbIdentifiers?.FirstOrDefault(i => i == "externalId") != null)
            {
                mirthMessageModel.HchbPatientModelMock.externalId = "ER-" + Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(4);
            }
            if (hchbIdentifiers?.FirstOrDefault(i => i == "admissionId") != null)
            {
                mirthMessageModel.HchbPatientModelMock.admissionId = Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(2);
            }
            if (hchbIdentifiers?.FirstOrDefault(i => i == "EpisodeId") != null)
            {
                mirthMessageModel.HchbPatientModelMock.episodeId = Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(2);
            }
            if (hchbIdentifiers?.FirstOrDefault(i => i == "PatientId") != null)
            {
                mirthMessageModel.HchbPatientModelMock.hchbId = "HC-" + Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(2);
            }

            return mirthMessageModel;
        }



    }
}
