using NUnit.Framework;
using SutureHealth.Patients.Services;
using SutureHealth.Patients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SutureHealth.Hchb.Services.SqlServer;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Patients.Services.SqlServer;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using SutureHealth.Requests.Services;
using SutureHealth.Requests.Services.SqlServer;
using Azure;
using System.Diagnostics;
using Kendo.Mvc.Extensions;

namespace SutureHealth.Hchb.Services.Testing
{
    public class HCHBInMemoryDbContextTest
    {
        protected HchbWebDbContext hchbWebDbContext;

        [SetUp]
        public void Setup()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            hchbWebDbContext = new InMemoryHCHBDbContext(nameof(HCHBInMemoryDbContextTest));            
            hchbWebDbContext.Database.EnsureDeleted();
            hchbWebDbContext.Database.EnsureCreated();            
        }

        [TearDown]
        public void Clean()
        {
            hchbWebDbContext.SaveChanges();
            hchbWebDbContext.Dispose();
        }

        [Test]
        public void CreatePatientAndInsertIntoDatabaseShouldSuccess()
        {
            // Init
            HchbPatientWeb hchbPatient = new HchbPatientWeb()
            {
                PatientId = 1,
                HchbPatientId = "1",
                EpisodeId = "1",
                Status = "1",
            };
            
            // Act
            hchbWebDbContext.Add(hchbPatient);
            hchbWebDbContext.SaveChanges();
            
            // Assert
            Assert.IsTrue(hchbWebDbContext.HchbPatients.Count() == 1);
        }

        [Test]
        public void CreatePatientThenUpdateShouldSuccess()
        {
            // Init           
            HchbPatientWeb patient = new HchbPatientWeb()
            {
                PatientId = 1,
                HchbPatientId = "1",
                EpisodeId = "1",
                Status = "1",
            };
            
            // Act
            hchbWebDbContext.Add(patient);
            hchbWebDbContext.SaveChanges();
            patient.HchbPatientId = "2";
            hchbWebDbContext.Update(patient);
            hchbWebDbContext.SaveChanges();
            string _id = hchbWebDbContext.HchbPatients.First().HchbPatientId;
            // Assert
            Assert.IsTrue( _id == "2");
        }

        [Test]
        public void CreatePatientThenGetPatientStatusShouldMatchWithCreatedStatus()
        {
            // Init           
            HchbPatientWeb patient = new HchbPatientWeb()
            {
                PatientId = 1,
                HchbPatientId = "1",
                EpisodeId = "1",
                Status = "3",
            };

            // Act
            hchbWebDbContext.Add(patient);
            hchbWebDbContext.SaveChanges();
            var status = hchbWebDbContext.HchbPatients.Where(p=>p.Id==1).FirstOrDefault()?.Status;

            // Assert
            Assert.IsTrue(status == "3");
        }

        [Test]
        public void CreatePatientThenGetPatientByIdShouldReturnPatient()
        {
            // Init           
            HchbPatientWeb patient = new HchbPatientWeb()
            {
                PatientId = 2,
                HchbPatientId = "1",
                EpisodeId = "1",
                Status = "1",
            };

            // Act
            hchbWebDbContext.HchbPatients.Add(patient);
            hchbWebDbContext.SaveChanges();
            var retrivedPatient = hchbWebDbContext.HchbPatients.Where(P => P.PatientId == 2).FirstOrDefault();
            // Assert
            Assert.IsNotNull(retrivedPatient);
            Assert.IsTrue(retrivedPatient?.Id == 1);
        }

        [Test]
        public void CreatePatientThenGetPatientIdShouldReturnId()
        {
            // Init           
            HchbPatientWeb patient = new HchbPatientWeb()
            {
                PatientId = 1,                
                HchbPatientId = "1",                
                EpisodeId = "1",
                Status = "1"                
            };

            // Act
            hchbWebDbContext.HchbPatients.Add(patient);
            hchbWebDbContext.SaveChanges();
            patient = new HchbPatientWeb()
            {
                PatientId = 2,
                HchbPatientId = "2",
                EpisodeId = "2",
                Status = "2",
            };
            hchbWebDbContext.HchbPatients.Add(patient);
            hchbWebDbContext.SaveChanges();
            patient = new HchbPatientWeb()
            {
                PatientId = 1,
                HchbPatientId = "1",
                EpisodeId = "1",
                Status = "1",
            };
            var retrivedId = hchbWebDbContext.HchbPatients.Where(P => P.PatientId == 1).Select(P => P.PatientId).FirstOrDefault();
            
            // Assert

            Assert.IsTrue(retrivedId == 1);
        }

