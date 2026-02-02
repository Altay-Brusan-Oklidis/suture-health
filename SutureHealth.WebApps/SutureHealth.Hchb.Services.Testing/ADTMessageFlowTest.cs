using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Amazon.Lambda.KinesisEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.Runtime;
using Microsoft.Extensions.Hosting;
using NHapi.Base.Parser;
using NHapiTools.Base.Net;
using NUnit.Framework;
using SutureHealth.Hchb.Services.Testing.Model.Header;
using SutureHealth.Hchb.Services.Testing.Model.Patient;
using SutureHealth.Patients;
using SutureHealth.Patients.Services;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.DependencyInjection;
using SutureHealth.Diagnostics;
using SutureHealth.Extensions.Configuration;
using SutureHealth.Hchb.Services.Lambda;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Patients.Services.SqlServer;
using NHapi.Base.Model;
using SutureHealth.Hchb.Services;
using SutureHealth.Hchb.Services.Testing.Utility;
using SutureHealth.Hchb.Services.Testing.Model.Address;
using SutureHealth.Hchb.Services.Testing.MessageBuilder;

namespace SutureHealth.PatientAPI.HCHB.Services.Testing
{
    public class ADTMessageFlowTest
    {
        private static int organizationId = 2000;
        private static string MIRTH_SERVER_IP = "10.50.90.21";
        private static int MIRTH_SERVER_PORT = 6661;//6661; 6661 is test port 6662 is production port
        ADTMessageBuilder adtMessageBuilder;
        protected PatientDbContext patientDbContext;
        protected HchbWebDbContext hchbWebDbContext;
        protected IPatientServicesProvider patientServicesProvider;
        protected IHchbServiceProvider hchbServiceProvider;
        protected Application.Organization organization;


        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static string Base64Encode(byte[] message)
        {
            return Convert.ToBase64String(message);
        }
        private static async Task<string> GetKinesisLogOutput()
        {
            var credentials = new BasicAWSCredentials("YOUR_AWS_ACCESS_KEY", @"YOUR_AWS_SECRET_KEY");
            IAmazonCloudWatchLogs client = new AmazonCloudWatchLogsClient(credentials, RegionEndpoint.USEast1); // provide regionEndpoint
            string kinesisOutput = String.Empty;
            List<string> stringlist = new();
            var logGroupName = @"/aws/lambda/test-lambda-kinesis"; // the CloudWatch log group name
            var describeLogStreamsRequest = new DescribeLogStreamsRequest()
            {
                LogGroupName = logGroupName,
                OrderBy = OrderBy.LastEventTime,
                Descending = true
            };
            try
            {
                var describeLogStreamsResult = await client.DescribeLogStreamsAsync(describeLogStreamsRequest);
                var logStreams = describeLogStreamsResult.LogStreams;//OrderByDescending(x => x.CreationTime).Take(1); // sort by creation time and get the latest log stream
                var stream = describeLogStreamsResult.LogStreams.FirstOrDefault(t => t.LogStreamName == "KINESIS_LOG_STREAM");

                var eventsRequest = new GetLogEventsRequest()
                {
                    LogStreamName = stream?.LogStreamName,
                    LogGroupName = logGroupName,
                    StartFromHead = false
                };

                var result = await client.GetLogEventsAsync(eventsRequest);
                var latestRecord = result.Events.OrderByDescending(t => t.Timestamp).FirstOrDefault();
                return latestRecord.Message.Substring(latestRecord.Message.IndexOf('{'));
            }
            catch (Exception)
            {
                throw;
            }

            return kinesisOutput;
        }

