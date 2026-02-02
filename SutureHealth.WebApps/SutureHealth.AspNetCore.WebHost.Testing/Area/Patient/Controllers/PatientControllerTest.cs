using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.Areas.Controllers.Patient;
using SutureHealth.AspNetCore.Areas.Patient.Models;
using SutureHealth.Linq;
using SutureHealth.Patients;
using SutureHealth.Patients.Services;
using SutureHealth.Reporting.Digest;

namespace SutureHealth.AspNetCore.WebHost.Testing.Area.Patient.Controllers;

public class PatientControllerTest
{
    [TestClass]
    public class SaveNewTest
    {
        private Application.Organization organization;
        private PatientModel patientModel;
        private Mock<IApplicationService> applicationServiceMock;

        [TestInitialize]
        public void BeforeEach()
        {
            organization = new Application.Organization
            {
                OrganizationId = 1
            };

            patientModel = new PatientModel
            {
                DateOfBirth = DateTime.Parse("1999-01-01"),
            };
            
            applicationServiceMock = new Mock<IApplicationService>();
        }

        [TestMethod]
        public async Task WhenTopMatchAndPatientModelHaveFullSocialShouldSavePatientModelFullSocial()
        {
            // Arrange
            patientModel.SocialSecurityNumber = "111-22-3333";
            patientModel.SocialSecurityNumberType = PatientModel.SocialSecurityNumberStyle.Full;

            var topMatch = new Patients.Patient
            {
                Identifiers = new List<PatientIdentifier> { 
                    new()
                    {
                        Value = "999-88-7777",
                        Type = KnownTypes.SocialSecurityNumber
                    }
                }
            };
            const int memberId = 123;
            var patientServiceProviderMock = CreatePatientServiceProviderMock(topMatch, 1, memberId);
            var controller = GetController(applicationServiceMock.Object, patientServiceProviderMock.Object, memberId);

            // Act
            await controller.SaveNew(organization, patientModel);

            // Assert
            patientServiceProviderMock.Verify(x =>
                    x.UpdateAsync(It.Is<SutureHealth.Patients.Patient>(p =>
                            p.Identifiers.Any(identifier => identifier.Value == patientModel.SocialSecurityNumber)),
                        organization.OrganizationId,
                        memberId), 
                Times.Once());
        }

        [TestMethod]
        public async Task WhenTopMatchHasNoSocialShouldSaveNewSsn4()
        {
            // Arrange
            patientModel.SocialSecurityNumber = "0000";
            patientModel.SocialSecurityNumberType = PatientModel.SocialSecurityNumberStyle.Last4;

            var topMatch = new Patients.Patient
            {
                Identifiers = Array.Empty<PatientIdentifier>()
            };
            const int memberId = 123;
            var patientServiceProviderMock = CreatePatientServiceProviderMock(topMatch, 1, memberId);
            var controller = GetController(applicationServiceMock.Object, patientServiceProviderMock.Object, memberId);

            // Act
            await controller.SaveNew(organization, patientModel);

            // Assert
            patientServiceProviderMock.Verify(x =>
                x.UpdateAsync(It.Is<SutureHealth.Patients.Patient>(p =>
                        p.Identifiers.Any(identifier => identifier.Value == patientModel.SocialSecurityNumber)),
                    organization.OrganizationId,
                    memberId), 
                Times.Once());
        }
        
        [TestMethod]
        public async Task WhenTopMatchHasFullSocialAndPatientModelHasLast4SocialShouldSaveTopMatchFullSocial()
        {
            // Arrange
            patientModel.DateOfBirth = DateTime.Parse("1999-01-01");
            patientModel.SocialSecurityNumber = "0000";
            patientModel.SocialSecurityNumberType = PatientModel.SocialSecurityNumberStyle.Last4;

            const string topMatchFullSocial = "111-22-3333";
            var topMatch = new Patients.Patient
            {
                Identifiers = new List<PatientIdentifier>
                {
                    new()
                    {
                        Type = KnownTypes.SocialSecurityNumber,
                        Value = topMatchFullSocial
                    }
                }
            };

            const int memberId = 123;
            var patientServiceProviderMock = CreatePatientServiceProviderMock(topMatch, 1, memberId);
            var controller = GetController(applicationServiceMock.Object, patientServiceProviderMock.Object, memberId);

            // Act
            await controller.SaveNew(organization, patientModel);

            // Assert
            patientServiceProviderMock.Verify(x =>
                    x.UpdateAsync(It.Is<SutureHealth.Patients.Patient>(p =>
                            p.Identifiers.Any(identifier => identifier.Value == topMatchFullSocial)),
                        organization.OrganizationId,
                        memberId), 
                Times.Once());
        }
        
        [TestMethod]
        public async Task WhenTopMatchHasLast4SocialAndPatientModelHasLast4SocialShouldSaveNewLast4Social()
        {
            // Arrange
            patientModel.SocialSecurityNumber = "0000";
            patientModel.SocialSecurityNumberType = PatientModel.SocialSecurityNumberStyle.Last4;

            var topMatch = new Patients.Patient
            {
                Identifiers = new List<PatientIdentifier>
                {
                    new()
                    {
                        Type = KnownTypes.SocialSecuritySerial,
                        Value = "9999"
                    }
                }
            };

            const int memberId = 123;
            var patientServiceProviderMock = CreatePatientServiceProviderMock(topMatch, 1, memberId);
            var controller = GetController(applicationServiceMock.Object, patientServiceProviderMock.Object, memberId);

            // Act
            await controller.SaveNew(organization, patientModel);

            // Assert
            patientServiceProviderMock.Verify(x =>
                    x.UpdateAsync(It.Is<SutureHealth.Patients.Patient>(p =>
                            p.Identifiers.Any(identifier => identifier.Value == patientModel.SocialSecurityNumber)),
                        organization.OrganizationId,
                        memberId), 
                Times.Once());
        }
        
