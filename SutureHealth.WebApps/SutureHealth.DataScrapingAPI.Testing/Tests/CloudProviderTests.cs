using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SutureHealth.DataScraping;
using SutureHealth.DataScraping.Core;
using SutureHealth.DataScraping.Services.SqlServer;
using SutureHealth.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SutureHealth.DataScrapingAPI.Testing.Tests
{

    /// <summary>
    /// Cloud provider tests are running on localstack(mock aws server) and local ms sql database.
    /// localstack with all required aws elements and local database with identical tables(datascraping schema),
    /// user, pw and user memberships must be running in order to run the cloud provider tests.    
    /// </summary>
    [TestClass]
    internal class CloudProviderTests : IntegrationTestBase
    {
        private SqlServerDataScrapingDbContext _localDbContext;
        public CloudProviderTests()
        {
            string connectionString = String.Format("Data Source=.;Initial Catalog=SutureLocalStackTesting;User Id=localStackTest;Password=pwd123;");
            var optionsBuilder = new DbContextOptionsBuilder<SqlServerDataScrapingDbContext>()
                                                               .UseSqlServer(connectionString);

            _localDbContext = new SqlServerDataScrapingDbContext(optionsBuilder.Options);
        }

        [Test]
        public async Task PushPatientToInsertUpdateQueueCreateTest()
        {
            ScrapedPatient scrapedPatient = new ScrapedPatient()
            {
                URL = "testURL1",
                ExternalId = "12",
                FirstName = "John",
                LastName = "Doe",
                Phone = "04678543267",
                SSN = "testSSN1",
                DateOfBirth = new DateTime(1982, 12, 05),
                AttendedPhysician = "TestPhysician1",
                CreatedAt = DateTime.Now
            };

            //clean ScrapedPatient table before insertion
            _localDbContext.ScrapedPatient.RemoveRange(_localDbContext.ScrapedPatient);
            _localDbContext.SaveChanges();

            var scrapedPatientCountBeforeInsert = _localDbContext.ScrapedPatient.Where(sp => 1 == 1).ToList().Count;
            Dictionary<string, MessageAttributeValue> requestTypeDictionary = new Dictionary<string, MessageAttributeValue>()
            {
                { "RequestType", new MessageAttributeValue() { StringValue = "SimplePatient", DataType="String"} }
            };

            var config = new AmazonSQSConfig
            {
                ServiceURL = "http://localhost:4566",
            };
            var client = new AmazonSQSClient("abc", "123", config);
            var request = new SendMessageRequest()
            {
                MessageBody = JsonSerializer.Serialize(scrapedPatient),
                QueueUrl = "http://localhost:4566/000000000000/InsertUpdatePatientQueue",
                MessageAttributes = requestTypeDictionary
            };
            var res = await client.SendMessageAsync(request);

            Thread.Sleep(8000);
            var scrapedPatientCountAfterInsert = _localDbContext.ScrapedPatient.Where(sp => 1 == 1).ToList().Count;

            NUnit.Framework.Assert.That(scrapedPatientCountAfterInsert, Is.EqualTo(scrapedPatientCountBeforeInsert + 1));

        }

        [Test]
        public async Task PushPatientDetailToInsertUpdateQueueCreateTest()
        {

            List<ObservationHistory> observationHistories = new List<ObservationHistory>()
            {
                new ObservationHistory()
                {
                    Labs= "labs123",
                    Vitals="vitals-abcd",
                }
            };
            List<ContactHistory> contactHistories = new List<ContactHistory>()
            {
                new ContactHistory()
                {
                    ContactText="1238758764",
                    ContactType=DataScraping.ContactType.MobilePhone,
                },
                new ContactHistory()
                {
                    ContactText="testemail",
                    ContactType=DataScraping.ContactType.Email,
                },
                new ContactHistory()
                {
                    ContactText="067482622972",
                    ContactType=DataScraping.ContactType.HomePhone,
                },
            };
            List<ConditionHistory> conditionHistories = new List<ConditionHistory>()
            {
                new ConditionHistory()
                {
                    Diagnosis="testDiagnosis",
                    DiagnosisCode="testDiagnosisCode",
                    StartDate = new DateTime(2012,04,12),
                    EndDate = null
                },
                new ConditionHistory()
                {
                    Diagnosis="testDiagnosis2",
                    DiagnosisCode="testDiagnosisCode2",
                    StartDate = new DateTime(2020,11,11),
                    EndDate = new DateTime(2022,01,14)
                }
            };

            List<AllergyHistory> allergyHistories = new List<AllergyHistory>()
            {
                new AllergyHistory()
                {
                    Name="testAlergy1",
                    Code="allergyCode1",
                    Reaction="testReaction",
                    Severity="severe!",
                    StartDate = new DateTime(2018,12,04),
                    EndDate = null
                }
            };

            List<MedicationHistory> medicationHistories = new List<MedicationHistory>()
            {
                new MedicationHistory()
                {
                    Name="medication1",
                    Code="medicationCode1",
                    StartDate=new DateTime(1999,02,04),
                    EndDate=new DateTime(2002,08,14)
                },
                new MedicationHistory()
                {
                    Name="medication2",
                    Code="medicationCode2",
                    StartDate=new DateTime(2014,06,13),
                    EndDate=new DateTime(2016,08,14)
                },
                new MedicationHistory()
                {
                    Name="medication2",
                    Code="medicationCode2",
                    StartDate=new DateTime(2021,11,06),
                    EndDate=null
                }
            };

            List<ImmunizationHistory> immunizationHistories = new List<ImmunizationHistory>()
            {
                new ImmunizationHistory()
                {
                    Name="ImmuniationName1",
                    Code="ImmunizationCode1",
                    AdministrationDate=new DateTime(2022,06,15),
                    ExpirationDate= null
                }
            };

            List<PrescriptionHistory> prescriptionHistories = new List<PrescriptionHistory>()
            {
                new PrescriptionHistory()
                {
                    DrugName="DrugName1",
                    Details="prescriptionDetails1",
                    FillDate= new DateTime(2021,07,07),
                    Quantity="12",
                    Refills="refills"
                }
            };

            List<ProcedureHistory> procedureHistories = new List<ProcedureHistory>()
            {
                new ProcedureHistory()
                {
                    Reason="Reason1",
                    Billing="Billing1",
                    Insurance="testInsurance1",
                    Issue="testIssue1",
                    Provider="testProvider1",
                    Date=new DateTime(2021,09,24)
                }
            };

            ScrapedPatientDetailHistory scrapedPatientDetail = new ScrapedPatientDetailHistory()
            {
                URL = "testURL",
                ExternalId = "72",
                FirstName = "John",
                MiddleName = "Harry",
                LastName = "Doe",
                DateOfBirth = new DateTime(1962, 06, 21),
                Gender = "Male",
                MaritalStatus = "Married",
                SexualOrientation = "unknown",
                AttendedPhysician = "testPhysician",
                Address = "testAdress1123",
                City = "New York",
                Country = "USA",
                PostalCode = "",
                PatientBalance = "4500",
                InsuranceBalance = "5500",
                TotalBalance = "10000",
                Occupation = "chef",
                Employer = "employer ABC",
                Language = "English",
                Ethnicity = "Caucasian",
                Race = "race1",
                FamilySize = "4",
                MonthlyIncome = "10000",
                Religion = "Christian",
                DeceaseDate = "",
                DeceaseReason = "",
                SSN = "test-ssn123",
                CreatedAt = DateTime.UtcNow,

                Observations = observationHistories,
                Contacts = contactHistories,
                Conditions = conditionHistories,
                Allergies = allergyHistories,
                Medications = medicationHistories,
                Immunizations = immunizationHistories,
                Prescriptions = prescriptionHistories,
                Procedures = procedureHistories
            };

            //clean db tables
            var dbScrapedPatientDetails = _localDbContext.ScrapedPatientDetail
                                                                            .Include(spd => spd.Allergies)
                                                                            .Include(spd => spd.Conditions)
                                                                            .Include(spd => spd.Contacts)
                                                                            .Include(spd => spd.Immunizations)
                                                                            .Include(spd => spd.Medications)
                                                                            .Include(spd => spd.Observations)
                                                                            .Include(spd => spd.Prescriptions)
                                                                            .Include(spd => spd.Procedures)
                                                                            .Where(spd => 1 == 1).ToList();
            _localDbContext.RemoveRange(dbScrapedPatientDetails);
            _localDbContext.SaveChanges();
                        
            int observationsCount = scrapedPatientDetail.Observations.Count;
            int contactsCount = scrapedPatientDetail.Contacts.Count;
            int conditionsCount = scrapedPatientDetail.Conditions.Count;
            int allergiesCount = scrapedPatientDetail.Allergies.Count;
            int medicationsCount = scrapedPatientDetail.Medications.Count;
            int immunizationsCount = scrapedPatientDetail.Immunizations.Count;
            int prescriptionsCount = scrapedPatientDetail.Prescriptions.Count;
            int proceduresCount = scrapedPatientDetail.Procedures.Count;

            var scrapedPatientDetailCountBeforeInsert = _localDbContext.ScrapedPatientDetail.Where(sp => 1 == 1).ToList().Count;
            Dictionary<string, MessageAttributeValue> requestTypeDictionary = new Dictionary<string, MessageAttributeValue>()
            {
                { "RequestType", new MessageAttributeValue() { StringValue = "DetailedPatient", DataType="String"} }
            };

            var config = new AmazonSQSConfig
            {
                ServiceURL = "http://localhost:4566",
            };
            var client = new AmazonSQSClient("abc", "123", config);
            var request = new SendMessageRequest()
            {
                MessageBody = JsonSerializer.Serialize(scrapedPatientDetail),
                QueueUrl = "http://localhost:4566/000000000000/InsertUpdatePatientQueue",
                MessageAttributes = requestTypeDictionary
            };
            var res = await client.SendMessageAsync(request);
            
            Thread.Sleep(10000);
            var scrapedPatientDetailCountAfterInsert = _localDbContext.ScrapedPatientDetail.Where(sp => 1 == 1).ToList().Count;
            var dbScrapedPatientDetail = _localDbContext.ScrapedPatientDetail
                                                                        .Include(spd => spd.Allergies)
                                                                        .Include(spd => spd.Conditions)
                                                                        .Include(spd => spd.Contacts)
                                                                        .Include(spd => spd.Immunizations)
                                                                        .Include(spd => spd.Medications)
                                                                        .Include(spd => spd.Observations)
                                                                        .Include(spd => spd.Prescriptions)
                                                                        .Include(spd => spd.Procedures)
                                                                        .Where(spd => 1 == 1)
                                                                        .FirstOrDefault();

            NUnit.Framework.Assert.That(scrapedPatientDetailCountAfterInsert, Is.EqualTo(scrapedPatientDetailCountBeforeInsert + 1));
            NUnit.Framework.Assert.That(observationsCount, Is.EqualTo(dbScrapedPatientDetail.Observations.Count));
            NUnit.Framework.Assert.That(contactsCount, Is.EqualTo(dbScrapedPatientDetail.Contacts.Count));
            NUnit.Framework.Assert.That(conditionsCount, Is.EqualTo(dbScrapedPatientDetail.Conditions.Count));
            NUnit.Framework.Assert.That(allergiesCount, Is.EqualTo(dbScrapedPatientDetail.Allergies.Count));
            NUnit.Framework.Assert.That(medicationsCount, Is.EqualTo(dbScrapedPatientDetail.Medications.Count));
            NUnit.Framework.Assert.That(immunizationsCount, Is.EqualTo(dbScrapedPatientDetail.Immunizations.Count));
            NUnit.Framework.Assert.That(prescriptionsCount, Is.EqualTo(dbScrapedPatientDetail.Prescriptions.Count));
            NUnit.Framework.Assert.That(proceduresCount, Is.EqualTo(dbScrapedPatientDetail.Procedures.Count));

        }

        [Test]
        public async Task PushPatientToInsertUpdateQueueUpdateTest()
        {
            ScrapedPatient scrapedPatient = new ScrapedPatient()
            {
                URL = "testURL2",
                ExternalId = "8",
                FirstName = "Jane",
                LastName = "Smith",
                Phone = "0874567241",
                SSN = "testSSN2",
                DateOfBirth = new DateTime(1992, 10, 24),
                AttendedPhysician = "TestPhysician2",
                CreatedAt = DateTime.Now
            };           
                        

            _localDbContext.ScrapedPatient.RemoveRange(_localDbContext.ScrapedPatient);
            _localDbContext.ScrapedPatient.Add(scrapedPatient);
            _localDbContext.SaveChanges();
            
            scrapedPatient.FirstName = "updatedFirstName";
            scrapedPatient.MiddleName = "updatedMiddleName";
            scrapedPatient.LastName = "updatedLastName";
            scrapedPatient.SSN = "updatedSSn";
            scrapedPatient.DateOfBirth = new DateTime(1986, 03, 08);
            scrapedPatient.AttendedPhysician = "updatedPhysician";
            scrapedPatient.CreatedAt = DateTime.Now;            

            Dictionary<string, MessageAttributeValue> requestTypeDictionary = new Dictionary<string, MessageAttributeValue>()
            {
                { "RequestType", new MessageAttributeValue() { StringValue = "SimplePatient", DataType="String"} }
            };

            var config = new AmazonSQSConfig
            {
                ServiceURL = "http://localhost:4566",
            };
            var client = new AmazonSQSClient("abc", "123", config);
            var request = new SendMessageRequest()
            {
                MessageBody = JsonSerializer.Serialize(scrapedPatient),
                QueueUrl = "http://localhost:4566/000000000000/InsertUpdatePatientQueue",
                MessageAttributes = requestTypeDictionary
            };
            var res = await client.SendMessageAsync(request);
            Thread.Sleep(8000);

            var scrapedPatientCount = _localDbContext.ScrapedPatient.Where(sp => 1 == 1).ToList().Count;
            NUnit.Framework.Assert.That(scrapedPatientCount, Is.EqualTo(1));            
        }

        [Test]
        public async Task PushPatientDetailToInsertUpdateQueueUpdateTest()
        {

            List<Observation> observations= new List<Observation>()
            {
                new Observation()
                {
                    Labs= "labs123",
                    Vitals="vitals-abcd",
                }
            };
            List<Contact> contacts = new List<Contact>()
            {
                new Contact()
                {
                    ContactText="1238758764",
                    ContactType=DataScraping.ContactType.MobilePhone,
                },
                new Contact()
                {
                    ContactText="testemail",
                    ContactType=DataScraping.ContactType.Email,
                },
                new Contact()
                {
                    ContactText="067482622972",
                    ContactType=DataScraping.ContactType.HomePhone,
                },
            };
            List<Condition> conditions = new List<Condition>()
            {
                new Condition()
                {
                    Diagnosis="testDiagnosis",
                    DiagnosisCode="testDiagnosisCode",
                    StartDate = new DateTime(2012,04,12),
                    EndDate = null
                },
                new Condition()
                {
                    Diagnosis="testDiagnosis2",
                    DiagnosisCode="testDiagnosisCode2",
                    StartDate = new DateTime(2020,11,11),
                    EndDate = new DateTime(2022,01,14)
                }
            };

            List<Allergy> allergies = new List<Allergy>()
            {
                new Allergy()
                {
                    Name="testAlergy1",
                    Code="allergyCode1",
                    Reaction="testReaction",
                    Severity="severe!",
                    StartDate = new DateTime(2018,12,04),
                    EndDate = null
                }
            };

            List<Medication> medications = new List<Medication>()
            {
                new Medication()
                {
                    Name="medication1",
                    Code="medicationCode1",
                    StartDate=new DateTime(1999,02,04),
                    EndDate=new DateTime(2002,08,14)
                },
                new Medication()
                {
                    Name="medication2",
                    Code="medicationCode2",
                    StartDate=new DateTime(2014,06,13),
                    EndDate=new DateTime(2016,08,14)
                },
                new Medication()
                {
                    Name="medication2",
                    Code="medicationCode2",
                    StartDate=new DateTime(2021,11,06),
                    EndDate=null
                }
            };

            List<Immunization> immunizations = new List<Immunization>()
            {
                new Immunization()
                {
                    Name="ImmuniationName1",
                    Code="ImmunizationCode1",
                    AdministrationDate=new DateTime(2022,06,15),
                    ExpirationDate= null
                }
            };

            List<Prescription> prescriptions = new List<Prescription>()
            {
                new Prescription()
                {
                    DrugName="DrugName1",
                    Details="prescriptionDetails1",
                    FillDate= new DateTime(2021,07,07),
                    Quantity="12",
                    Refills="refills"
                }
            };

            List<Procedure> procedures = new List<Procedure>()
            {
                new Procedure()
                {
                    Reason="Reason1",
                    Billing="Billing1",
                    Insurance="testInsurance1",
                    Issue="testIssue1",
                    Provider="testProvider1",
                    Date=new DateTime(2021,09,24)
                }
            };

            ScrapedPatientDetail scrapedPatientDetail = new ScrapedPatientDetail()
            {
                URL = "testURL",
                ExternalId = "72",
                FirstName = "John",
                MiddleName = "Harry",
                LastName = "Doe",
                DateOfBirth = new DateTime(1962, 06, 21),
                Gender = "Male",
                MaritalStatus = "Married",
                SexualOrientation = "unknown",
                AttendedPhysician = "testPhysician",
                Address = "testAdress1123",
                City = "New York",
                Country = "USA",
                PostalCode = "",
                PatientBalance = "4500",
                InsuranceBalance = "5500",
                TotalBalance = "10000",
                Occupation = "chef",
                Employer = "employer ABC",
                Language = "English",
                Ethnicity = "Caucasian",
                Race = "race1",
                FamilySize = "4",
                MonthlyIncome = "10000",
                Religion = "Christian",
                DeceaseDate = "",
                DeceaseReason = "",
                SSN = "test-ssn123",
                CreatedAt = DateTime.UtcNow,

                Observations = observations,
                Contacts = contacts,
                Conditions = conditions,
                Allergies = allergies,
                Medications = medications,
                Immunizations = immunizations,
                Prescriptions = prescriptions,
                Procedures = procedures
            };
           

            _localDbContext.ScrapedPatientDetail.RemoveRange(_localDbContext.ScrapedPatientDetail);
            _localDbContext.ScrapedPatientDetail.Add(scrapedPatientDetail);
            _localDbContext.SaveChanges();

            scrapedPatientDetail.FirstName = "updatedFirstName";
            scrapedPatientDetail.MiddleName = "updatedMiddleName";
            scrapedPatientDetail.LastName = "updatedLastName";
            scrapedPatientDetail.SSN = "updatedSSn";
            scrapedPatientDetail.DateOfBirth = new DateTime(1994, 07, 04);
            scrapedPatientDetail.AttendedPhysician = "updatedPhysician";
            scrapedPatientDetail.CreatedAt = DateTime.Now;

           

            Dictionary<string, MessageAttributeValue> requestTypeDictionary = new Dictionary<string, MessageAttributeValue>()
            {
                { "RequestType", new MessageAttributeValue() { StringValue = "SimplePatient", DataType="String"} }
            };

            var config = new AmazonSQSConfig
            {
                ServiceURL = "http://localhost:4566",
            };
            var client = new AmazonSQSClient("abc", "123", config);
            var request = new SendMessageRequest()
            {
                MessageBody = JsonSerializer.Serialize(scrapedPatientDetail),
                QueueUrl = "http://localhost:4566/000000000000/InsertUpdatePatientQueue",
                MessageAttributes = requestTypeDictionary
            };
            var res = await client.SendMessageAsync(request);
            Thread.Sleep(10000);

            var scrapedPatientDetailCount = _localDbContext.ScrapedPatientDetail.Where(sp => 1 == 1).ToList().Count;
            NUnit.Framework.Assert.That(scrapedPatientDetailCount, Is.EqualTo(1));           
        }

        [Test]
        public async Task PushHtmlToBucketMustCreateObject()
        {
            ScrapPatientHtmlRequest scrapPatientHtmlRequest = new ScrapPatientHtmlRequest()
            {
                OrganizationName = "testOrganization",
                PageName = "testPageName",
                PageUrl = "testUrl",
                ScrapedAt = DateTime.Now,
                Html = File.ReadAllText(@"..\net6.0\HtmlSamples\OpenEmrPatientSample.txt")
            };

            var config = new AmazonS3Config
            {                   
                ServiceURL = "http://localhost:4566/",
                ForcePathStyle = true
            };
            var credentials = new BasicAWSCredentials("abc", "123");
            var s3Client = new AmazonS3Client(credentials, config);
            IStorageService storageService = new SimpleStorageService(s3Client, Configuration);
            StorageServicePutRequest htmlPutRequest = new StorageServicePutRequest()
            {
                ContentBody = scrapPatientHtmlRequest.Html,
                ContentType = "text/html",
                Container = "html-bucket",
                Key = scrapPatientHtmlRequest.ScrapedAt.ToString()
            };
            await storageService.PutObjectAsync(htmlPutRequest);

            GetObjectRequest getObjectRequest = new GetObjectRequest()
            {
                BucketName = "html-bucket",
                Key = scrapPatientHtmlRequest.ScrapedAt.ToString()
            };
            GetObjectResponse s3ObjectResponse = await s3Client.GetObjectAsync(getObjectRequest);

            NUnit.Framework.Assert.That(s3ObjectResponse, Is.Not.Null);

        }
    }
}