        private IMessage BuildADTMessage(ADTMessageBuilder messageBuilder, TriggerEvent trigger = TriggerEvent.A01)
        {
            var message = messageBuilder.Build(trigger);
            return message;
        }
        private string ParseADTMessage(IMessage message)
        {
            var pipeParser = new PipeParser();
            var parsedMessage = pipeParser.Encode(message);
            return parsedMessage;
        }
        private string SendMessageToMirthServer(string message)
        {

            string response = string.Empty;
            try
            {
                var connection = new SimpleMLLPClient(MIRTH_SERVER_IP, MIRTH_SERVER_PORT, Encoding.UTF8);

                PipeParser parser = new PipeParser();
                string msg = String.Empty;
                //string myBucketName = "hl7messagebucket"; //your s3 bucket name goes here  
                response = connection.SendHL7Message(message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occured while creating and transmitting HL7 Message {e.Message}");
            }
            return response;
        }

        private static ADTMessageBuilder ConvertMessageBuilderTriggerType(ADTMessageBuilder source, TriggerEvent trigger)
        {
            ADTMessageBuilder newBuilder = new ADTMessageBuilder();
            newBuilder.Build(trigger, source.MedicalRecordNumber);
            if (newBuilder.PID is not null)
            {
                newBuilder.PID.Address = source?.PID?.Address;
                newBuilder.PID.AccountNumber = source?.PID?.AccountNumber;
                newBuilder.PID.BusinessPhoneNumber = source?.PID?.BusinessPhoneNumber;
                newBuilder.PID.HomePhoneNumber = source?.PID?.HomePhoneNumber;
                newBuilder.PID.Race = source?.PID?.Race;
                newBuilder.PID.AccountNumber = source?.PID?.AccountNumber;
                newBuilder.PID.Address = source?.PID?.Address;
                newBuilder.PID.AlternatePatientId = source?.PID?.AlternatePatientId;
                newBuilder.PID.BusinessPhoneNumber = source?.PID?.BusinessPhoneNumber;
                newBuilder.PID.DateOfBirth = source?.PID?.DateOfBirth;
                newBuilder.PID.DeathDateAndTime = source?.PID?.DeathDateAndTime;
                newBuilder.PID.DeathIndicator = source?.PID?.DeathIndicator;
                newBuilder.PID.ExternalPatientId = source?.PID?.ExternalPatientId;
                newBuilder.PID.HomePhoneNumber = source?.PID?.HomePhoneNumber;
                newBuilder.PID.MaritalStatus = source?.PID?.MaritalStatus;
                newBuilder.PID.Name = source?.PID.Name;
                newBuilder.PID.PatientId = source?.PID.PatientId;
                newBuilder.PID.PrimaryLanguage = source?.PID?.PrimaryLanguage;
                newBuilder.PID.Race = source?.PID?.Race;
                newBuilder.PID.SetId = source?.PID?.SetId; // SetID may need to be updated                
                newBuilder.PID.Sex = source.PID.Sex;
                newBuilder.PID.SSN = source?.PID?.SSN;
            }
            if (newBuilder.PV1 is not null)
            {
                newBuilder.PV1.VisitNumber = source.PV1.VisitNumber;
                newBuilder.PV1.DischargeDisposition = source.PV1.DischargeDisposition;
                newBuilder.PV1.DischargeDateTime = source.PV1.DischargeDateTime;
                newBuilder.PV1.DischargedToLocation = source.PV1.DischargeDateTime;
                newBuilder.PV1.AdmitDateTime = source.PV1.AdmitDateTime;
                newBuilder.PV1.AdmissionType = source.PV1.AdmissionType;
                newBuilder.PV1.AttendingDoctor = source.PV1.AttendingDoctor;
                newBuilder.PV1.AccountStatus = source.PV1.AccountStatus;
                newBuilder.PV1.AssignedPatientLocation = source.PV1.AssignedPatientLocation;
                newBuilder.PV1.ConsultingDoctor = source.PV1.ConsultingDoctor;
                newBuilder.PV1.VisitNumber = source?.PV1.VisitNumber;
                //newBuilder.PV1.SetId = source?.PV1.SetId;
                newBuilder.PV1.ReferringDoctor = source?.PV1.ReferringDoctor;
                newBuilder.PV1.PatientType = source?.PV1.PatientType;
            }

            return newBuilder;


        }


        [SetUp]
        public void Setup()
        {
            adtMessageBuilder = new ADTMessageBuilder();
            (patientServicesProvider, patientDbContext, hchbServiceProvider, hchbWebDbContext) = CreateServicesProvider();

            organization = new Application.Organization
            {
                OrganizationId = organizationId
            };
        }


        private static (IPatientServicesProvider patientServicesProvider,
                        PatientDbContext patientDbContext,
                        IHchbServiceProvider hchbServiceProvider,
                        HchbWebDbContext hchbDbContext) CreateServicesProvider()
        {

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
                                                  DataSource = configuration["SqlDatabase:DataSource"],
                                                  UserID = configuration["SqlDatabase:UserID"],
                                                  Password = configuration["SqlDatabase:Password"],
                                                  InitialCatalog = configuration["SqlDatabase:InitialCatalogue:SutureHealthAPI"],
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
            var hchbDbContext = applicationHost.Services.GetRequiredService<HchbWebDbContext>();
            var patientDbContext = applicationHost.Services.GetRequiredService<PatientDbContext>();
            var hchbServiceProvider = applicationHost.Services.GetRequiredService<IHchbServiceProvider>();
            return (patientServicesProvider, patientDbContext, hchbServiceProvider, hchbDbContext);
        }

        [TearDown]
        public void Clean()
        {
            patientDbContext.SaveChanges();
        }


        [Test]
        [Category("Integration")]
        public void SendA01MessageToMirthServerShouldReceiveResponseACKResponse()
        {
            var imessage = BuildADTMessage(adtMessageBuilder);

            string message = ParseADTMessage(imessage);
            string response = string.Empty;
            try
            {
                response = SendMessageToMirthServer(message);
            }
            catch (Exception e)
            {
                Assert.Fail("Send Message failed due to: ", e.Message);
            }
            Assert.IsTrue(response.Contains("ACK"));
        }

        [Test]
        [Category("Integration")]
        public void SendA04MessageToMirthServerShouldReceiveResponseACKResponse()
        {
            //Init
            var imessage = BuildADTMessage(adtMessageBuilder, TriggerEvent.A04);
            string message = ParseADTMessage(imessage);

            //Act
            try
            {
                var response = SendMessageToMirthServer(message);
            }
            catch (Exception e)
            {
                Assert.Fail("Send Message failed due to: ", e.Message);
            }

            //Asset       
            try
            {
                var result = patientServicesProvider.QueryPatients()
                                                .FirstOrDefault(p => p.FirstName == adtMessageBuilder.PID.Name.FirstName &&
                                                                     p.LastName == adtMessageBuilder.PID.Name.FamilyName);

                Assert.IsNotNull(result);


            }
            catch (Exception e)
            {

                Assert.Fail("Send Message failed due to: ", e.Message);
            }
        }

        [Test]
        [Category("Integration")]
        [Obsolete("The Mirth channels used for this test is not active.")]
        public async Task SendA01MessageThenReadKinesisLogShouldIncludeAllSentData()
        {
            string message = string.Empty, response = string.Empty;
            JsonElement patientInfo = new JsonElement();

            try
            {
                var imessage = BuildADTMessage(adtMessageBuilder);
                message = ParseADTMessage(imessage);
                response = string.Empty;

                response = SendMessageToMirthServer(message);
                Task.Delay(2000).Wait();
                var kinesisLog = await GetKinesisLogOutput();
                var dataSection = kinesisLog.Split(',')[3];
                var lastIndex = dataSection.Length;
                var data = dataSection.Substring(10, lastIndex - 11);
                var msg = Base64Decode(data);
                patientInfo = JsonSerializer.Deserialize<JsonElement>(msg);

            }
            catch (Exception e)
            {
                Assert.Fail("Send Message failed due to: ", e.Message);
            }

            Assert.IsTrue(response.Contains("ACK"));
            Assert.IsTrue(patientInfo.GetProperty("Patient").GetProperty("FirstName").ToString() == adtMessageBuilder?.PID?.Name.FirstName);
            Assert.IsTrue(patientInfo.GetProperty("Patient").GetProperty("LastName").ToString() == adtMessageBuilder?.PID?.Name.FamilyName);
            Assert.IsTrue(patientInfo.GetProperty("Patient").GetProperty("city").ToString() == adtMessageBuilder?.PID?.Address?.City);
            Assert.IsTrue(patientInfo.GetProperty("Patient").GetProperty("addressLine1").ToString() == adtMessageBuilder?.PID?.Address?.StreetAddress);

        }

        [Test]
        [Category("Integration")]
        [Obsolete("Pre-recorded kinesis data is not used anymore.")]
        public async Task CreateNewPatientShouldCreateAWSCloudWatchLog()
        {

            var imessage = BuildADTMessage(adtMessageBuilder);
            var message = ParseADTMessage(imessage);
            KinesisEvent kinesisEvent = new KinesisEvent
            {
                Records = new List<KinesisEvent.KinesisEventRecord>
                    {
                        new KinesisEvent.KinesisEventRecord
                        {
                            AwsRegion = "us-west-2",
                            Kinesis = new KinesisEvent.Record
                            {
                                ApproximateArrivalTimestamp = DateTime.Now,
                                Data = new MemoryStream(Encoding.UTF8.GetBytes(message))
                            }
                        }
                    }
            };

            try
            {
                var context = new TestLambdaContext();
                var function = new Function();
                await function.FunctionHandler(kinesisEvent, context);
            }
            catch (Exception e)
            {

                Assert.Fail(e.Message);
            }

            Assert.Pass();

        }

        [Test]
        [Category("Integration")]
        public async Task CreateNewPatientThenUpdateStatusToCurrentShouldCreateAWSCloudWatchLog()
        {
            var matchRequest = adtMessageBuilder.ConvertToMatchRequest();
            var imessage = BuildADTMessage(adtMessageBuilder);
            string message = ParseADTMessage(imessage);
            SendMessageToMirthServer(message);
            await patientServicesProvider.MatchAsync(matchRequest);
            var ADT03Message = ConvertMessageBuilderTriggerType(adtMessageBuilder, TriggerEvent.A03);
            imessage = BuildADTMessage(ADT03Message);
            SendMessageToMirthServer(message);

        }

        [Test]
        [Category("Integration")]
        public async Task CreateNewPatientWithA04ThenUpdateStatusWithA01ToCurrentShouldUpdateThePatientStatus()
        {
            // Init
            var imessage = BuildADTMessage(adtMessageBuilder, TriggerEvent.A04);
            string message = ParseADTMessage(imessage);
            SendMessageToMirthServer(message);

            // Act
            adtMessageBuilder.UpdateTrigger(TriggerEvent.A01);
            message = ParseADTMessage(adtMessageBuilder.Message);
            SendMessageToMirthServer(message);

            //Assert
            string? result = string.Empty;
            try
            {
                var patient = patientDbContext.Patients.FirstOrDefault(p => p.FirstName == adtMessageBuilder.PID.Name.FirstName &&
                                                            p.LastName == adtMessageBuilder.PID.Name.FamilyName);
                if (patient is null) throw new ArgumentNullException();
                int patientId = patient.PatientId;
                result = hchbWebDbContext.HchbPatients.Where(p=> p.Id == patientId).FirstOrDefault().Status;
            }
            catch (Exception e)
            {
                Assert.Fail("Test failed fue to:" + e.Message);
            }
            Assert.IsTrue(string.Equals(result, "CURRENT"));

        }

        [Test]
        [Category("Integration")]
        public void CreateNewPatientWithA04ThenUpdateThenPatientNameWithA08ShouldUpdateThePatientNameInDatabase()
        {
            // Init
            var imessage = BuildADTMessage(adtMessageBuilder, TriggerEvent.A04);
            string message = ParseADTMessage(imessage);
            SendMessageToMirthServer(message);

            var patient = patientDbContext.Patients
                                          .OrderByDescending(p => p.CreatedAt)
                                          .FirstOrDefault(p => p.FirstName == adtMessageBuilder.PID.Name.FirstName);

            // Act
            adtMessageBuilder.UpdateADTMessageDemographics(name: "TestName");
            adtMessageBuilder.UpdateTrigger(TriggerEvent.A08);
            message = ParseADTMessage(imessage);
            SendMessageToMirthServer(message);
            string firstName = patientDbContext.Patients
                                               .FirstOrDefault(p => p.PatientId == patient.PatientId).FirstName;

            // Assest
            Assert.IsTrue(firstName == "TestName");


        }

        [Test]
        [Category("Integration")]
        public async Task CreateNewPatientWithA04ThenUpdateStatusToDischargeWithA03ShouldUpdateThePatientStatusToCancel()
        {
            // Init
            var imessage = BuildADTMessage(adtMessageBuilder, TriggerEvent.A04);
            string message = ParseADTMessage(imessage);
            SendMessageToMirthServer(message);

            // Act
            adtMessageBuilder.UpdateTrigger(TriggerEvent.A03);
            message = ParseADTMessage(adtMessageBuilder.Message);
            SendMessageToMirthServer(message);

            //Assert
            string? result = string.Empty;
            try
            {
                var patient = patientDbContext.Patients.FirstOrDefault(p => p.FirstName == adtMessageBuilder.PID.Name.FirstName &&
                                                            p.LastName == adtMessageBuilder.PID.Name.FamilyName);
                if (patient is null) throw new ArgumentNullException();
                int patientId = patient.PatientId;
                result = hchbWebDbContext.HchbPatients.FirstOrDefault(p=>p.Id==patientId).Status;
            }
            catch (Exception e)
            {
                Assert.Fail("Test failed fue to:" + e.Message);
            }
            Assert.IsTrue(string.Equals(result, "DISCHARGED"));
        }

        [Test]
        [Category("Integration")]
        public async Task CreateNewPatientWithA04ThenCancelWithA11ShouldUpdateThePatientStatusToCancel()
        {
            // Init
            var imessage = BuildADTMessage(adtMessageBuilder, TriggerEvent.A04);
            string message = ParseADTMessage(imessage);
            SendMessageToMirthServer(message);

            // Act
            adtMessageBuilder.UpdateTrigger(TriggerEvent.A11);
            message = ParseADTMessage(adtMessageBuilder.Message);
            SendMessageToMirthServer(message);

            //Assert
            string? result = string.Empty;
            try
            {
                var patient = patientDbContext.Patients.FirstOrDefault(p => p.FirstName == adtMessageBuilder.PID.Name.FirstName &&
                                                            p.LastName == adtMessageBuilder.PID.Name.FamilyName);
                if (patient is null) throw new ArgumentNullException();
                int patientId = patient.PatientId;
                result = hchbWebDbContext.HchbPatients.Where(p => p.Id == patientId).FirstOrDefault().Status;
            }
            catch (Exception e)
            {
                Assert.Fail("Test failed fue to:" + e.Message);
            }
            Assert.IsTrue(string.Equals(result, "NON_ADMIT"));
        }


        [Test]
        [Category("Integration")]
        public void CreatealreadyExistingPatientShouldCreateAMatchLogRecord()
        {
            Patient? patient;
            // Init
            try
            {
                Random random = new Random();
                patient = patientDbContext.Patients.Include(p => p.Addresses)
                                                   .Include(p => p.Identifiers)
                                                   .OrderByDescending(o => o.CreatedAt)
                                                   .Where(p => p.Identifiers.Count > 1)
                                                   .Take(250).ToList()[random.Next(0, 249)];

            }
            catch (Exception)
            {

                throw;
            }

            if (patient is null)
                Assert.Fail("Can not retrive an existing Patient from databse.");

            var imessage = BuildADTMessage(adtMessageBuilder, TriggerEvent.A04);


            // Act
            adtMessageBuilder.UpdateADTMessageWithPatientInfo(patient);
            string message = ParseADTMessage(adtMessageBuilder.Message);
            SendMessageToMirthServer(message);

            // Assert
            //MatchLog should create a new item.
            MatchLog? matchLog = patientDbContext.MatchLogs
                                                .FirstOrDefault(l => l.FirstName == patient.FirstName
                                                                && l.LastName == patient.LastName);

            Assert.IsNotNull(matchLog);

        }
    }

