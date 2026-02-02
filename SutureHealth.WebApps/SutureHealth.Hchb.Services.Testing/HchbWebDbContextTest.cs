using NUnit.Framework;
using SutureHealth.Hchb.Services.SqlServer;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Patients.Services.SqlServer;
using Microsoft.Data.SqlClient;
using Kendo.Mvc.Extensions;
using Microsoft.Extensions.Logging;
using SutureHealth.Hchb.Services.Testing.Utility;

namespace SutureHealth.Hchb.Services.Testing
{
    public class HchbWebDbContextTest
    {
        protected HchbWebDbContext commonSpiritDbContext;
        protected HchbWebDbContext accuCareDbContext;


        [SetUp]
        public void Setup()
        {
            try
            {
                DotNetEnv.Env.Load();
                DotNetEnv.Env.TraversePath().Load();

                var connString = new SqlConnectionStringBuilder()
                {
                    DataSource = DotNetEnv.Env.GetString("SqlDatabase__DataSource"),
                    UserID = DotNetEnv.Env.GetString("SqlDatabase__UserID"),
                    Password = DotNetEnv.Env.GetString("SqlDatabase__Password"),
                    InitialCatalog = DotNetEnv.Env.GetString("SqlDatabase__InitialCatalog"),
                    ApplicationName = nameof(HchbWebDbContextTest),
                    ConnectTimeout = 5,
                    MinPoolSize = 5,
                    Pooling = true,
                    Encrypt = true,
                    TrustServerCertificate = true
                };
                DbContextOptionsBuilder<SqlServerHchbWebDbContext> primaryOptionBuilder = new();
                primaryOptionBuilder.UseSqlServer(connString.ToString())
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
                
                commonSpiritDbContext = new SqlServerHchbWebDbContext(primaryOptionBuilder.Options, new DbContextSchema("HCHB_AccuCare"));
                accuCareDbContext = new SqlServerHchbWebDbContext(primaryOptionBuilder.Options,new DbContextSchema("HCHB_AccuCare"));

            }
            catch (Exception)
            {

                throw;
            }
            
        }

        [TearDown]
        public void Clean()
        {
            commonSpiritDbContext.SaveChanges();
            commonSpiritDbContext.Dispose();
            
            accuCareDbContext.SaveChanges();
            accuCareDbContext.Dispose();
        }


        [Test]
        [Category("Integration")]
        public void CreatePatientAndInsertIntoDatabaseShouldSuccess()
        {

            // Init
            HchbPatientWeb hchbPatient = new HchbPatientWeb()
            {
                PatientId = 1012446,
                EpisodeId = Utilities.GetRandomAlphabeticString(5),
                HchbPatientId = "1",
                Status = "s1",
                IcdCode = "icd1",
            };

            // Act
            try
            {
                commonSpiritDbContext.HchbPatients.Add(hchbPatient);
                commonSpiritDbContext.SaveChanges();
            }
            catch (Exception)
            {

                Assert.Fail("failed to create hchb patient through Patient view in Api database.");
            }

            // Assert
            int countOfPatients = commonSpiritDbContext.HchbPatients.Count();
            Assert.IsTrue(countOfPatients != 0);
        }

        [Test]
        [Category("Integration")]
        public void CreatePatientThenGetThePatientFromDatabaseShouldSuccess()
        {

            // Init
            HchbPatientWeb hchbPatient = new HchbPatientWeb()
            {
                PatientId = 1012446,
                EpisodeId = Utilities.GetRandomAlphabeticString(5),
                HchbPatientId = "2",
                Status = "s1",
                IcdCode = "icd1",
            };

            HchbPatientWeb? patient = null;

            // Act
            try
            {
                commonSpiritDbContext.HchbPatients.Add(hchbPatient);
                commonSpiritDbContext.SaveChanges();
                patient = commonSpiritDbContext.HchbPatients.Where(p => p.HchbPatientId == "2").FirstOrDefault();
            }
            catch (Exception)
            {

                Assert.Fail("failed to create hchb patient through Patient view in Api database.");
            }

            // Assert
            Assert.IsNotNull(patient);
        }

