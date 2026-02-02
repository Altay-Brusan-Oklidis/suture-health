using System.Globalization;
using NUnit.Framework;
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
using SutureHealth.Hchb.Services.Testing.Utility;
using SutureHealth.Hchb.Services.Testing.Model.Address;
using Newtonsoft.Json;
using SutureHealth.Hchb.Services.Testing.Model.Patient;
using Amazon.S3.Model;
using SutureHealth.Hchb.Services.SqlServer;
using SutureHealth.Hchb.Services.Testing.Mock;
using SutureHealth.Hchb.JsonConverters;
using System.Runtime.Serialization;
using Amazon.Kinesis.Model;
// ReSharper disable MethodHasAsyncOverload
// ReSharper disable UseAwaitUsing

namespace SutureHealth.Hchb.Services.Testing
{
    public class HchbServiceProviderTest
    {
        public const string schema = "HCHB_CommonSpirit";
        public const string database = "SutureSignWeb-CI";
        private const string dateTimeFormatString = "yyyyMMddHHmmss";
        private static int organizationId = 2000;
        ADTMessageBuilder adtMessageBuilder;
        protected PatientDbContext patientDbContext;
        protected HchbWebDbContext hchbWebContext;
        protected IPatientServicesProvider patientServicesProvider;
        protected IHchbServiceProvider hchbServiceProvider;
        protected Application.Organization organization;

        [SetUp]
        public void Setup()
        {

            (patientServicesProvider, patientDbContext, hchbServiceProvider, hchbWebContext) = CreateServicesProvider();

            organization = new Application.Organization
            {
                OrganizationId = organizationId
            };
        }

