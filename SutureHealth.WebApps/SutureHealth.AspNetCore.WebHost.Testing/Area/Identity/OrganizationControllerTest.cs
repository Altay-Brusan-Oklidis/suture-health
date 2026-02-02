using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.ApplicationAPI.Testing.MockServices;
using SutureHealth.AspNetCore.Areas.Identity.Controllers;
using SutureHealth.AspNetCore.Areas.Identity.Models.Organization;
using SutureHealth.AspNetCore.WebHost.Testing.Mock;

namespace SutureHealth.AspNetCore.WebHost.Testing.Area.Identity
{
    [TestClass]
    public class OrganizationControllerTest
    {
        private ApplicationServiceMock applicationServiceMock;
        private IdentityServiceMock identityServiceMock;

        [TestInitialize]
        public void BeforeEach()
        {
            applicationServiceMock = new ApplicationServiceMock();
            identityServiceMock = new IdentityServiceMock();
        }

        [TestMethod]
        public async Task SaveNewOrganizationWithInvalidProfileModelShouldReturnView()
        {
            // Arrange
            var orgController = GetOrganizationController(applicationServiceMock.Object, identityServiceMock.Object);
            var model = new ProfileModel();
            orgController.ModelState.AddModelError("PropertyName", "Error Message");

            // Act
            var result = await orgController.SaveNewOrganization(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("Profile", (result as ViewResult).ViewName);
        }

        [TestMethod]
        public async Task SaveNewOrganizationWithValidProfileModelShouldReturnView()
        {
            // Arrange
            var orgController = GetOrganizationController(applicationServiceMock.Object, identityServiceMock.Object);
            var model = new ProfileModel();

            // Act
            var result = await orgController.SaveNewOrganization(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            Assert.IsTrue((result as RedirectResult).Url.Contains("AdminOrganizationIndex"));
        }

        [TestMethod]
        public async Task SaveNewOrganizationWithNonAdminProfileModelShouldReturnView()
        {
            // Arrange
            var memberId = 456;
            var orgController = GetOrganizationController(applicationServiceMock.Object, identityServiceMock.Object, memberId);
            var model = new ProfileModel();

            // Act
            var result = await orgController.SaveNewOrganization(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            Assert.IsTrue((result as RedirectResult).Url.Contains("SendIndex"));
        }

        [TestMethod]
        public async Task SaveNewOrganizationWithExistingNPIShouldReturnModelError()
        {
            // Arrange
            var orgController = GetOrganizationController(applicationServiceMock.Object, identityServiceMock.Object);
            var model = new ProfileModel
            {
                Npi = "110101" // This NPI is already existing.
            };

            // Act
            var result = await orgController.SaveNewOrganization(model);

            // Assert
            var error = orgController.ModelState["NPI"].Errors.FirstOrDefault();
            Assert.IsNotNull(error);
            Assert.AreEqual("This NPI is already existing.", error.ErrorMessage);
        }

        [TestMethod]
        public void SaveNewOrganizationWithConcurrentTwoRequests_Should_ExecuteInOrder()
        {
            // Arrange
            var callNumber = 0;
            applicationServiceMock
                .Setup(x => x.GetOrganizationByNPIAsync(It.IsAny<long>()))
                .Returns(async (long npi) =>
                {
                    if (callNumber == 0)
                    {
                        callNumber++;
                        await Task.Delay(1000);
                    }
                    
                    return new Organization
                    {
                        OrganizationId = 11010,
                        Name = "org1",
                        CompanyId = 12,
                        IsActive = true,
                        NPI = "110101"
                    };
                });
            var orgController = GetOrganizationController(applicationServiceMock.Object, identityServiceMock.Object);
            var model = new ProfileModel();

            // Act
            var task1 = orgController.SaveNewOrganization(model);
            var task2 = orgController.SaveNewOrganization(model);
            var index = Task.WaitAny(task1, task2);
            
            // Assert
            Assert.AreEqual(0, index, "The first call should complete first");
        }

        private OrganizationController GetOrganizationController(IApplicationService applicationService,
            IIdentityService identityService, int memberId = 123)
        {

            var controller = new OrganizationController(applicationService, identityService)
            {
                CurrentUser = new MemberIdentity
                {
                    MemberId = memberId
                }
            };

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(x => x.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns((UrlRouteContext context) => context.RouteName);

            controller.Url = urlHelperMock.Object;

            if (memberId == 123)
                controller.CurrentUser.MemberTypeId = 2016; // Default memberId is an admin
            else if (memberId == 456)
                controller.CurrentUser.MemberTypeId = 2003; // sender

            return controller;
        }
    }
}