        [Test]
        [Category("Integration")]
        public void CreatePatientThenGetThePatientStatusShouldRetrieveCorrectValue()
        {

            // Init
            HchbPatientWeb hchbPatient = new HchbPatientWeb()
            {
                PatientId = 1012445,
                EpisodeId = Utilities.GetRandomAlphabeticString(5),
                HchbPatientId = "2",
                Status = "submitted",
                IcdCode = "icd1",
            };

            string? status = string.Empty;

            // Act
            try
            {
                commonSpiritDbContext.HchbPatients.Add(hchbPatient);
                commonSpiritDbContext.SaveChanges();
                status = GetHchbPatientStatus(1012445).Result;
            }
            catch (Exception)
            {

                Assert.Fail("failed to create hchb patient through Patient view in Api database.");
            }

            // Assert
            Assert.IsTrue(status == "submitted");
        }

        [Test]
        [Category("Integration")]
        public void CreateTemplateThenRetrieveShouldSuccess()
        {

            int id = commonSpiritDbContext.Templates.Count() + 1;
            // Init
            HchbTemplate template = new HchbTemplate()
            {
                Id = id,
                AdmissionType = "type_1",
                ObservationId = "observ_1",
                ObservationText = "obs_test",
                PatientType = "type_1",
                TemplateId = 1001
            };

            // Act
            try
            {
                commonSpiritDbContext.Templates.Add(template);
                commonSpiritDbContext.SaveChanges();
            }
            catch (Exception)
            {

                Assert.Fail("failed to create template through Hchb Template view in Api database.");
            }

            // Assert
            int count = commonSpiritDbContext.Templates.Count();
            Assert.IsTrue(count != 0);
        }

        [Test]
        [Category("Integration")]
        public void CreateTemplateThenSearchForItShouldSuccess()
        {

            int id = commonSpiritDbContext.Templates.Count() + 1;
            string obsId = Utilities.GetRandomAlphabeticString(10);
            string obsTxt = Utilities.GetRandomAlphabeticString(10);
            // Init
            HchbTemplate template = new HchbTemplate()
            {
                Id = id,
                AdmissionType = "Admit_5",
                ObservationId = obsId,
                ObservationText = obsTxt,
                PatientType = "created",
                TemplateId = 1001
            };

            HchbTemplate? searchResult = null;
            // Act
            try
            {
                commonSpiritDbContext.Templates.Add(template);
                commonSpiritDbContext.SaveChanges();
                searchResult = GetHchbTemplate("Admit_5", obsId, obsTxt, "created");
            }
            catch (Exception)
            {

                Assert.Fail("failed to create template through Hchb Template view in Api database.");
            }

            // Assert            
            Assert.IsNotNull(searchResult);
            Assert.IsTrue(searchResult?.Id == id && searchResult?.TemplateId == 1001);
        }

        [Test]
        [Category("Integration")]
        public async Task CreateHL7MessageLogThenStoreInDatabaseShouldSuccess()
        {
            // Init
            string hchbId = Utilities.GetRandomString(10);
            string episodId = Utilities.GetRandomAlphabeticString(10);
            HchbPatientWeb hchbPatientWeb = new HchbPatientWeb() 
            {
                PatientId= 1012444,
                EpisodeId=episodId,
                HchbPatientId=hchbId,
                Status = "submitted",
                IcdCode = "U70.1"
            };
            await commonSpiritDbContext.HchbPatients.AddAsync(hchbPatientWeb);
            commonSpiritDbContext.SaveChanges();

            HL7MessageLog log = new HL7MessageLog()
            {                
                MessageControlId = Utilities.GetRandomAlphabeticString(5) + "_" + Utilities.GetRandomDecimalString(5),
                Type = "ADT",
                SubType = "A01",
                EpisodeId = episodId,
                HchbPatientId = hchbId,
                Message = "this is test message",
                RawMessageFile = "ADT/raw/ADT-A04-202310031530040R600062098312.txt",
                JsonMessageFile = "ADT/json/ADT-A04-202310031530040R600062098312.json",
                ReceivedDate = DateTime.Today,
                Reason = "test local database",
                IsProcessed = false
            };
            
            int countBeforeInsertion = commonSpiritDbContext.Logs.Count();
            
            int countAfterInsertion = 0;
            int? id = -1;
            // Act
            try
            {
                id = await LogHl7Message(log);
                commonSpiritDbContext.SaveChanges();
                countAfterInsertion = commonSpiritDbContext.Logs.Count();
            }
            catch (Exception)
            {

                Assert.Fail("failed to create log hl7 message through HL7LogMessage view in Api database.");
            }

            // Assert            

            Assert.IsTrue(id > 0);
            Assert.IsTrue((countBeforeInsertion + 1) == countAfterInsertion);

        }