        [Test]
        public void CreateTransactionShouldStoreInDatabase()
        {
            // Init           
            HchbTransaction transaction = new HchbTransaction()
            {
                RequestId = 1,
                OrderDate = DateTime.Now,
                OrderNumber = "1",
                FileName ="file_1",                
                ObservationId = "observation_1",
                ObservationText ="observation_text",
                TemplateId = 10001
            };

            // Act
            hchbWebDbContext.HchbTransactions.Add(transaction);
            hchbWebDbContext.SaveChanges();

            // Assert
            Assert.IsTrue(hchbWebDbContext.HchbTransactions.Count() == 1);
        }

        [Test]
        public void CreateTransactionThenGetByRequestIdShouldReturnTransaction()
        {
            // Init           
            HchbTransaction transaction = new HchbTransaction()
            {
                RequestId = 1,
                OrderDate = DateTime.Now,
                OrderNumber = "1",
                FileName = "file_1",
                ObservationId = "observation_1",
                ObservationText = "observation_text",
                TemplateId = 10001
            };

            // Act
            hchbWebDbContext.HchbTransactions.Add(transaction);
            hchbWebDbContext.SaveChanges();
            var retrivedTransaction = hchbWebDbContext.HchbTransactions.Where(T => T.RequestId == 1).FirstOrDefault();

            // Assert
            Assert.IsTrue(retrivedTransaction.RequestId == 1);
        }

        [Test]
        public async Task CreateHL7MessageThenLogProcessedMessageShouldSetIsProccessedFlagToTrue()
        {
            // Init           
            HL7MessageLog hl7MessageLog = new HL7MessageLog()
            {
                Id =1,
                MessageControlId ="1",
                Type ="type_1",
                SubType ="sub_type_1",
                Message ="message_one",
                RawMessageFile ="raw_message_file_content",
                JsonMessageFile ="{Message:'success'}",
                ProcessedDate = DateTime.Now,
                Reason = "reason_one",
                IsProcessed = false
            };

            // Act
            hchbWebDbContext.Logs.Add(hl7MessageLog);
            hchbWebDbContext.SaveChanges();
            var log = hchbWebDbContext.Logs.Where(H => H.Id == 1).First();
            log.IsProcessed = true;
            log.ProcessedDate = DateTime.Now;
            hchbWebDbContext.Logs.Update(log);
            await hchbWebDbContext.SaveChangesAsync();

            // Assert
            Assert.IsTrue(hchbWebDbContext.Logs.First().IsProcessed == true);
        }

        [Test]
        public async Task CreateHL7MessageThenLogReasonShouldUpdateReasonToNewValue()
        {
            // Init           
            HL7MessageLog hl7MessageLog = new HL7MessageLog()
            {
                Id = 1,
                MessageControlId = "1",
                Type = "type_1",
                SubType = "sub_type_1",
                Message = "message_one",
                RawMessageFile = "raw_message_file_content",
                JsonMessageFile = "{Message:'success'}",
                ProcessedDate = DateTime.Now,
                Reason = "reason_one",
                IsProcessed = false
            };

            // Act
            hchbWebDbContext.Logs.Add(hl7MessageLog);
            hchbWebDbContext.SaveChanges();
            var log = hchbWebDbContext.Logs.Where(H => H.Id == 1).First();
            log.Reason = "new_readon_one";
            await hchbWebDbContext.SaveChangesAsync();

            // Assert
            Assert.IsTrue(hchbWebDbContext.Logs.First().Reason.IsCaseInsensitiveEqual("new_readon_one"));
        }

        [Test]
        public void CreateHL7MessagesThenGetAllUnprocessedMessagesShouldReturnAll()
        {
            // Init
            List<HL7MessageLog> hl7messageList = new List<HL7MessageLog>()
            {
                new HL7MessageLog()
                {
                    Id = 1,
                    IsProcessed = false
                },
                new HL7MessageLog()
                {
                    Id = 2,
                    IsProcessed = false
                },
                new HL7MessageLog()
                {
                    Id = 3,
                    IsProcessed = false
                },
                new HL7MessageLog()
                {
                    Id = 4,
                    IsProcessed = true
                },
        };
            
            // Act
            hchbWebDbContext.Logs.AddRange(hl7messageList);
            hchbWebDbContext.SaveChanges();            
            var unprocessedMessages = hchbWebDbContext.Logs.Where(l => l.IsProcessed != true);

            // Assert
            Assert.IsTrue(unprocessedMessages.Count() == 3);
        }

