using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.Areas.Admin.Controllers;
using SutureHealth.AspNetCore.Areas.Admin.Models.Review;
using SutureHealth.Diagnostics;
using SutureHealth.Extensions.Configuration;
using SutureHealth.Hchb.Services;
using SutureHealth.Patients;
using SutureHealth.Patients.Services;
using SutureHealth.Patients.Services.SqlServer;
using SutureHealth.Requests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SutureHealth.AspNetCore.WebHost.Testing.Area.Admin.Controllers
{

    public class ReviewControllerTestBase
    {
        protected ApplicationServiceMock securityServicesMock;
        protected RequestServiceProviderMock requestServicesMock;
        protected PatientServicesProvider patientServices = null;
        protected HchbServiceProvider hchbServiceProvider = null;
        protected PatientDbContext patientDbContext;
        protected HchbWebDbContext hchbWebDbContext;
        protected Application.Organization organization;
        protected Patients.Organization requestOrganization;
        protected IHost applicationHost;

        protected (ApplicationServiceMock, RequestServiceProviderMock, PatientServicesProvider) GetServiceProviders(PatientDbContext patientDbContext)
        {
            var securityServicesMock = new ApplicationServiceMock();
            var requestServicesMock = new RequestServiceProviderMock();
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(config =>
                {
                    config.SingleLine = false;
                    config.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
                    config.IncludeScopes = true;
                    config.UseUtcTimestamp = false;
                });
            });
            var logger = loggerFactory.CreateLogger<IPatientServicesProvider>();
            var tracer = new NullTracingService();
            var patientServicesProvider = new PatientServicesProvider(patientDbContext, logger, tracer, serviceProvider);


            return (securityServicesMock, requestServicesMock, patientServicesProvider);
        }

        protected ReviewController GetController(
            IRequestServicesProvider requestService,
            IApplicationService securityService,
            IPatientServicesProvider patientService,
            int memberId)
        {
            var controller = new ReviewController(requestService, securityService, patientService);

            controller.CurrentUser = new MemberIdentity
            {
                MemberId = memberId
            };

            return controller;
        }

        [SetUp]
        public void Setup()
        {
            try
            {
                patientDbContext = InitPatientDatabaseContex();
                (securityServicesMock, requestServicesMock, patientServices) = GetServiceProviders(patientDbContext);
            }
            catch (Exception e)
            {

                throw;
            }

        }

        [TearDown]
        public void Dispose()
        {
            patientDbContext.SaveChanges();
            patientDbContext.Dispose();
        }

        protected PatientDbContext CreateDbContext()
        {
            return applicationHost.Services.GetRequiredService<PatientDbContext>();
        }
        private PatientDbContext InitPatientDatabaseContex()
        {
            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath().Load();

            applicationHost = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var assemblies = new string[] {
                                          typeof(SutureHealth.Services.Amazon.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Application.Services.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.Application.Services.SqlServer.HostingStartup).Assembly.GetName().Name,
                                          typeof(SutureHealth.AspNetCore.Identity.HostingStartup).Assembly.GetName().Name

                          };
                    webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, string.Join(";", assemblies));
                    webBuilder.ConfigureAppConfiguration((host, config) =>
                    {
                        config.AddDefaultConfigurations(runtimeEnvironment: "dev")
                              .AddJsonFile("appsettings.dev.json")
                              .Build();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddDefaultAWSOptions(context.Configuration.GetAWSOptions());
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
                    });
                }).Build();

            patientDbContext = applicationHost.Services.GetRequiredService<PatientDbContext>();
            return patientDbContext;
        }
    }


    public class GivenMatchHappened : ReviewControllerTestBase
    {

        private int memberId = 3003118;
        private int organizationId = 12069;
        private Patients.Patient patient; // default patient object stored in db,
        private List<Patients.Patient> garbaseList = new();
        protected ReviewController reviewController;

        private Patients.Patient CreatePatient( string firstName = null,
                                                string lastName = null,
                                                DateTime? birthDay = null,
                                                Gender gender = Gender.Unknown,
                                                DateTime? dob = null,
                                                List<PatientPhone> phoneList = null,
                                                List<PatientIdentifier> identifiers = null,
                                                List<PatientAddress> addresses = null)
        {


            string ssn = Utilities.GetRandomSSN();
            Patients.Patient patient = new Patients.Patient()
            {
                FirstName = firstName ?? Utilities.GetRandomFirstName(),
                LastName = lastName ?? Utilities.GetRandomLastName(),
                Gender = gender == Gender.Unknown ? Gender.Male : gender,
                Birthdate = dob ?? Utilities.GetRandomDOB(),
                Phones = phoneList ?? new List<PatientPhone>()
                {
                    new PatientPhone()
                    {
                        Type = ContactType.HomePhone,
                        Value = Utilities.GetRandomPhoneNumber(),
                        IsPrimary = true
                    },
                    new PatientPhone()
                    {
                        Type = ContactType.Mobile,
                        Value = Utilities.GetRandomPhoneNumber(),
                        IsPrimary = false
                    },
                    new PatientPhone()
                    {
                        Type = ContactType.WorkPhone,
                        Value = Utilities.GetRandomPhoneNumber(),
                        IsPrimary = false
                    }
                },
                Addresses = addresses ?? new List<PatientAddress>()
                {
                    new PatientAddress()
                    {
                        Line1 = Utilities.GetRandomStreetAddress(),
                        StateOrProvince = Utilities.GetRandomState(),
                        City = Utilities.GetRandomCity(),
                        PostalCode = Utilities.GetRandomZip()
                    }
                },
                Identifiers = identifiers ?? new List<PatientIdentifier>()
                {
                    new PatientIdentifier()
                    {
                        Type= KnownTypes.SocialSecurityNumber,
                        Value = ssn,
                    },
                    new PatientIdentifier()
                    {
                        Type= KnownTypes.SocialSecuritySerial,
                        Value =ssn.GetLast(4)
                    }
                }
            };
            return patient;
        }



        [SetUp]
        public async Task Init()
        {
            patient = CreatePatient();
            var _patient = await patientServices.CreateAsync(patient, organizationId, memberId);
            garbaseList.Add(_patient);
            reviewController = GetController(requestServicesMock.Object, securityServicesMock.Object, patientServices, memberId);
            patientDbContext.SaveChanges();
        }

        [TearDown]
        public void CleanUp()
        {
            using var txn = patientDbContext.Database.BeginTransaction();

            try
            {
                foreach (var patient in garbaseList)
                {
                    try
                    {
                        patientDbContext.Database.ExecuteSqlRaw(
                        "DELETE FROM [SutureSignApi-QA].[dbo].[PatientPhone] WHERE PatientId = {0}", patient.PatientId);

                        patientDbContext.Database.ExecuteSqlRaw(
                            "DELETE FROM [SutureSignApi-QA].[dbo].[PatientAddress] WHERE PatientId = {0}", patient.PatientId);

                        patientDbContext.Database.ExecuteSqlRaw(
                            "DELETE FROM [SutureSignApi-QA].[dbo].[Patient] WHERE PatientId = {0}", patient.PatientId);

                        patientDbContext.Database.ExecuteSqlRaw(
                            "DELETE FROM [SutureSignApi-QA].[dbo].[PatientMatchLog] WHERE FirstName = {0} AND LastName = {1}", patient.FirstName, patient.LastName);

                        patientDbContext.Database.ExecuteSqlRaw(
                          "DELETE FROM [SutureSignApi-QA].[dbo].[PatientMatchOutcome] WHERE PatientId = {0}", patient.PatientId);
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
            }
            catch (Exception e)
            {

                throw;
            }

            reviewController.Dispose();
            txn.Commit();

            //base.Dispose();
        }

        private void RemovedLoggedTable()
        {
            patientDbContext.Database.ExecuteSqlRaw(
                       " DECLARE @DeletedId TABLE(Id INT);" +
                       " DELETE FROM [SutureSignApi-QA].[dbo].[PatientMatchLog]" +
                       " OUTPUT DELETED.MatchPatientLogID INTO @DeletedId" +
                       " WHERE MatchPatientLogID = (SELECT TOP 1 MatchPatientLogID" +
                       "            FROM [SutureSignApi-QA].[dbo].[PatientMatchLog]" +
                       "            ORDER BY CreateDate DESC);" +
                       " SELECT * FROM @DeletedId" +
                       " DELETE FROM [SutureSignApi-QA].[dbo].[PatientMatchOutcome]" +
                       " WHERE MatchPatientLogID  IN (SELECT Id FROM @DeletedId)");
        }


        [Test]
        public async Task TryToCreateAnExistingPatientShouldReturnMatch()
        {
            //Init
            var matchRequest = patient.ToMatchingRequest();
            PatientMatchingResponse response;

            try
            {
                // Act
                response = await patientServices.MatchAsync(matchRequest);
            }
            catch (Exception e)
            {
                throw;
            }

            // Assert
            Assert.IsTrue(response.MatchLevel == Linq.MatchLevel.Match);
        }


        [Test]
        public async Task TheMatchListIncludesAllMatchOutcomes()
        {
            //Init
            var matchRequest = patient.ToMatchingRequest();

            // Act
            _ = await patientServices.MatchAsync(matchRequest);
            patient.FirstName = "Calanthia";
            var newMatchRequest = patient.ToMatchingRequest();
            garbaseList.Add(await patientServices.CreateAsync(patient, organizationId, memberId));
            var response = await patientServices.MatchAsync(newMatchRequest);

            // Assert

            RemovedLoggedTable();
            Assert.IsTrue(response.MatchResults.Count() > 1);
        }

        [Test]
        public async Task AssociateMergeShouldSucceed()
        {
            // Init
            Patients.Patient patientClone = patient.Clone();
            patientClone.FirstName = Utilities.GetRandomFirstName();
            patientClone.LastName = Utilities.GetRandomLastName();
            var matchRequest = patientClone.ToMatchingRequest();
            var response = await patientServices.MatchAsync(matchRequest);
            var log = patientDbContext.MatchLogs.OrderByDescending(t => t.CreateDate).FirstOrDefault();
            AssociateMergeRequest mergeOptions = new()
            {
                SelectedPatientId = response.TopMatch?.PatientId.ToString(),
                FirstName = patientClone.FirstName,
                LastName = patientClone.LastName
            };

            // Act
            _ = await reviewController.AssociateMerge(log.MatchPatientLogID, mergeOptions);
            var _patient = patientServices.GetByIdentifier("suture-unique-identifier", response.TopMatch.PatientId.ToString()).AsEnumerable().FirstOrDefault();
            patientDbContext.Entry(_patient).Reload();

            // Assert
            Assert.IsNotNull(_patient);
            Assert.IsTrue(_patient.FirstName == patientClone.FirstName);
        }

        [Test]
        public async Task UpdateSSNWithNullInAssociateMergeShouldSucceed()
        {
            // Init
            Patients.Patient patientClone = patient.Clone();
            patientClone.Identifiers.Clear();
            patientClone.Identifiers.Add(new PatientIdentifier() { Type = KnownTypes.MedicaidNumber, Value = "5120124" });
            var matchRequest = patientClone.ToMatchingRequest();
            var response = await patientServices.MatchAsync(matchRequest);
            var log = patientDbContext.MatchLogs.OrderByDescending(t => t.CreateDate).FirstOrDefault();
            AssociateMergeRequest mergeOptions = new()
            {
                SelectedPatientId = response.TopMatch?.PatientId.ToString(),
                FirstName = patientClone.FirstName,
                LastName = patientClone.LastName,
                IsSSNDownCasted = true,
                SSN = string.Empty
            };

            // Act             
            var result = await reviewController.AssociateMerge(log.MatchPatientLogID, mergeOptions);

            Patients.Patient reloadPatient = null;

            using (var dbContext = CreateDbContext())
            {
                int id = response.TopMatch.PatientId;
                reloadPatient = dbContext.Patients.Where(p => p.PatientId == id).FirstOrDefault();
                dbContext.Entry<Patients.Patient>(reloadPatient).Collection(p => p.Identifiers).Load();
            }

            // Assert
            Assert.IsNotNull(reloadPatient);
            Assert.IsTrue(!reloadPatient.Identifiers.Any(p => p.Type == KnownTypes.SocialSecurityNumber));
        }

        //[Test]
        //public async Task UpdateMedicaidWithNullInAssociateMergeShouldSucceed()
        //{
        //    // Init

        //    Patients.Patient patientClone = patient.Clone();
        //    patientClone.Identifiers.Clear();
        //    patientClone.Identifiers.Add(new PatientIdentifier() { Type = KnownTypes.MedicaidNumber, Value = "5120124" });
        //    patientClone.Identifiers.Add(new PatientIdentifier() { Type = KnownTypes.MedicaidState, Value = "NY" });
        //    patientClone.Identifiers.Add(new PatientIdentifier() { Type = KnownTypes.Medicaid });
        //    patientClone.Identifiers.Add(new PatientIdentifier() { Type = KnownTypes.SocialSecurityNumber, Value = Utilities.GetRandomSSN() });
        //    var _patient = await patientServices.CreateAsync(patientClone, organizationId, memberId);
        //    garbaseList.Add(_patient);
        //    patientClone = _patient.Clone();

        //    patientClone.FirstName = Utilities.GetRandomFirstName();
        //    patientClone.Identifiers.Remove(patientClone.Identifiers.First(i=> i.Type== KnownTypes.MedicaidNumber));
        //    patientClone.Identifiers.Remove(patientClone.Identifiers.First(i=> i.Type== KnownTypes.MedicaidState));
        //    patientClone.Identifiers.Remove(patientClone.Identifiers.First(i=> i.Type== KnownTypes.Medicaid));
        //    patientClone.Identifiers.Add(new PatientIdentifier() { Type = KnownTypes.SelfPay , Value="True"});

        //    var matchRequest = patientClone.ToMatchingRequest();
        //    var response = await patientServices.MatchAsync(matchRequest);
        //    var log = patientDbContext.MatchLogs.OrderByDescending(t => t.CreateDate).FirstOrDefault();

        //    AssociateMergeRequest mergeOptions = new()
        //    {
        //        SelectedPatientId = response.TopMatch?.PatientId.ToString(),
        //        FirstName = patientClone.FirstName,
        //        LastName = patientClone.LastName,
        //        IsSSNDownCasted = false,
        //        SSN = string.Empty,
        //        MedicaidNumber = string.Empty,
        //        HomePhone = Utilities.GetRandomPhoneNumber(),
        //        PrimaryPhone = "HomePhone"
        //    };

        //    // Act             
        //    var result = await reviewController.AssociateMerge(log.MatchPatientLogID, mergeOptions);

        //    Patients.Patient reloadPatient = null;

        //    using (var dbContext = CreateDbContext())
        //    {
        //        int id = response.TopMatch.PatientId;
        //        reloadPatient = dbContext.Patients.Where(p => p.PatientId == id).FirstOrDefault();
        //        dbContext.Entry<Patients.Patient>(reloadPatient).Collection(p => p.Identifiers).Load();
        //    }

        //    // Assert
        //    Assert.IsNotNull(reloadPatient);
        //    Assert.IsTrue(!reloadPatient.Identifiers.Any(p => p.Type == KnownTypes.MedicaidNumber));
        //}



        [Test]
        public async Task AssociateMergeUpdatesAllOptionalDemographicShouldSucceed()
        {
            Patients.Patient patientClone = patient.Clone();
            patientClone.FirstName = Utilities.GetRandomFirstName();
            patientClone.LastName = Utilities.GetRandomLastName();
            patientClone.MiddleName = Utilities.GetRandomFirstName();
            patientClone.Suffix = Utilities.GetRandomSuffix();

            var matchRequest = patientClone.ToMatchingRequest();
            var response = await patientServices.MatchAsync(matchRequest);
            var log = patientDbContext.MatchLogs.OrderByDescending(t => t.CreateDate).FirstOrDefault();
            AssociateMergeRequest mergeOptions = new()
            {
                SelectedPatientId = response.TopMatch?.PatientId.ToString(),
                FirstName = patientClone.FirstName,
                LastName = patientClone.LastName,
                MiddleName = patientClone.MiddleName,
                Suffix = patientClone.Suffix,
                IsSuffixMatch = true,
                IsMiddleNameMatch = true,

            };

            // Act
            _ = await reviewController.AssociateMerge(log.MatchPatientLogID, mergeOptions);
            var _patient = patientServices.GetByIdentifier("suture-unique-identifier", response.TopMatch.PatientId.ToString()).AsEnumerable().FirstOrDefault();
            patientDbContext.Entry(_patient).Reload();

            // Assert
            Assert.IsNotNull(_patient);
            Assert.IsTrue(_patient.FirstName == patientClone.FirstName);
            Assert.IsTrue(_patient.LastName == patientClone.LastName);
            Assert.IsTrue(_patient.MiddleName == patientClone.MiddleName);
            Assert.IsTrue(_patient.Suffix == patientClone.Suffix);
        }

        [Test]
        public async Task AssociateMergeUpdatesPhonesShouldSucceed()
        {
            Patients.Patient patientClone = patient.Clone();
            patientClone.Phones.Clear();
            patientClone.Phones = new List<PatientPhone>()
            {
                new PatientPhone()
                {
                     Type= ContactType.HomePhone,
                     Value= Utilities.GetRandomPhoneNumber(),
                     IsPrimary= true,
                },
                new PatientPhone()
                {
                     Type= ContactType.WorkPhone,
                     Value=Utilities.GetRandomPhoneNumber(),
                },
                new PatientPhone()
                {
                    Type= ContactType.Mobile,
                    Value=Utilities.GetRandomPhoneNumber()
                },
                new PatientPhone()
                {
                    Type= ContactType.OtherPhone,
                    Value=Utilities.GetRandomPhoneNumber()
                }
            };


            var matchRequest = patientClone.ToMatchingRequest();
            var response = await patientServices.MatchAsync(matchRequest);
            var log = patientDbContext.MatchLogs.OrderByDescending(t => t.CreateDate).FirstOrDefault();
            AssociateMergeRequest mergeOptions = new()
            {
                SelectedPatientId = response.TopMatch?.PatientId.ToString(),
                HomePhone = patientClone.Phones.Where(p=>p.Type== ContactType.HomePhone).Single().Value,
                IsHomePhoneMatch=true,
                WorkPhone = patientClone.Phones.Where(p => p.Type == ContactType.HomePhone).Single().Value,
                IsWorkPhoneMatch = true,
                Mobile = patientClone.Phones.Where(p => p.Type == ContactType.HomePhone).Single().Value,
                IsMobileMatch = true,
                OtherPhone = patientClone.Phones.Where(p => p.Type == ContactType.HomePhone).Single().Value,
                IsOtherPhoneMatch = true,
                PrimaryPhone = "HomePhone"
            };


            // Act           
            _ = await reviewController.AssociateMerge(log.MatchPatientLogID, mergeOptions);


            int patientId = response.TopMatch.PatientId;

            var _patient = patientDbContext.Patients
                                           .Include(p=>p.Phones)
                                           .Where(p => p.PatientId == patientId)
                                           .ToList()
                                           .FirstOrDefault();

            patientDbContext.Entry(_patient).Reload();

            // Assert
            Assert.IsNotNull(_patient);
            Assert.IsTrue(_patient.Phones.Count == patientClone.Phones.Count);
        }

        [Test]
        public async Task AutoMergeTriggeredForAddressDifference()
        {
            // Init
            patient.Addresses.Clear();
            patient.Addresses.Add(new PatientAddress()
            {
                PostalCode = Utilities.GetRandomZip(),
                StateOrProvince = Utilities.GetRandomState(),
                City = Utilities.GetRandomCity(),
            });
            var matchRequest = patient.ToMatchingRequest();

            // Act
            var response = await patientServices.MatchAsync(matchRequest);
            var log = patientDbContext.MatchLogs.OrderByDescending(t => t.CreateDate).FirstOrDefault();
            var result = await reviewController.ResolveMatch(log.MatchPatientLogID);

            // Assert
            Assert.IsTrue(response.MatchLevel == Linq.MatchLevel.Match);
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.That(viewResult.Model, Is.InstanceOf<PatientMatchModel>());
            var model = viewResult.Model as PatientMatchModel;
            Assert.IsTrue(model.PatientMatches.Count == 1);
            Assert.IsTrue(model.IsAutoMerge == true);

        }

        [Test]
        public async Task AutoCreateShouldTriggerForNonMatchRequests()
        {
            // Init
            Patients.Patient patientClone = patient.Clone();

            patientClone.FirstName = Utilities.GetRandomFirstName();
            patientClone.LastName = Utilities.GetRandomLastName();
            patientClone.Birthdate = Utilities.GetRandomDOB();

            patientClone.Identifiers.Clear();
            patientClone.Identifiers.Add(new PatientIdentifier()
            {
                Type = KnownTypes.SocialSecuritySerial,
                Value = Utilities.GetRandomSSN4()
            });
            patientClone.Addresses.Add(new PatientAddress() { PostalCode = Utilities.GetRandomZip() });
            patientClone.Phones.Clear();

            var matchRequest = patientClone.ToMatchingRequest();
            var response = await patientServices.MatchAsync(matchRequest);



            // Act
            var latestLog = patientDbContext.MatchLogs.OrderByDescending(t => t.CreateDate).FirstOrDefault();
            var result = await reviewController.ResolveMatch(latestLog.MatchPatientLogID);

            // Assert
            Assert.IsTrue(response.MatchLevel == Linq.MatchLevel.NonMatch);
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult);
            Assert.That(viewResult.Model, Is.InstanceOf<PatientMatchModel>());
            var model = viewResult.Model as PatientMatchModel;
            Assert.IsTrue(model.IsAutoCreate == true);

        }


    }

    static class Extentions
    {
        private static int memberId = 3000762;
        private static int organizationId = 12264;
        public static PatientMatchingRequest ToMatchingRequest(this Patients.Patient patient)
        {
            var ids = new List<IIdentifier>();
            foreach (var id in patient.Identifiers)
            {
                ids.Add(new PatientIdentifier() { Type = id.Type, Value = id.Value });
            }

            PatientMatchingRequest matchingRequest = new PatientMatchingRequest()
            {

                Phones = patient.Phones,
                Ids = ids,
                Birthdate = patient.Birthdate,
                Gender = patient.Gender,
                FirstName = patient.FirstName,
                MiddleName = patient.MiddleName,
                LastName = patient.LastName,
                Suffix = patient.Suffix,
                LogMatches = true,
                ManualReviewEnabled = true,
                AddressLine1 = patient.Addresses.FirstOrDefault()?.Line1,
                AddressLine2 = patient.Addresses.FirstOrDefault()?.Line2,
                City = patient.Addresses.FirstOrDefault()?.City,
                StateOrProvince = patient.Addresses.FirstOrDefault()?.StateOrProvince,
                PostalCode = patient.Addresses.FirstOrDefault()?.PostalCode,
                RequestSource = RequestSource.SutureHealth,
                MemberId = memberId,
                OrganizationId = organizationId,
            };

            return matchingRequest;
        }

        public static Patients.Patient Clone(this Patients.Patient patient)
        {

            List<PatientIdentifier> identities = new List<PatientIdentifier>();
            List<PatientAddress> addresses = new List<PatientAddress>();
            List<PatientPhone> phones = new List<PatientPhone>();

            foreach (var id in patient.Identifiers)
            {
                identities.Add(new PatientIdentifier() { Type = id.Type, Value = id.Value });
            }
            foreach (var address in patient.Addresses)
            {
                addresses.Add(new PatientAddress() { City = address.City, Line1 = address.Line1, PostalCode = address.PostalCode });
            }
            foreach (var phone in patient.Phones)
            {
                phones.Add(new PatientPhone() { Type = phone.Type, Value = phone.Value, IsPrimary = phone.IsPrimary });
            }

            return new Patients.Patient()
            {
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                MiddleName = patient.MiddleName,
                Suffix = patient.Suffix,
                Addresses = addresses,
                Gender = patient.Gender,
                Birthdate = patient.Birthdate,
                Contacts = patient.Contacts,
                Identifiers = identities,
                Phones = phones,
                OrganizationKeys = patient.OrganizationKeys,
                PatientId = patient.PatientId
            };
        }
    }
}