        [Test]
        [Category("Integration")]
        public async Task CreateHL7MessageLogThenSetProcessedFlagToTrueShouldSuccess()
        {
            string messageControlId = Utilities.GetRandomAlphabeticString(5) + "_" + Utilities.GetRandomDecimalString(5);
            // Init
            HL7MessageLog log = new HL7MessageLog()
            {
                MessageControlId = messageControlId,
                Type = "ADT",
                SubType = "A01",
                Message = "this is test message",
                RawMessageFile = "ADT/raw/ADT-A04-202310031530040R600062098312.txt",
                JsonMessageFile = "ADT/json/ADT-A04-202310031530040R600062098312.json",
                ReceivedDate = DateTime.Now,
                Reason = "test local database",
                IsProcessed = false
            };
            HL7MessageLog recoveredLog = new HL7MessageLog();
            int? id = -1;
            // Act
            try
            {
                id = await LogHl7Message(log);
                commonSpiritDbContext.SaveChanges();

                await LogProcessedMessage(id);
                recoveredLog = commonSpiritDbContext.Logs.Where(l => l.Id == id).First();

            }
            catch (Exception)
            {

                Assert.Fail("failed to create log hl7 message through HL7LogMessage view in Api database.");
            }

            // Assert            

            Assert.IsTrue(id > 0);
            Assert.IsNotNull(recoveredLog);
            Assert.IsTrue(recoveredLog.IsProcessed);

        }

        [Test]
        [Category("Integration")]
        public async Task CreateHL7MessageLogThenLogReasonShouldUpdateReason()
        {
            string messageControlId = Utilities.GetRandomAlphabeticString(5) + "_" + Utilities.GetRandomDecimalString(5);

            // Init
            string episodId = Utilities.GetRandomAlphabeticString(5);
            HchbPatientWeb patient = new HchbPatientWeb() 
            { 
                PatientId= 1012444,
                EpisodeId = episodId,
                Status= "submitted",
                IcdCode="U70.1"
            };

            commonSpiritDbContext.HchbPatients.Add(patient);
            commonSpiritDbContext.SaveChanges();

            HL7MessageLog log = new HL7MessageLog()
            {
                MessageControlId = messageControlId,
                HchbPatientId= "1012444",
                EpisodeId= "XFHCXVIZQJ",
                Type = "ADT",
                SubType = "A01",
                Message = "this is test message",
                RawMessageFile = "ADT/raw/ADT-A04-202310031530040R600062098312.txt",
                JsonMessageFile = "ADT/json/ADT-A04-202310031530040R600062098312.json",
                ReceivedDate = DateTime.Now,
                IsProcessed = false
            };
            HL7MessageLog recoveredLog = new HL7MessageLog();
            int? id = -1;
            // Act
            try
            {
                id = await LogHl7Message(log);
                commonSpiritDbContext.SaveChanges();

                LogReason(id, "updated log reason").Wait();
                recoveredLog = commonSpiritDbContext.Logs.Where(l => l.Id == id).First();

            }
            catch (Exception)
            {

                Assert.Fail("failed to create log hl7 message through HL7LogMessage view in Api database.");
            }

            // Assert            

            Assert.IsTrue(id > 0);
            Assert.IsNotNull(recoveredLog);
            Assert.IsTrue(recoveredLog.Reason.EqualsIgnoreCase("updated log reason"));

        }

