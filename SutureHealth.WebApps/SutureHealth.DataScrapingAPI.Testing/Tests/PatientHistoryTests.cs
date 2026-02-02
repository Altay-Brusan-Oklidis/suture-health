using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SutureHealth.DataScraping;
using SutureHealth.DataScraping.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SutureHealth.DataScrapingAPI.Testing.Tests
{
    [TestClass]
    public class PatientHistoryTests : IntegrationTestBase
    {
        [Test]
        public async Task ScrapedPatientHistoryInsertMustCreateOneRow()
        {
            ScrapedPatientHistory scrapedPatient = new ScrapedPatientHistory()
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

            var scrapedPatientCountBeforeInsert = DataScrapingDbContext.ScrapedPatientHistory.ToList().Count;
            await DataScrapingDbContext.CreateScrapedPatientHistoryAsync(scrapedPatient);
            var scrapedPatientCountAfterInsert = DataScrapingDbContext.ScrapedPatientHistory.ToList().Count;

            NUnit.Framework.Assert.That(scrapedPatientCountAfterInsert, Is.EqualTo(scrapedPatientCountBeforeInsert + 1));
        }

        [Test]
        public async Task ScrapedPatientDetailHistoryInsertMustCreateOneRow()
        {
            var patientDetailId = Guid.NewGuid();

            List<ObservationHistory> observationHistories = new List<ObservationHistory>()
            {
                new ObservationHistory()
                {
                    PatientId= patientDetailId,
                    Labs= "labs123",
                    Vitals="vitals-abcd",
                }
            };
            List<ContactHistory> contactHistories = new List<ContactHistory>()
            {
                new ContactHistory()
                {
                    PatientId= patientDetailId,
                    ContactText="1238758764",
                    ContactType=DataScraping.ContactType.MobilePhone,
                },
                new ContactHistory()
                {
                    PatientId= patientDetailId,
                    ContactText="testemail",
                    ContactType=DataScraping.ContactType.Email,
                },
                new ContactHistory()
                {
                    PatientId= patientDetailId,
                    ContactText="067482622972",
                    ContactType=DataScraping.ContactType.HomePhone,
                },
            };
            List<ConditionHistory> conditionHistories = new List<ConditionHistory>()
            {
                new ConditionHistory()
                {
                    PatientId= patientDetailId,
                    Diagnosis="testDiagnosis",
                    DiagnosisCode="testDiagnosisCode",
                    StartDate = new DateTime(2012,04,12),
                    EndDate = null
                },
                new ConditionHistory()
                {
                    PatientId= patientDetailId,
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
                    PatientId= patientDetailId,
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
                    PatientId= patientDetailId,
                    Name="medication1",
                    Code="medicationCode1",
                    StartDate=new DateTime(1999,02,04),
                    EndDate=new DateTime(2002,08,14)
                },
                new MedicationHistory()
                {
                    PatientId= patientDetailId,
                    Name="medication2",
                    Code="medicationCode2",
                    StartDate=new DateTime(2014,06,13),
                    EndDate=new DateTime(2016,08,14)
                },
                new MedicationHistory()
                {
                    PatientId= patientDetailId,
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
                    PatientId= patientDetailId,
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
                    PatientId= patientDetailId,
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
                    PatientId= patientDetailId,
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
                Id = patientDetailId,
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


            int observationsCount = scrapedPatientDetail.Observations.Count;
            int contactsCount = scrapedPatientDetail.Contacts.Count;
            int conditionsCount = scrapedPatientDetail.Conditions.Count;
            int allergiesCount = scrapedPatientDetail.Allergies.Count;
            int medicationsCount = scrapedPatientDetail.Medications.Count;
            int immunizationsCount = scrapedPatientDetail.Immunizations.Count;
            int prescriptionsCount = scrapedPatientDetail.Prescriptions.Count;
            int proceduresCount = scrapedPatientDetail.Procedures.Count;

            var scrapedPatientDetailCountBeforeInsert = DataScrapingDbContext.ScrapedPatientDetailHistory.ToList().Count;
            await DataScrapingDbContext.CreateScrapedPatientDetailHistoryAsync(scrapedPatientDetail);
            var scrapedPatientDetailCountAfterInsert = DataScrapingDbContext.ScrapedPatientDetailHistory.ToList().Count;

            var dbScrapedPatientDetail = DataScrapingDbContext.ScrapedPatientDetailHistory
                                                                            .Include(spd => spd.Allergies)
                                                                            .Include(spd => spd.Conditions)
                                                                            .Include(spd => spd.Contacts)
                                                                            .Include(spd => spd.Immunizations)
                                                                            .Include(spd => spd.Medications)
                                                                            .Include(spd => spd.Observations)
                                                                            .Include(spd => spd.Prescriptions)
                                                                            .Include(spd => spd.Procedures)
                                                                            .FirstOrDefault(spd => 1 == 1);

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
    }
}