        private static (IPatientServicesProvider patientServicesProvider,
                PatientDbContext patientDbContext,
                IHchbServiceProvider hchbServiceProvider,
                HchbWebDbContext hchbWebDbContext) CreateServicesProvider()
        {
            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath().Load();
            var applicationHost = Host.CreateDefaultBuilder()
                                  .ConfigureWebHostDefaults(webBuilder =>
                                  {
                                      var assemblies = new string[] {
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
                                          config.AddJsonFile("appsettings.test.json")
                                                .AddDefaultConfigurations(runtimeEnvironment: "ci")
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
            var hchbWebDbContext = applicationHost.Services.GetRequiredService<HchbWebDbContext>();
            var patientDbContext = applicationHost.Services.GetRequiredService<PatientDbContext>();
            var hchbServiceProvider = applicationHost.Services.GetRequiredService<IHchbServiceProvider>();
            return (patientServicesProvider, patientDbContext, hchbServiceProvider, hchbWebDbContext);
        }

        [TearDown]
        public void Clean()
        {
            patientDbContext.Dispose();
        }

        protected MirthAdtMessageModel GetAdtMessage(string hchbId = "HCHB-Patient-123", List<IdentifierModelMock>? identifiers = null)
        {
            identifiers ??= new List<IdentifierModelMock>
            {
                new()
                {
                    Type = "ssn4",
                    Value = "5549"
                }
            };

            return new MirthAdtMessageModel
            {
                MessageType = "A04",
                RawFilename = "ADT/raw/patient123.txt",
                JsonFilename = "ADT/processed/patient123.json",
                Patient = new PatientModelMock
                {
                    identifiers = identifiers,
                    birthDate = (DateTime.UtcNow - TimeSpan.FromDays(365 * 60)).ToString("yyy-MM-dd"),
                    City = "Atlanta",
                    Gender = "Male",
                    Phonenumber = new PhoneNumber
                    {
                        home = "205-555-5555"
                    },
                    State = "GA",
                    AddressLine1 = "1234 Fake Street",
                    FirstName = "Ray",
                    LastName = "Cyrus",
                    PostalCode = "30316",
                    MiddleName = "R",
                },
                BranchCode = "015",
                HchbPatientModelMock = new HchbPatientModelMock
                {
                    admissionId = "1",
                    episodeId = "100",
                    hchbId = hchbId,
                    physicianNpi = "9545455542",
                    physicianFirstName = "Bill",
                    physicianLastName = "Hancock"
                }
            };
        }

        protected Adt GetAdtMessage(ADT subType, string hchbId = "HchbPatientId-1234", string episodId="10002", List<PatientIdentifier>? identifiers = null)
        {
            identifiers ??= new List<PatientIdentifier>
            {
                new()
                {
                    Type = "ssn",
                    Value = "120122550"
                }
            };

            return new Adt
            {
                Type = subType,
                MessageControlId= "2023091508541804000059888602",
                RawFileName = "ADT/raw/patient123.txt",
                JsonFileName = "ADT/processed/patient123.json",
                Patient = new Patient
                {
                    FirstName = "Kelli",
                    MiddleName = "A",
                    LastName = "Wilder",
                    Identifiers = identifiers,
                    Birthdate = (DateTime.UtcNow - TimeSpan.FromDays(365 * 60)),
                    Addresses= new List<PatientAddress> 
                    { 
                        new() 
                        {
                            City = "Atlanta",
                            StateOrProvince = "GA",
                            Line1 = "1234 Fake Street",
                            PostalCode = "30321",
                        } 
                    },                    
                    Gender = Gender.Male,
                    Phones = new List<PatientPhone> 
                    {
                        new()
                        {
                            Type= ContactType.HomePhone,
                            Value = "2055555555"
                        }
                    },
                },
                BranchCode = "015",
                HchbPatient = new HchbPatientWeb
                {
                    PatientId = 1013003,
                    
                    EpisodeId = episodId,
                    HchbPatientId = hchbId,
                    IcdCode= "U07.1"
                }
            };
        }

        protected Mdm ConvertAdtToMdm(Adt adt)
        {
            Mdm mdm = new Mdm();
            
            mdm.Patient = new();
            mdm.Transaction = new();
            mdm.Signer = new();
            mdm.Sender = new();
            mdm.Status = "CURRENT";
            mdm.Patient.FirstName = adt.Patient.FirstName;
            mdm.Patient.LastName = adt.Patient.LastName;
            mdm.Patient.MiddleName = adt.Patient.MiddleName;
            mdm.Patient.Ids = new List<IIdentifier>()
                    {
                        new PatientIdentifier()
                        {
                            Type=adt.Patient.Identifiers.First().Type,
                            Value=adt.Patient.Identifiers.First().Value,
                        }
                    };
            mdm.Patient.Birthdate = adt.Patient.Birthdate;
            mdm.Patient.Gender = adt.Patient.Gender;
            mdm.Patient.AddressLine1 = adt.Patient.Addresses.FirstOrDefault()?.Line1;
            mdm.Patient.AddressLine2 = adt.Patient.Addresses.FirstOrDefault()?.Line2;
            mdm.Patient.PostalCode = adt.Patient.Addresses?.FirstOrDefault()?.PostalCode;
            mdm.Patient.City = adt.Patient.Addresses?.FirstOrDefault()?.City;
            mdm.Patient.RequestSource = RequestSource.HCHB;
            mdm.Jsonfilename = adt.JsonFileName;
            mdm.Rawfilename = adt.RawFileName;
            mdm.MessageControlId = adt.MessageControlId;

            mdm.Transaction.EpisodeId = adt.HchbPatient.EpisodeId;
            mdm.Transaction.HchbPatientId = adt.HchbPatient.HchbPatientId;
            mdm.Transaction.OrderDate = DateTime.Now;
            mdm.Transaction.AdmitDate = new DateTime(2022, 05, 25);
            mdm.Transaction.SendDate = new DateTime(2022, 05, 28);
            mdm.Transaction.RequestId = 100005;
            mdm.Transaction.OrderNumber = "4875706";
            mdm.Transaction.OrderDate = new DateTime(2023, 5, 10);
            mdm.Transaction.ObservationId = "2";
            mdm.Transaction.ObservationText = "POCU";
            mdm.Transaction.PatientType = "HOME HEALTH";
            mdm.Transaction.FileName = "MDM/pdf/040_2_4875706.pdf";
            mdm.Transaction.AdmissionType = "NEW ADMISSION";
            
            mdm.Signer.Npi = 1649944919;
            mdm.Signer.FirstName = "PEGGY";
            mdm.Signer.LastName = "GILDENBLATT";
            mdm.Signer.BranchCode = adt.BranchCode;
            mdm.Sender.BranchCode = adt.BranchCode;
            
            return mdm;
        }


        protected string AdtToString(Adt adt) 
        {
            JsonConverter[] converters = new JsonConverter[] { new AdtConverter(), new PatientConverter(), new HchbPatientConverter() };
            string adtJsonMessage = JsonConvert.SerializeObject(adt, converters);
            return adtJsonMessage;
        }
        protected string MdmToString(Mdm mdm)
        {
            JsonConverter[] converters = new JsonConverter[] { new MdmConverter(), new PatientMatchingRequestConverter(), new TransactionConverter(), new SignerConverter(), new SenderConverter() };
            string mdmJsonMessage = JsonConvert.SerializeObject(mdm, converters);
            return mdmJsonMessage;
        }


        /// <summary>
        /// This class contains tests designed to evaluate the JSON messages produced by using the Hchb core JsonConverters.                
        /// The purpose of these tests is to verify the correctness and validity of these messages for the HchbServiceProvider.
        /// <b> Please note that the Hchb workflow itself is not the focus of these tests.</b>
        /// </summary>

        class GivenHchbPatient : HchbServiceProviderTest 
        {
            private Adt adt;
            private string adtResult;
            private HchbPatientWeb patient;

            [SetUp]
            public async Task BeforeEach()
            {
                base.Setup();           
                adt= GetAdtMessage(ADT.A04);
                using (var txn = patientDbContext.Database.BeginTransaction())
                {
                    string adtJsonMessage = AdtToString(adt);
                    adtResult = await hchbServiceProvider.ProcessAdtMessage(adtJsonMessage);
                    txn.Commit();
                }
                try
                {
                    patient = hchbWebContext.HchbPatients.FirstOrDefault(x => x.HchbPatientId == adt.HchbPatient.HchbPatientId);

                }
                catch (Exception e)
                {

                    throw;
                }
            }

            [TearDown]
            public void AfterEach()
            {
                using (var txn = hchbWebContext.Database.BeginTransaction())
                {
                    hchbWebContext.Database.ExecuteSqlRaw(
                      "DELETE FROM [dbo].[PatientPhone] WHERE PatientId = {0}", patient.PatientId);

                    hchbWebContext.Database.ExecuteSqlRaw(
                      "DELETE FROM [" + schema + "].[HCHB_Patients] WHERE PatientId = {0}", patient.PatientId);

                    hchbWebContext.Database.ExecuteSqlRaw(
                        "DELETE FROM [" + schema + "].[HCHB_Transactions] WHERE HCHB_PatientId = {0}", patient.HchbPatientId);
                    txn.Commit();
                }

                using (var txn = patientDbContext.Database.BeginTransaction())
                {
                    patientDbContext.Database.ExecuteSqlRaw(
                           "DELETE FROM [" + database + "].[dbo].[PatientAddress] WHERE PatientId = {0}", patient.PatientId);

                    patientDbContext.Database.ExecuteSqlRaw(
                        "DELETE FROM ["+ database + "].[dbo].[Patients] WHERE PatientId = {0}",
                        patient.PatientId);

                    txn.Commit();
                }              

            }


            [Test]
            [Category("Integration")]
            public async Task CreateAdtMessageToUpdatePatientStatusToAdmitThenCallServiceProviderWithTheMessageShouldSuccess() 
            {
                //Init
                adt.Type = ADT.A01;
                string json = AdtToString(adt);
                
                // Act
                string result = await hchbServiceProvider.ProcessAdtMessage(json);

                // Assert
                Assert.IsTrue(result == Messages.ADMIT_SUCCESS);                
            }

            [Test]
            [Category("Integration")]
            public async Task CreateAdtMessageToUpdatePatientInfoThenCallServiceProviderWithTheMessageShouldSuccess()
            {
                //Init
                adt.HchbPatient.IcdCode = "U80.8";
                adt.Patient.FirstName="Bill";
                adt.Patient.LastName = "Carson";
                adt.Type = ADT.A08;
                string json = AdtToString(adt);
                
                // Act
                string result = await hchbServiceProvider.ProcessAdtMessage(json);

                // Assert
                Assert.IsTrue(result == Messages.UPDATE_SUCCESS);
            }

            [Test]
            [Category("Integration")]
            public async Task CreateAdtMessageToDischargePatientThenCallServiceProviderWithTheMessageShouldSuccess()
            {
                //Init
                adt.Type = ADT.A03;
                string json = AdtToString(adt);

                // Act
                string result = await hchbServiceProvider.ProcessAdtMessage(json);

                // Assert
                Assert.IsTrue(result == Messages.DISCHARGE_SUCCESS);
            }

            [Test]
            [Category("Integration")]
            public async Task CreateAdtMessageToCancelPatientThenCallServiceProviderWithTheMessageShouldSuccess()
            {
                //Init
                adt.Type = ADT.A11;
                string json = AdtToString(adt);

                // Act
                string result = await hchbServiceProvider.ProcessAdtMessage(json);

                // Assert
                Assert.IsTrue(result == Messages.CANCLE_SUCCESS);
            }

            [Test]
            [Category("Integration")]
            public async Task CreateMdmMessageThenCallServiceProviderWithTheMessageShouldSuccess()
            {
                //Init
                adt.Type = ADT.A01;
                string json = AdtToString(adt);
                string result = await hchbServiceProvider.ProcessAdtMessage(json);
                Assert.IsTrue(result == Messages.ADMIT_SUCCESS);

                Mdm mdm = new Mdm();
                mdm.Patient = new();                
                mdm.Transaction = new();
                mdm.Signer = new();
                mdm.Sender=new();
                mdm.Patient.FirstName= adt.Patient.FirstName;
                mdm.Patient.LastName = adt.Patient.LastName;
                mdm.Patient.MiddleName= adt.Patient.MiddleName;
                mdm.Patient.Ids =
                    new List<IIdentifier>()
                    {
                        new PatientIdentifier()
                        {
                            Type=adt.Patient.Identifiers.First().Type,
                            Value=adt.Patient.Identifiers.First().Value,
                        }
                    };
                mdm.Patient.Birthdate = adt.Patient.Birthdate;
                mdm.Patient.Gender = adt.Patient.Gender;
                mdm.Patient.AddressLine1 = adt.Patient.Addresses.FirstOrDefault()?.Line1;
                mdm.Patient.AddressLine2 = adt.Patient.Addresses.FirstOrDefault()?.Line2;
                mdm.Patient.PostalCode = adt.Patient.Addresses?.FirstOrDefault()?.PostalCode;
                mdm.Patient.City = adt.Patient.Addresses?.FirstOrDefault()?.City;
                mdm.Patient.RequestSource = RequestSource.HCHB;
                mdm.Jsonfilename = adt.JsonFileName;
                mdm.Rawfilename = adt.RawFileName;
                mdm.MessageControlId = adt.MessageControlId;
                mdm.Transaction.EpisodeId = adt.HchbPatient.EpisodeId;
                mdm.Transaction.HchbPatientId = adt.HchbPatient.HchbPatientId;
                mdm.Transaction.OrderDate = DateTime.Now;
                mdm.Transaction.AdmitDate = new DateTime(2022, 05, 25);
                mdm.Transaction.SendDate = new DateTime(2022, 05, 28);

                mdm.Transaction.RequestId = 100005;
                mdm.Signer.Npi = 1649944919;
                mdm.Signer.FirstName = "PEGGY";
                mdm.Signer.LastName = "GILDENBLATT";
                mdm.Signer.BranchCode = adt.BranchCode;
                mdm.Sender.BranchCode = adt.BranchCode;
                    
                json =  MdmToString(mdm);
                
                // Act
                result = await hchbServiceProvider.ProcessMdmMessage(json);

                // Assert
                Assert.IsTrue(result == Messages.TEMPLATE_NOTEXIST_ERROR ||
                              result == Messages.SENDER_NOTEXIST_ERROR ||
                              result == Messages.SIGNER_NOTEXIST_ERROR ||
                              result == Messages.SIGNER_FACILITY_NOTEXIST_ERROR ||
                              result == Messages.DOCUMENT_SEND ||
                              result == Messages.PATIENT_NONMATCH);
            }

            [Test]
            [Category("Integration")]
            public async Task CreateMdmMessageWithCompleteTransactionSectionThenCallServiceProviderWithTheMessageShouldSuccess()
            {
                //Init
                adt.Type = ADT.A01;
                string json = AdtToString(adt);
                string result = await hchbServiceProvider.ProcessAdtMessage(json);
                Assert.IsTrue(result == Messages.ADMIT_SUCCESS);
               
                Mdm mdm;
                mdm = ConvertAdtToMdm(adt);
                json = MdmToString(mdm);

                // Act
                result = await hchbServiceProvider.ProcessMdmMessage(json);

                // Assert
                Assert.IsTrue(result == Messages.TEMPLATE_NOTEXIST_ERROR ||
                              result == Messages.SENDER_NOTEXIST_ERROR ||
                              result == Messages.SIGNER_NOTEXIST_ERROR ||
                              result == Messages.SIGNER_FACILITY_NOTEXIST_ERROR ||
                              result == Messages.DOCUMENT_SEND ||
                              result == Messages.PATIENT_NONMATCH);
            }


            [Test]
            [Category("Integration")]
            public void CreateAdtMessageThenCallServiceProviderToLogTheMessageShouldSuccess()
            {
                //Init
                adt.Type = ADT.A04;
                string json = AdtToString(adt);
                
                // Act
                int id = hchbServiceProvider.LogMessage("ADT", json);

                // Assert
                Assert.IsTrue(id > 0);
            }

            [Test]
            [Category("Integration")]
            public async Task CreateSimilarHighRiskPatientShouldCreateMessageOnSlackChannel()
            {

                // Init
                adt.Patient.FirstName = "Corie";
                adt.Patient.LastName = "Coral";
                adt.Patient.Birthdate = (DateTime.UtcNow - TimeSpan.FromDays(365 * 60));
                adt.Patient.Addresses = new List<PatientAddress>
                    {
                        new()
                        {
                            City = "Atlanta",
                            StateOrProvince = "GA",
                            Line1 = "1234 Fake Street",
                            PostalCode = Utilities.GetRandomDecimalString(5),
                        }
                    };

                adt.HchbPatient.PatientId= int.Parse(Utilities.GetRandomDecimalString(5));
                adt.HchbPatient.EpisodeId= Utilities.GetRandomAlphabeticString(5);

                using (var txn = patientDbContext.Database.BeginTransaction())
                {
                    string adtJsonMessage = AdtToString(adt);
                    adtResult = await hchbServiceProvider.ProcessAdtMessage(adtJsonMessage);
                    txn.Commit();
                }

                Assert.IsTrue(adtResult == "The patient already exists.");

            }
        }

        class GivenA04Message : HchbServiceProviderTest
        {
            const string externalId = "HCHB-Patient-123";

            private MirthAdtMessageModel adtMessage;
            private string adtResult;
            private HchbPatientWeb patient;

            [SetUp]
            public async Task BeforeEach()
            {
                base.Setup();
                var identifiers = new List<IdentifierModelMock>
                {
                    new()
                    {
                        Type = "ssn4",
                        Value = "5544"
                    }
                };

                adtMessage = new MirthAdtMessageModel
                {
                    MessageType = "A04",
                    RawFilename = "ADT/raw/patient123.txt",
                    JsonFilename = "ADT/processed/patient123.json",
                    Patient = new PatientModelMock
                    {
                        identifiers = identifiers,
                        birthDate = (DateTime.UtcNow - TimeSpan.FromDays(365 * 60)).ToString("yyy-MM-dd"),
                        City = "Atlanta",
                        Gender = "Male",
                        Phonenumber = new PhoneNumber
                        {
                            home = "205-555-5555"
                        },
                        State = "GA",
                        AddressLine1 = "1234 Fake Street",
                        FirstName = "Ray",
                        LastName = "Cyrus",
                        PostalCode = "30316",
                        MiddleName = "R",
                    },
                    BranchCode = "015",
                    HchbPatientModelMock = new HchbPatientModelMock
                    {
                        admissionId = "1",
                        episodeId = "100",
                        hchbId = externalId,
                        physicianNpi = "9545455542",
                        physicianFirstName = "Bill",
                        physicianLastName = "Hancock"
                    }
                };

                using (var txn = patientDbContext.Database.BeginTransaction())
                {
                    adtResult = await hchbServiceProvider.ProcessAdtMessage(JsonConvert.SerializeObject(adtMessage));
                    txn.Commit();
                }

                patient = hchbWebContext.HchbPatients.Single(x => x.HchbPatientId == externalId);
            }

            [TearDown]
            public void AfterEach()
            {
                using var txn = patientDbContext.Database.BeginTransaction();
                patientDbContext.Database.ExecuteSqlRaw(
                    "DELETE FROM \"dbo\".\"HCHB_Patient\" WHERE PatientId = {0}",
                    patient.PatientId);

                patientDbContext.Database.ExecuteSqlRaw(
                    "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"HCHB_Patient_Transaction\" WHERE PatientId = {0}",
                    patient.PatientId);

                patientDbContext.Database.ExecuteSqlRaw(
                    "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"PatientAddress\" WHERE PatientId = {0}",
                    patient.PatientId);

                patientDbContext.Database.ExecuteSqlRaw(
                    "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"Patients\" WHERE PatientId = {0}",
                    patient.PatientId);

                txn.Commit();
            }

            [Test]
            [Category("Integration")]
            public Task ShouldHaveCorrectValuesInDatabase()
            {
                Assert.AreEqual(adtMessage?.HchbPatientModelMock?.episodeId, patient.EpisodeId,
                    "Episode ID Should match");
                Assert.AreEqual(Status.PENDDING, patient.Status, "Patient Status should match");//"Admission ID should match");
                return Task.CompletedTask;
            }
            class WhenPatientExistsGivenPhysicianOrder : HchbServiceProviderTest
            {
                protected MirthAdtMessageModel adtMessage;
                protected MirthMdmMessageModel mdmMessage;
                protected HchbPatientWeb patient;
                protected HchbTransaction transaction;
                protected int templateId;

                private const string hchbPatientId = "HCHB-Patient-HchbServiceProviderTest";
                private const string DateTimeFormat = "yyyyMMddHHmmss";


                [SetUp]
                public async Task BeforeEach()
                {
                    adtMessage = GetAdtMessage(hchbPatientId);
                    using (var txn = hchbWebContext.Database.BeginTransaction())
                    {
                        await hchbServiceProvider.ProcessAdtMessage(JsonConvert.SerializeObject(adtMessage));
                        txn.Commit();
                    }

                    mdmMessage = GetMdmMessage(adtMessage.HchbPatientModelMock.hchbId);

                    using (var txn = hchbWebContext.Database.BeginTransaction())
                    {
                        await hchbServiceProvider.ProcessMdmMessage(JsonConvert.SerializeObject(mdmMessage));
                        txn.Commit();
                    }

                    patient = hchbWebContext.HchbPatients
                        .Single(x => x.HchbPatientId == hchbPatientId);


                    transaction = hchbWebContext.HchbTransactions
                            .Single(x => x.OrderNumber == mdmMessage.Transaction.OrderNumber);

                    //Transaction = hchbWebDbContext.GetTransactionByOrderNumber(mdmMessage.Transaction.OrderNumber);

                    templateId = hchbWebContext.Database.SqlQueryRaw<int>(
                        "SELECT template FROM \"SutureSignWeb-CI\".\"dbo\".Requests r WHERE r.Id = {0}",
                        transaction.RequestId).ToList().Single();
                    Assert.AreNotEqual(0, templateId);
                }


                [TearDown]
                public async Task AfterEach()
                {
                    using (var txn = patientDbContext.Database.BeginTransaction())
                    {
                        patientDbContext.Database.ExecuteSqlRaw(
                            "DELETE FROM \"dbo\".\"HCHB_Patient\" WHERE PatientId = {0}",
                            patient.PatientId);

                        patientDbContext.Database.ExecuteSqlRaw(
                            "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"HCHB_Patient_Transaction\" WHERE PatientId = {0}",
                            patient.PatientId);

                        patientDbContext.Database.ExecuteSqlRaw(
                            "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"PatientAddress\" WHERE PatientId = {0}",
                            patient.PatientId);

                        patientDbContext.Database.ExecuteSqlRaw(
                            "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"Patients\" WHERE PatientId = {0}",
                            patient.PatientId);

                        patientDbContext.Database.ExecuteSqlRaw(
                            "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"Transactions\" WHERE OrderNumber = {0}",
                            transaction.OrderNumber);

                        patientDbContext.Database.ExecuteSqlRaw(
                            "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"Requests\" where Id = {0}",
                            transaction.RequestId);

                        if (templateId != 0)
                        {
                            patientDbContext.Database.ExecuteSqlRaw(
                                "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"Templates\" where TemplateId = {0}",
                                templateId);
                        }

                        txn.Commit();
                    }
                }

                [Test]
                [Category("Integration")]
                public void ShouldSaveCorrectlyToDatabase()
                {
                    Assert.AreEqual(mdmMessage.Transaction.AdmissionType, transaction.AdmissionType);
                    Assert.AreEqual(mdmMessage.Transaction.OrderNumber, transaction.OrderNumber);
                    Assert.AreEqual(mdmMessage.Transaction.OrderDate, transaction.OrderDate?.ToString(DateTimeFormat));
                    Assert.AreEqual(mdmMessage.Transaction.AdmitDate, transaction.AdmitDate?.ToString(DateTimeFormat));
                }

                [Test]
                [Category("Integration")]
                public async Task AfterAnotherPoWithSameIdShouldUpdateOriginalPo()
                {
                    var newMessage = GetMdmMessage(hchbPatientId);
                    newMessage.Signer.NPI = "1427070937";
                    using (var txn = hchbWebContext.Database.BeginTransaction())
                    {
                        await hchbServiceProvider.ProcessMdmMessage(JsonConvert.SerializeObject(newMessage));
                        txn.Commit();
                    }

                    var newTransaction = hchbWebContext.HchbTransactions
                            .Single(x => x.OrderNumber == newMessage.Transaction.OrderNumber);
                    Assert.AreEqual(newTransaction.SignerId, 3000010);
                    Assert.AreEqual(newTransaction.SignerFacilityId, 11005);
                }

                private MirthMdmMessageModel GetMdmMessage(string hchbId)
                {
                    return new MirthMdmMessageModel
                    {
                        Patient = adtMessage.Patient,
                        HchbPatient = new HchbPatientModel
                        {
                            AdmissionId = "999",
                            EpisodeId = "777",
                            ExternalId = "111",
                            PatientId = hchbId,
                            PhysicianNpi = "7222222247",
                            PhysicianFirstName = "Signer",
                            PhysicianLastName = "McSigner",
                        },
                        RawFilename = "TEST/mdm/raw/WhenPatientExistsGivenMdmMessage.txt",
                        JsonFilename = "TEST/mdm/processed/WhenPatientExistsGivenMdmMessage.json",
                        Sender = new PersonModelMock
                        {
                            BranchCode = "040",
                            FirstName = "Sender",
                            LastName = "McSender",
                            NPI = "9779797232"
                        },
                        Signer = new PersonModelMock
                        {
                            FirstName = "Signer",
                            LastName = "McSigner",
                            NPI = "1231231313"
                        },
                        Transaction = new TransactionModelMock
                        {
                            AdmissionType = "NEW ADMISSION",
                            AdmitDate = new DateTime(2022, 05, 25).ToString(DateTimeFormat),
                            EffectiveDate = new DateTime(2023, 6, 1).ToString(DateTimeFormat),
                            FileName = "MDM/pdf/16_TS_12345.pdf",
                            ObservationId = "6",
                            ObservationText = "PO",
                            OrderNumber = "12345",
                            PatientType = "HOME HEALTH",
                            OrderDate = new DateTime(2023, 6, 1).ToString(DateTimeFormat),
                            SendDate = new DateTime(2023, 6, 3).ToString(DateTimeFormat)
                        }
                    };
                }
            }

            // [Test]
            // [Category("Integration")]
            // public async Task HandleDuplicatedOrderMDMMessages()
            // {
            //     const string externalId = "HCHB-Patient-123";
            //     var identifiers = new List<IdentifierModelMock>
            //     {
            //         new()
            //         {
            //             Type = "ssn4",
            //             Value = "5544"
            //         }
            //     };
            //     
            //     var adtMessage = new MirthAdtMessageModel
            //     {
            //         MessageType = "A04",
            //         RawFilename = "ADT/raw/patient123.txt",
            //         JsonFilename = "ADT/processed/patient123.json",
            //         Patient = new PatientModelMock
            //         {
            //             identifiers = identifiers,
            //             BirthDate = (DateTime.UtcNow - TimeSpan.FromDays(365 * 60)).ToString("yyy-MM-dd"),
            //             City = "Atlanta",
            //             Gender = "Male",
            //             Phonenumber = new PhoneNumber
            //             {
            //                 home = "205-555-5555"
            //             },
            //             State = "GA",
            //             AddressLine1 = "1234 Fake Street",
            //             FirstName = "Ray",
            //             LastName = "Cyrus",
            //             PostalCode = "30316",
            //             MiddleName = "R",
            //         },
            //         BranchCode = "015",
            //         HchbPatientModelMock = new HchbPatientModelMock
            //         {
            //             admissionId = "1",
            //             EpisodeId = "100",
            //             PatientId = "123",
            //             externalId = externalId,
            //             physicianNpi = "9545455542",
            //             physicianFirstName = "Bill",
            //             physicianLastName = "Hancock"
            //         }
            //     };
            //
            //     string adtResult = await hchbServiceProvider.ProcessAdtMessage(JsonConvert.SerializeObject(adtMessage));
            //     Console.WriteLine(adtResult);
            //     Assert.IsNotNull(adtResult);
            //     
            //     var Patient = (hchbWebDbContext as SqlServerHchbDbContext).GetHchbPatients()
            //         .SingleOrDefault(Patient => Patient.externalId == externalId);
            //
            //     try
            //     {
            //         Assert.IsNotNull(Patient);
            //         
            //         Assert.AreEqual(adtMessage.HchbPatientModelMock.physicianFirstName, Patient.physicianFirstName);
            //
            //         var firstMdmMessage = GetRandomMirthMDMMesage();
            //         firstMdmMessage.Sender.BranchCode = "016";
            //         firstMdmMessage.Transaction.AdmissionType = "NEW ADMISSION";
            //         firstMdmMessage.Transaction.AdmitDate =
            //             (DateTime.UtcNow - TimeSpan.FromDays(-3)).ToString("yyyyMMddHHmmss");
            //         firstMdmMessage.Transaction.PatientType = "HOME HEALTH";
            //         firstMdmMessage.Patient = adtMessage.Patient;
            //         firstMdmMessage.HchbPatient = new HchbPatientModel
            //         {
            //             AdmissionId = "1",
            //             EpisodeId = "100",
            //             PatientId = "123",
            //             ExternalId = "HCHB-Patient-123",
            //             PhysicianNpi = "9545455542",
            //             PhysicianFirstName = "Bill",
            //             PhysicianLastName = "Hancock"
            //         };
            //
            //         var secondMdmMessage = firstMdmMessage;
            //         secondMdmMessage.Signer.NPI = "4545454541";
            //
            //         string firstResult =
            //             await hchbServiceProvider.ProcessMdmMessage(JsonConvert.SerializeObject(firstMdmMessage));
            //         string secondResult =
            //             await hchbServiceProvider.ProcessMdmMessage(JsonConvert.SerializeObject(firstMdmMessage));
            //         
            //         var Transaction = hchbWebDbContext.GetTransactionByOrderNumber(firstMdmMessage.Transaction.OrderNumber);
            //         Assert.IsNotNull(Transaction);
            //         Assert.AreEqual(firstMdmMessage.Transaction.AdmissionType, Transaction.AdmissionType);
            //         Assert.AreEqual(secondMdmMessage.Signer.NPI, secondMdmMessage.Signer.NPI);
            //
            //         Console.WriteLine(firstResult);
            //         Assert.AreEqual(Messages.DOCUMENT_SEND, firstResult);
            //         Assert.AreEqual(Messages.DOCUMENT_SEND, secondResult);
            //     }
            //     finally
            //     {
            //         using (var txn = patientDbContext.Database.BeginTransaction())
            //         {
            //             patientDbContext.Database.ExecuteSqlRaw(
            //                 "DELETE FROM \"dbo\".\"HCHB_Patient\" WHERE PatientId = {0}",
            //                 Patient.PatientId);
            //             
            //             patientDbContext.Database.ExecuteSqlRaw(
            //                 "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"HCHB_Patient_Transaction\" WHERE PatientId = {0}",
            //                 Patient.PatientId);
            //
            //             patientDbContext.Database.ExecuteSqlRaw(
            //                 "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"PatientAddress\" WHERE PatientId = {0}", 
            //                 Patient.PatientId);
            //             
            //             patientDbContext.Database.ExecuteSqlRaw(
            //                 "DELETE FROM \"SutureSignWeb-CI\".\"dbo\".\"Patients\" WHERE PatientId = {0}", 
            //                 Patient.PatientId);
            //             
            //             txn.Commit();
            //         }
            //     }
            // }
            //
            // [Test]
            // [Category("Integration")]
            // public async Task ReCreateNewAPatientWithChangingTheHchbIdentifiersShouldFail()
            // {
            //     // Init
            //     var mirthMessageModel = GetRandomMirthMesage(new string[] { "ssn4" }, new string[] { });
            //     mirthMessageModel.MessageType = "A04";
            //     string serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string firstResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Act            
            //     mirthMessageModel.HchbPatientModelMock = new HchbPatientModelMock()             
            //     {
            //         externalId = "ER-" + Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(4),
            //         admissionId = Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(2),
            //         EpisodeId = Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(2),
            //         HchbId = "HC-" + Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(2)
            //     };
            //     serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string secondResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //
            //     // Assert
            //     Assert.IsTrue(firstResult.Equals("Create the Patient successfully."));
            //     Assert.IsTrue(secondResult.Equals("Patient is already exists."));
            // }
            //
            // [Test]
            // [Category("Integration")]
            // public async Task CreateNewPatientWithInsurancePlanShouldSucceed()
            // {
            //     // Init
            //     var mirthMessageModel = GetRandomMirthMesage(new string[] { "ssn4","mbi" }, new string[] { "admissionId" });
            //     mirthMessageModel.MessageType = "A04";
            //     string serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     
            //     // Act            
            //     string firstResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //     
            //     // Assert
            //     Assert.IsTrue(firstResult.Equals("Create the Patient successfully."));
            //
            // }
            //
            // [Test]
            // [Category("Integration")]
            // public async Task CreateNewPatientWithA04ThenUpdatePatientWithA08MessageShouldUpdatePatient()
            // {
            //     // Init
            //     var mirthMessageModel = GetRandomMirthMesage(new string[] { "ssn" }, new string[] { "externalId", "admissionId", "EpisodeId" });
            //     mirthMessageModel.MessageType = "A04";
            //     string serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string createNewPatientResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Act
            //     mirthMessageModel.Patient.FirstName = Utilities.GetRandomNameOrFamilyName("FirstName");
            //     mirthMessageModel.MessageType = "A08";
            //     serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string updatePatientResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Assert
            //     Assert.IsTrue(createNewPatientResult.Equals("Create the Patient successfully."));
            //     Assert.IsTrue(updatePatientResult.Equals("Update the Patient successfully."));
            // }
            //
            // [Test]
            // [Category("Integration")]
            // public async Task CreateNewPatientWithA04ThenDischargeWithA03ShouldUpdatePatientStatusToDischarged() 
            // {
            //     // Init
            //     var mirthMessageModel = GetRandomMirthMesage(new string[] { "ssn" }, new string[] { "externalId", "admissionId", "EpisodeId" });
            //     mirthMessageModel.MessageType = "A04";
            //     string serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string createNewPatientResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Act
            //     mirthMessageModel.MessageType = "A03";
            //     serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string updatePatientStatus = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Asset
            //     Assert.IsTrue(createNewPatientResult.Equals("Create the Patient successfully."));
            //     Assert.IsTrue(updatePatientStatus.Equals("Discharge the Patient successfully."));
            // }
            //
            // [Test]
            // [Category("Integration")]
            // public async Task CreateNewPatientWithA04IncludSSNThenDischargeItWithA03UsingDifferentSSN4ShouldFaile()
            // {
            //     // Init
            //     var mirthMessageModel = GetRandomMirthMesage(new string[] { "ssn" }, new string[] { "externalId", "admissionId", "EpisodeId" });
            //     mirthMessageModel.MessageType = "A04";
            //     string serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string createNewPatientResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Act
            //     mirthMessageModel.Patient.identifiers= new List<IdentifierModelMock> { new IdentifierModelMock() {Type="ssn4", Value="1112" } };
            //     mirthMessageModel.MessageType = "A03";
            //     serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string updatePatientStatus = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Asset
            //     Assert.IsTrue(createNewPatientResult.Equals("Create the Patient successfully."));
            //     Assert.IsTrue(updatePatientStatus.Equals("There is no matched Patient."));
            // }
            //
            // [Test]
            // [Category("Integration")]
            // public async Task CreateNewPatientWithA04ThenChangeStatusWithA01ShouldUpdatePatientStatusToCurrent()
            // {
            //     // Init
            //     var mirthMessageModel = GetRandomMirthMesage(new string[] { "ssn" }, new string[] { "externalId", "admissionId", "EpisodeId" });
            //     mirthMessageModel.MessageType = "A04";
            //     string serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string createNewPatientResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Act
            //     mirthMessageModel.MessageType = "A01";
            //     serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string updatedPatientStatus = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Asset
            //     Assert.IsTrue(createNewPatientResult.Equals("Create the Patient successfully."));
            //     Assert.IsTrue(updatedPatientStatus.Equals("Admit the Patient successfully."));
            // }
            //
            // [Test]
            // [Category("Integration")]
            // public async Task CreateNewPatientWithA04ThenChangeStatusWithA11ShouldUpdatePatientStatusToCanceled()
            // {
            //     // Init
            //     var mirthMessageModel = GetRandomMirthMesage(new string[] { "ssn" }, new string[] { "externalId", "admissionId", "EpisodeId" });
            //     mirthMessageModel.MessageType = "A04";
            //     string serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string createNewPatientResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Act
            //     mirthMessageModel.MessageType = "A11";
            //     serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string updatedPatientStatus = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Asset
            //     Assert.IsTrue(createNewPatientResult.Equals("Create the Patient successfully."));
            //     Assert.IsTrue(updatedPatientStatus.Equals("Cancle the admission successfully."));
            // }
            //
            // [Test]
            // [Category("Integration")]
            // public async Task CreateNewPatientWithA04ThenChangeChangeHchcbIdentifiersThenCancelThePatientWithA11ShouldUpdatePatientStatusToCanceled()
            // {
            //     // Init
            //     var mirthMessageModel = GetRandomMirthMesage(new string[] { "ssn" }, new string[] { "externalId", "admissionId", "EpisodeId" });
            //     mirthMessageModel.MessageType = "A04";
            //     string serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string createNewPatientResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Act
            //     mirthMessageModel.MessageType = "A11";
            //     mirthMessageModel.HchbPatientModelMock= new HchbPatientModelMock() { EpisodeId= "ep"+ Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(2) };
            //     serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string updatedPatientStatus = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Asset
            //     Assert.IsTrue(createNewPatientResult.Equals("Create the Patient successfully."));
            //     Assert.IsTrue(updatedPatientStatus.Equals("Cancle the admission successfully."));
            // }
            //
            // [Test]
            // [Category("Integration")]
            // public async Task CreateNewPatientWithA04ThenUpdatePatientAllDemographyWithA08MessageShouldNotUpdatePatient()
            // {
            //     // Init
            //     var mirthMessageModel = GetRandomMirthMesage(new string[] { "ssn" }, new string[] { "externalId", "admissionId", "EpisodeId" });
            //     mirthMessageModel.MessageType = "A04";
            //     string serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string createNewPatientResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Act
            //     mirthMessageModel.Patient.FirstName = Utilities.GetRandomNameOrFamilyName("FirstName");
            //     mirthMessageModel.Patient.FirstName = Utilities.GetRandomNameOrFamilyName("LastName");
            //     mirthMessageModel.Patient.BirthDate = Utilities.GetRandomDateTime(new DateTime(1950, 01, 01)).ToString("yyy-MM-dd");
            //     mirthMessageModel.MessageType = "A08";
            //     serializedMessage = JsonConvert.SerializeObject(mirthMessageModel);
            //     string updatePatientResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     // Assert
            //     Assert.IsTrue(createNewPatientResult.Equals("Create the Patient successfully."));
            //     Assert.IsTrue(updatePatientResult.Equals("There is no matched Patient."));
            // }
            //
            // [Test]
            // [Category("Integration")]
            // public async Task CreatPatientWithA04ThenSendMdmMessageSouldSuccess() 
            // {
            //     // Init
            //     var mirthAdtMessageModel = GetRandomMirthMesage(new string[] { "ssn" }, new string[] { "externalId", "admissionId", "EpisodeId" });
            //     mirthAdtMessageModel.MessageType = "A04";
            //     string serializedMessage = JsonConvert.SerializeObject(mirthAdtMessageModel);
            //     string createNewPatientResult = await hchbServiceProvider.ProcessAdtMessage(serializedMessage);
            //
            //     MirthMdmMessageModel mirthMdmMessageModel = GetRandomMirthMDMMesage();
            //     mirthMdmMessageModel.Patient = mirthAdtMessageModel.Patient;
            //     serializedMessage = JsonConvert.SerializeObject(mirthMdmMessageModel);
            //     
            //     // Act
            //     string processResult = await hchbServiceProvider.ProcessMdmMessage(serializedMessage);
            //
            //     // Assert
            //     Assert.IsTrue(createNewPatientResult.Equals("Create the Patient successfully."));
            //     Assert.IsTrue(processResult.Equals("Send docuemnt successfully."));
            //
            //
            // }

            MirthAdtMessageModel GetRandomMirthMesage(IEnumerable<string>? identifiers = null, IEnumerable<string>? hchbIdentifiers = null)
            {
                MirthAdtMessageModel mirthMessageModel = new MirthAdtMessageModel();
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

                mirthMessageModel.BranchCode = Utilities.GetShortListRandomBranch().Code;

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
                    admissionId = string.Empty,
                    episodeId = string.Empty,
                    externalId = string.Empty,
                    hchbId = string.Empty,
                    patientId = "0",
                    status = string.Empty
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

            MirthMdmMessageModel GetRandomMirthMDMMesage(IEnumerable<string>? identifiers = null)
            {
                MirthMdmMessageModel mirthMessageModel = new MirthMdmMessageModel();

                mirthMessageModel.MessageControlId = DateTime.Now.ToString();

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
                if (!identifiers.IsNullOrEmpty())
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

                mirthMessageModel.Patient = patient;

                TransactionModelMock transaction = new TransactionModelMock();
                transaction.OrderDate = Utilities.GetRandomDateTime().ToString("yyyyMMddHHmmss");
                transaction.OrderNumber = Utilities.GetRandomDecimalString(5);
                transaction.FileName = @"MDM/pdf/16_TS_12345.pdf";
                transaction.EffectiveDate = Utilities.GetRandomDateTime().ToString("yyyyMMddHHmmss");
                transaction.ObservationId = string.Empty;// "Report";
                transaction.ObservationText = string.Empty;// "Report of Patient";
                transaction.TemplateId = "548979";

                mirthMessageModel.Transaction = transaction;

                PersonModelMock signer = new PersonModelMock();
                signer.NPI = "1231231313";//Utilities.GetRandomDecimalString(10).ToString();
                signer.FirstName = Utilities.GetRandomNameOrFamilyName("FirstName");
                signer.LastName = Utilities.GetRandomNameOrFamilyName("LastName");
                signer.BranchCode = Utilities.GetShortListRandomBranch()?.Code;
                mirthMessageModel.Signer = signer;


                PersonModelMock sender = new PersonModelMock();
                sender.NPI = "9779797232";//Utilities.GetRandomDecimalString(10).ToString();
                sender.FirstName = Utilities.GetRandomNameOrFamilyName("FirstName");
                sender.LastName = Utilities.GetRandomNameOrFamilyName("LastName");
                sender.BranchCode = Utilities.GetShortListRandomBranch()?.Code;
                mirthMessageModel.Sender = signer;

                return mirthMessageModel;
            }


        }
    }
}