        [Test]
        [Category("Integration")]
        public async Task CreateUnprocessedHL7MessageLogThenSetProcessFlagShouldUpdateReason()
        {
            string messageControlId = Utilities.GetRandomAlphabeticString(5) + "_" + Utilities.GetRandomDecimalString(5);
            // Init
            HL7MessageLog log = new HL7MessageLog()
            {
                MessageControlId = messageControlId,
                Type = "ADT",
                SubType = "A01",
                Message = "this is test message",
                RawMessageFile = "ADT/raw/ADT-A04-202310031530040R600062098312.txt",
                JsonMessageFile = "ADT/json/ADT-A04-202310031530040R600062098312.json",
                ReceivedDate = DateTime.Now,
                IsProcessed = false
            };
            HL7MessageLog recoveredLog = new HL7MessageLog();
            int? id = -1;
            int countBeforeInsertion = GetUnprocessedMessage().Count();
            int countAfterInsertion = 0;
            // Act
            try
            {
                id = await LogHl7Message(log);
                commonSpiritDbContext.SaveChanges();
                countAfterInsertion = GetUnprocessedMessage().Count();
            }
            catch (Exception)
            {

                Assert.Fail("failed to create log hl7 message through HL7LogMessage view in Api database.");
            }

            // Assert            

            Assert.IsTrue(countAfterInsertion == countBeforeInsertion + 1);
        }


        [Test]
        [Category("Integration")]
        public async Task CreateUnprocessedHL7MessageLogThenCheckIfProcessedFlagShouldReturnFalse()
        {
            string messageControlId = Utilities.GetRandomAlphabeticString(5) + "_" + Utilities.GetRandomDecimalString(5);
            // Init
            HL7MessageLog log = new HL7MessageLog()
            {
                MessageControlId = messageControlId,
                Type = "ADT",
                SubType = "A01",
                Message = "this is test message",
                RawMessageFile = "ADT/raw/ADT-A04-202310031530040R600062098312.txt",
                JsonMessageFile = "ADT/json/ADT-A04-202310031530040R600062098312.json",
                ReceivedDate = DateTime.Today,
                IsProcessed = false
            };
            HL7MessageLog recoveredLog = new HL7MessageLog();
            int? id = -1;

            bool? isProcessed = true;
            // Act
            try
            {
                id = await LogHl7Message(log);
                commonSpiritDbContext.SaveChanges();
                isProcessed = IsProcessedMessage(id);
            }
            catch (Exception)
            {

                Assert.Fail("failed to create log hl7 message through HL7LogMessage view in Api database.");
            }

            // Assert            

            Assert.IsTrue(isProcessed == false);
        }

        [Test]
        [Category("Integration")]
        public void GetSignerPrimaryFacilityShouldSuccess() 
        {
            // Init
            int signerId = 3006165;
            int facilityId = 1000;
            int patientId = 1012444;
            string episodeId = Utilities.GetRandomAlphabeticString(10);
            string hchbPatientId = Utilities.GetRandomAlphabeticString(10);

            HchbPatientWeb hchbPatientWeb = new HchbPatientWeb()
            {
                EpisodeId = episodeId,
                PatientId = patientId,                
                HchbPatientId = hchbPatientId
            };
            commonSpiritDbContext.Add(hchbPatientWeb);
            commonSpiritDbContext.SaveChanges();

            HchbTransaction transaction = new()
            {
                RequestId = 1,
                SignerId = signerId,
                SignerFacilityId = facilityId,
                OrderNumber = Utilities.GetRandomString(10),
                EpisodeId = episodeId,
                AdmitDate = DateTime.Today,
                SendDate = DateTime.Today,
                OrderDate = DateTime.Today,
                HchbPatientId = hchbPatientId
            };

            commonSpiritDbContext.HchbTransactions.Add(transaction);
            commonSpiritDbContext.SaveChanges();
            // Act
            int _facilityId = GetSignerPrimaryFacility(signerId);

            // Assert
            Assert.IsTrue(_facilityId == facilityId);

        }