        [TestMethod]
        public async Task WhenTopMatchHasLast4SocialAndPatientModelHasNoSocialShouldSaveTopMatchLast4Social()
        {
            // Arrange
            const string topMatchLast4Social = "9999";
            var topMatch = new Patients.Patient
            {
                Identifiers = new List<PatientIdentifier>
                {
                    new()
                    {
                        Type = KnownTypes.SocialSecuritySerial,
                        Value = topMatchLast4Social
                    }
                }
            };

            const int memberId = 123;
            var patientServiceProviderMock = CreatePatientServiceProviderMock(topMatch, 
                organization.OrganizationId, 
                memberId);
            var controller = GetController(applicationServiceMock.Object, patientServiceProviderMock.Object, memberId);

            // Act
            await controller.SaveNew(organization, patientModel);

            // Assert
            patientServiceProviderMock.Verify(x =>
                    x.UpdateAsync(It.Is<SutureHealth.Patients.Patient>(p =>
                            p.Identifiers.Any(identifier => identifier.Value == topMatchLast4Social)),
                        organization.OrganizationId,
                        memberId), 
                Times.Once());
        }

        [TestMethod]
        public async Task WhenMedicareMbiIsOnTopMatchShouldBePreserved()
        {
            // Arrange
            const string medicareMbi = "1A35H42JL11";

            var topMatch = new Patients.Patient
            {
                Identifiers = new List<PatientIdentifier>
                {
                    new()
                    {
                        Type = KnownTypes.Medicare,
                        Value = medicareMbi
                    }
                }
            };

            const int memberId = 123;
            var patientSerivceProviderMock =
                CreatePatientServiceProviderMock(topMatch, organization.OrganizationId, memberId);
            var controller = GetController(applicationServiceMock.Object, patientSerivceProviderMock.Object, memberId);
            
            // Act
            await controller.SaveNew(organization, patientModel);
            
            // Assert
            patientSerivceProviderMock.Verify(x =>
                x.UpdateAsync(
                    It.Is<Patients.Patient>(p => p.Identifiers.Any(identifier =>
                        identifier.Value == medicareMbi && identifier.Type == KnownTypes.Medicare)),
                    organization.OrganizationId,
                    memberId), Times.Once());
        }
        
        [TestMethod]
        public async Task WhenMedicaidIsOnTopMatchShouldBePreserved()
        {
            // Arrange
            const string medicaidNumber = "AABCD123";

            var topMatch = new Patients.Patient
            {
                Identifiers = new List<PatientIdentifier>
                {
                    new()
                    {
                        Type = KnownTypes.MedicaidNumber,
                        Value = medicaidNumber
                    }
                }
            };

            const int memberId = 123;
            var patientSerivceProviderMock =
                CreatePatientServiceProviderMock(topMatch, organization.OrganizationId, memberId);
            var controller = GetController(applicationServiceMock.Object, patientSerivceProviderMock.Object, memberId);
            
            // Act
            await controller.SaveNew(organization, patientModel);
            
            // Assert
            patientSerivceProviderMock.Verify(x =>
                x.UpdateAsync(
                    It.Is<Patients.Patient>(p => p.Identifiers.Any(identifier =>
                        identifier.Value == medicaidNumber && identifier.Type == KnownTypes.MedicaidNumber)),
                    organization.OrganizationId,
                    memberId), Times.Once());
        }
        
        private static Mock<IPatientServicesProvider> CreatePatientServiceProviderMock(Patients.Patient topMatch, 
            int organizationId, 
            int memberId)
        {
            var patientServiceProviderMock = new Mock<IPatientServicesProvider>();

            var patientMatchingResponse = new PatientMatchingResponse
            {
                MatchLevel = MatchLevel.Match,
                TopMatch = topMatch
            };
            patientServiceProviderMock
                .Setup(x => x.MatchAsync(It.IsAny<PatientMatchingRequest>()))
                .Returns(Task.FromResult(patientMatchingResponse));
            
            patientServiceProviderMock
                .Setup(x => 
                    x.UpdateAsync(It.IsAny<SutureHealth.Patients.Patient>(), organizationId, memberId));

            return patientServiceProviderMock;
        }

        private ILogger<T> GetLogger<T>()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();
            if (factory == null)
                Assert.Fail("Could not create logger factory");

            return factory.CreateLogger<T>();
        }

        private PatientController GetController(IApplicationService applicationService, 
            IPatientServicesProvider patientServiceProvider, int memberId)
        {
            var logger = GetLogger<PatientController>();
            
            var controller =
                new PatientController(logger, applicationService, patientServiceProvider);
            controller.CurrentUser = new MemberIdentity
            {
                MemberId = memberId
            };

            return controller;
        }
    }
}