        [Test]
        public void CreateHL7MessagesThenControlProccessedFlagShouldReturnRightFlagValue()
        {
            // Init
            List<HL7MessageLog> hl7messageList = new List<HL7MessageLog>()
            {
                new HL7MessageLog()
                {
                    Id = 1,
                    IsProcessed = false
                },
                new HL7MessageLog()
                {
                    Id = 2,
                    IsProcessed = true
                }
        };

            // Act
            hchbWebDbContext.Logs.AddRange(hl7messageList);
            hchbWebDbContext.SaveChanges();

            var messageOneIsProcessed = (from log in hchbWebDbContext.Logs
                                        where log.Id == 1
                                        select log.IsProcessed).FirstOrDefault();

            var messageTwoIsProcessed = (from log in hchbWebDbContext.Logs
                                         where log.Id == 2
                                         select log.IsProcessed).FirstOrDefault();

            // Assert
            Assert.IsTrue(messageOneIsProcessed == false && messageTwoIsProcessed == true);
        }
        [Test]
        public void AddBranchesAndGetFacilityIdFromBranchCodeShoudlReturnFacilityId() 
        {
            // Init
            List<Branch> branches = new List<Branch>()
            {
                new Branch()
                {
                    Id = 1,
                    BranchCode = "054",
                    BranchName = "SutureTest Home Health - St. Pete",
                    FacilityId = 12264
                },
                new Branch()
                {
                    Id = 2,
                    BranchCode = "0E4",
                    BranchName = "External Name - Agency Network Floater",
                    FacilityId = 13359
                },
                new Branch()
                {
                    Id = 3,
                    BranchCode = "8V1",
                    BranchName = "Harbor Hospice",
                    FacilityId = 13365
                },
                new Branch()
                 {
                    Id = 4,
                    BranchCode = "040",
                    BranchName = "Dentist Agency Senders",
                    FacilityId = 13504
                 }
            };
            hchbWebDbContext.AddRange(branches);
            hchbWebDbContext.SaveChanges();

            // Act
            var facilityId = GetFacilityIdFromBranchCode("8V1");

            // Assert
            Assert.IsTrue(facilityId == 13365);

        }
        
        
        private Task<string?> GetHchbPatientStatus(int patientId)
        {
            return Task.FromResult(
                hchbWebDbContext.HchbPatients.Where(p => p.PatientId == patientId)
                            .Select(p => p.Status)
                            .First());
        }

        private HchbTemplate? GetHchbTemplate(string admissionType, string observationId, string observationText, string patientType)
        {
            return (from trans in hchbWebDbContext.Templates
                    where trans.AdmissionType == admissionType && trans.ObservationId == observationId && trans.ObservationText == observationText && trans.PatientType == patientType
                    select trans).AsNoTracking().FirstOrDefault();
        }

        private async Task LogProcessedMessage(int? id)
        {
            if (id != null)
            {
                var log = hchbWebDbContext.Logs.Where(h => h.Id == id).First();
                log.IsProcessed = true;
                log.ProcessedDate = DateTime.Now;
                hchbWebDbContext.Logs.Update(log);
                await hchbWebDbContext.SaveChangesAsync();
            }
        }

        private HL7MessageLog? GetMessageLog(HL7MessageLog message)
        {
            HL7MessageLog? messagelog = (from log in hchbWebDbContext.Logs
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
                hchbWebDbContext.Logs.Add(message);
                await hchbWebDbContext.SaveChangesAsync();
                int? logId = GetMessageLog(message)?.Id;
                return logId;
            }
        }

        private int GetFacilityIdFromBranchCode(string branchCode)
        {
            int result;
            int? facilityId = (from branch in hchbWebDbContext.Branches
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
            int facilityId = (from trans in hchbWebDbContext.HchbTransactions
                              where trans.SignerId == memberId
                              select trans.SignerFacilityId).FirstOrDefault();
            return facilityId;
        }

        private IQueryable<HL7MessageLog> GetUnprocessedMessage()
        {
            return hchbWebDbContext.Logs.Where(l => l.IsProcessed != true);
        }

        private async Task LogReason(int? id, string reason)
        {
            if (id.HasValue)
            {
                var log = hchbWebDbContext.Logs.Where(h => h.Id == id.Value).First();
                log.Reason = reason;
                await hchbWebDbContext.SaveChangesAsync();
            }

        }

        private bool? IsProcessedMessage(int? id)
        {
            if (id.HasValue)
                return (from log in hchbWebDbContext.Logs
                        where log.Id == id
                        select log.IsProcessed).FirstOrDefault();
            else
                return false;
        }
    }
}