        [Test]
        [Category("Integration")]
        public void GetFacilityIdFromBranchCodeShouldSuccess()
        {
            // Init
            string branchCode = "0E4";

            // Act
            int facilityId = GetFacilityIdFromBranchCode(branchCode);

            // Assert
            Assert.IsTrue(facilityId == 13359);

        }

        [Test]
        [Category("Integration")]
        public void CreateTransactionAndInsertIntoDataSetThenSaveChangeToDatabaseShouldSuccess()
        {
            // Init
            int patientId = 1012444;
            string episodeId = Utilities.GetRandomAlphabeticString(10);
            string hchbPatientId = Utilities.GetRandomAlphabeticString(10);

            HchbPatientWeb hchbPatientWeb = new HchbPatientWeb() 
            {
                EpisodeId= episodeId,
                PatientId= patientId,
                HchbPatientId= hchbPatientId
            };
            commonSpiritDbContext.Add(hchbPatientWeb);
            commonSpiritDbContext.SaveChanges();

            HchbTransaction transaction = new()
            {
                RequestId = 1,
                //AdmitDate = DateTime.Now,
                OrderNumber = Utilities.GetRandomString(10),                
                EpisodeId = episodeId,
                HchbPatientId= hchbPatientId
            };
            int countBeforeInsertion = commonSpiritDbContext.HchbTransactions.Count();

            // Act
            commonSpiritDbContext.HchbTransactions.Add(transaction);
            commonSpiritDbContext.SaveChanges();
            int countAfterInsertion = commonSpiritDbContext.HchbTransactions.Count();

            // Assert
            Assert.IsTrue(countAfterInsertion == countBeforeInsertion + 1);

        }


        [Test]
        [Category("Integration")]
        public void CreatePatientAndInsertIntoTwoDifferentSchemasIntoDatabaseShouldSuccess()
        {

            // Init
            HchbPatientWeb hchbPatientOne = new HchbPatientWeb()
            {
                PatientId = 1012446,
                EpisodeId = Utilities.GetRandomAlphabeticString(5),
                HchbPatientId = "10",
                Status = "s1",
                IcdCode = "icd1",
            };
            HchbPatientWeb hchbPatientTwo = new HchbPatientWeb()
            {
                PatientId = 1012446,
                EpisodeId = Utilities.GetRandomAlphabeticString(5),
                HchbPatientId = "10",
                Status = "s1",
                IcdCode = "icd1",
            };

            // Act
            try
            {
                commonSpiritDbContext.HchbPatients.Add(hchbPatientOne);
                commonSpiritDbContext.SaveChanges();

                accuCareDbContext.HchbPatients.Add(hchbPatientTwo);
                accuCareDbContext.SaveChanges();
            }
            catch (Exception)
            {

                Assert.Fail("failed to create hchb patient through Patient view in Api database.");
            }

            // Assert
            int countOfPatients = commonSpiritDbContext.HchbPatients.Count();
            Assert.IsTrue(countOfPatients != 0);
            countOfPatients = accuCareDbContext.HchbPatients.Count();
            Assert.IsTrue(countOfPatients != 0);
        }


