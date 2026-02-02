using Microsoft.VisualStudio.TestTools.UnitTesting;
using SutureHealth.DataScraping;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SutureHealth.DataScrapingAPI.Testing.Tests
{
    [TestClass]
    public class DuplicatePatientTests : IntegrationTestBase
    {
        [Test]
        public async Task RemoveDuplicatePatientsMustLeaveOneRowWithSameId()
        {
            string updatedSSN = "lastUpdatedSSN";
            string updatedPhysician = "LastUpdatedPhysician";

            ScrapedPatient scrapedPatient1 = new ScrapedPatient()
            {
                URL = "testURL1",
                ExternalId = "12",
                FirstName = "John",
                LastName = "Doe",
                Phone = "04678543267",
                SSN = "testSSN1",
                DateOfBirth = new DateTime(1982, 12, 05),
                AttendedPhysician = "TestPhysician1",
                CreatedAt = DateTime.UtcNow
            };
            ScrapedPatient scrapedPatient2 = new ScrapedPatient()
            {
                URL = "testURL1",
                ExternalId = "12",
                FirstName = "John",
                LastName = "Doe",
                Phone = "04678543267",
                SSN = "testSSN1",
                DateOfBirth = new DateTime(1982, 12, 05),
                AttendedPhysician = "TestPhysician1",
                CreatedAt = DateTime.UtcNow
            };
            ScrapedPatient scrapedPatient3 = new ScrapedPatient()
            {
                URL = "testURL1",
                ExternalId = "12",
                FirstName = "John",
                LastName = "Doe",
                Phone = "04678543267",
                SSN = updatedSSN,
                DateOfBirth = new DateTime(1982, 12, 05),
                AttendedPhysician = updatedPhysician,
                CreatedAt = DateTime.UtcNow
            };

            DataScrapingDbContext.ScrapedPatient.Add(scrapedPatient1);
            DataScrapingDbContext.ScrapedPatient.Add(scrapedPatient2);
            DataScrapingDbContext.ScrapedPatient.Add(scrapedPatient3);
            DataScrapingDbContext.SaveChanges();

            DataScrapingService.RemoveDuplicatePatients();

            var patientCount = DataScrapingDbContext.ScrapedPatient.Where(sp => sp.ExternalId == scrapedPatient1.ExternalId).ToList().Count;
            var patient = DataScrapingDbContext.ScrapedPatient.FirstOrDefault(sp => sp.ExternalId == scrapedPatient1.ExternalId);

            NUnit.Framework.Assert.That(patientCount, Is.EqualTo(1));
            NUnit.Framework.Assert.That(patient.SSN, Is.EqualTo(updatedSSN));
            NUnit.Framework.Assert.That(patient.AttendedPhysician, Is.EqualTo(updatedPhysician));

        }

        [Test]
        public async Task RemoveDuplicatePatientDetailsMustLeaveOneRowWithSameId()
        {
            string updatedSSN = "lastUpdatedSSN";
            string updatedPhysician = "LastUpdatedPhysician";
            string updatedCity = "updatedCity";
            string updatedFamilySize = "5";

            ScrapedPatientDetail scrapedPatientDetail1 = new ScrapedPatientDetail()
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
                CreatedAt = DateTime.UtcNow
            };

            ScrapedPatientDetail scrapedPatientDetail2 = new ScrapedPatientDetail()
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
                AttendedPhysician = updatedPhysician,
                Address = "testAdress1123",
                City = updatedCity,
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
                FamilySize = updatedFamilySize,
                MonthlyIncome = "10000",
                Religion = "Christian",
                DeceaseDate = "",
                DeceaseReason = "",
                SSN = updatedSSN,
                CreatedAt = DateTime.UtcNow
            };

            DataScrapingDbContext.ScrapedPatientDetail.Add(scrapedPatientDetail1);
            DataScrapingDbContext.ScrapedPatientDetail.Add(scrapedPatientDetail2);
            DataScrapingDbContext.SaveChanges();

            DataScrapingService.RemoveDuplicatePatientDetails();

            var patientdetailCount = DataScrapingDbContext.ScrapedPatientDetail.Where(sp => sp.ExternalId == scrapedPatientDetail1.ExternalId).ToList().Count;
            var patientdetail = DataScrapingDbContext.ScrapedPatientDetail.FirstOrDefault(sp => sp.ExternalId == scrapedPatientDetail1.ExternalId);

            NUnit.Framework.Assert.That(patientdetailCount, Is.EqualTo(1));
            NUnit.Framework.Assert.That(patientdetail.SSN, Is.EqualTo(updatedSSN));
            NUnit.Framework.Assert.That(patientdetail.AttendedPhysician, Is.EqualTo(updatedPhysician));
            NUnit.Framework.Assert.That(patientdetail.City, Is.EqualTo(updatedCity));
            NUnit.Framework.Assert.That(patientdetail.FamilySize, Is.EqualTo(updatedFamilySize));

        }
    }
}