    public static class ADTMessageBuilderExtentions
    {
        public static PatientMatchingRequest ConvertToMatchRequest(this ADTMessageBuilder builder)
        {
            PatientMatchingRequest request = new();

            request.FirstName = builder?.PID?.Name.FirstName;
            request.MiddleName = builder?.PID?.Name.MiddleInitial;
            request.LastName = builder?.PID?.Name.FamilyName;

            switch (builder?.PID?.Sex)
            {
                case GenderType.F:
                    request.Gender = Gender.Female;
                    break;
                case GenderType.M:
                    request.Gender = Gender.Male;
                    break;
                case GenderType.U:
                    request.Gender = Gender.Unknown;
                    break;
            }

            request.City = builder?.PID?.Address?.City;
            request.AddressLine1 = builder?.PID?.Address?.StreetAddress;
            request.PostalCode = builder?.PID?.Address?.ZipCode;
            request.Ids = new List<IIdentifier>();
            for (int index = 0; index < builder?.PID?.PatientId.Count; index++)
            {
                var id = builder?.PID?.PatientId[index];
                if (id?.IdentifierTypeCode == IdentifierCodeType.SS)
                {
                    request.Ids.Add(new PatientIdentifier() { Value = id.Id, Type = KnownTypes.SocialSecurityNumber });
                }
                if (id?.IdentifierTypeCode == IdentifierCodeType.MA || id?.IdentifierTypeCode == IdentifierCodeType.MCD)
                {
                    request.Ids.Add(new PatientIdentifier() { Value = id.Id, Type = KnownTypes.MedicaidNumber });
                }
                if (id?.IdentifierTypeCode == IdentifierCodeType.MC || id?.IdentifierTypeCode == IdentifierCodeType.MCR)
                {
                    request.Ids.Add(new PatientIdentifier() { Value = id.Id, Type = KnownTypes.Medicare });
                }
                if (id?.IdentifierTypeCode == IdentifierCodeType.MB)
                {
                    request.Ids.Add(new PatientIdentifier() { Value = id.Id, Type = KnownTypes.PrivateInsurance });
                }
                if (builder?.PID?.ExternalPatientId.IsNullOrEmpty() == false)
                {
                    request.Ids.Add(new PatientIdentifier() { Value = builder?.PID?.ExternalPatientId, Type = KnownTypes.UniqueExternalIdentifier });
                }
                // TODO: check for further identifier types
            }

            /*
            Branch branch = new Branch()
            {
                AgencyName = msg?.PV1.AssignedPatientLocation.PointOfCare.Value,
                Room = msg?.PV1?.AssignedPatientLocation.Room.Value,
                Code = msg?.PV1?.AssignedPatientLocation.Facility.NamespaceID.Value,
                BranchName = msg?.PV1?.AssignedPatientLocation.Facility.UniversalID.Value,
                ServiceLine = msg?.PV1.AssignedPatientLocation.LocationStatus.Value,
                TeamName = msg?.PV1.AssignedPatientLocation.LocationDescription.Value
            };
            Organization org = GetOrganizationOf(branch);
            AdmissionType = msg?.PV1?.AdmissionType.Value;
            */

            return request;
        }

    }

}