        [Test]
        [Category("Integration")]
        public void CreatePatientThenInsertIntoTwoDifferentSchemasThenGetThePatientStatusShouldRetrieveCorrectValue()
        {

            // Init
            HchbPatientWeb hchbPatientOne = new HchbPatientWeb()
            {
                PatientId = 1012445,
                EpisodeId = Utilities.GetRandomAlphabeticString(5),
                HchbPatientId = "2",
                Status = "submitted",
                IcdCode = "icd1",
            };

            HchbPatientWeb hchbPatientTwo = new HchbPatientWeb()
            {
                PatientId = 1012445,
                EpisodeId = Utilities.GetRandomAlphabeticString(5),
                HchbPatientId = "2",
                Status = "submitted",
                IcdCode = "icd1",
            };
            
            string? statusOne = string.Empty;
            string? statusTwo = string.Empty;

            // Act
            try
            {
                commonSpiritDbContext.HchbPatients.Add(hchbPatientOne);
                commonSpiritDbContext.SaveChanges();
                statusOne = GetHchbPatientStatus(1012445).Result;

                accuCareDbContext.HchbPatients.Add(hchbPatientTwo);
                accuCareDbContext.SaveChanges();
                statusTwo = GetHchbPatientStatus(1012445).Result;
            }
            catch (Exception)
            {

                Assert.Fail("failed to create hchb patient through Patient view in Api database.");
            }

            // Assert
            Assert.IsTrue(statusOne == "submitted");
            Assert.IsTrue(statusTwo == "submitted");
        }


        private Task<string?> GetHchbPatientStatus(int patientId)
        {
            return Task.FromResult(
                commonSpiritDbContext.HchbPatients.Where(p => p.PatientId == patientId)
                            .Select(p => p.Status)
                            .First());
        }

        private HchbTemplate? GetHchbTemplate(string admissionType, string observationId, string observationText, string patientType)
        {
            return (from trans in commonSpiritDbContext.Templates
                    where trans.AdmissionType == admissionType && trans.ObservationId == observationId && trans.ObservationText == observationText && trans.PatientType == patientType
                    select trans).AsNoTracking().FirstOrDefault();
        }

        private async Task LogProcessedMessage(int? id)
        {
            if (id != null) 
            {
                var log = commonSpiritDbContext.Logs.Where(h => h.Id == id).First();
                log.IsProcessed = true;
                log.ProcessedDate = DateTime.Now;
                commonSpiritDbContext.Logs.Update(log);
                await commonSpiritDbContext.SaveChangesAsync();
            }
        }
        
        private HL7MessageLog? GetMessageLog(HL7MessageLog message)
        {
            HL7MessageLog? messagelog = (from log in commonSpiritDbContext.Logs
                                        where log.MessageControlId == message.MessageControlId
                                        orderby log.ReceivedDate descending
                                        select log).ToList().FirstOrDefault();
            return messagelog;
        }
        
        private async Task<int?> LogHl7Message(HL7MessageLog message)
        {
            HL7MessageLog? log = GetMessageLog(message);

            if (log != null)
            {
                return log.Id;
            }
            else
            {
                commonSpiritDbContext.Logs.Add(message);
                await commonSpiritDbContext.SaveChangesAsync();
                int? logId = GetMessageLog(message)?.Id;
                return logId;
            }
        }
        
        private int GetFacilityIdFromBranchCode(string branchCode)
        {
            int result;
            int? facilityId = (from branch in commonSpiritDbContext.Branches
                               where branch.BranchCode == branchCode
                               select branch.FacilityId).FirstOrDefault();
            if (facilityId == null)
                result = -1;
            else
                result = facilityId.Value;

            return result;
        }
        
        private int GetSignerPrimaryFacility(int memberId)
        {
            int facilityId = (from trans in commonSpiritDbContext.HchbTransactions
                              where trans.SignerId == memberId
                              select trans.SignerFacilityId).FirstOrDefault();
            return facilityId;
        }
        
        private IQueryable<HL7MessageLog> GetUnprocessedMessage()
        {
            return commonSpiritDbContext.Logs.Where(l => l.IsProcessed != true);
        }
        
        private async Task LogReason(int? id, string reason)
        {
            if (id.HasValue)
            {
                var log = commonSpiritDbContext.Logs.Where(h => h.Id == id.Value).First();
                log.Reason = reason;
                await commonSpiritDbContext.SaveChangesAsync();
            }
            
        }
        
        private bool? IsProcessedMessage(int? id)
        {
            if(id.HasValue)
            return (from log in commonSpiritDbContext.Logs
                    where log.Id == id
                    select log.IsProcessed).FirstOrDefault();
            else
                return false;
        }
    }
}
