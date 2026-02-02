using Moq;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.Patients.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.AspNetCore.WebHost.Testing.Area.Admin.Controllers
{
    public class ApplicationServiceMock:Mock<IApplicationService>
    {
        public ApplicationServiceMock() 
        {
            _ = Setup(x => x.GetOrganizationByIdAsync(It.IsAny<int>())).Returns(GetOrganizationByIdAsync);
            _ = Setup(x => x.GetMemberByIdAsync(It.IsAny<int>())).Returns(GetMemberByIdAsync);
        }
        Task<Organization> GetOrganizationByIdAsync(int organizationId) 
        {
            Organization organization = new Organization()
            {
                OrganizationId = 13574,
                ParentId = null,
                OrganizationTypeId = 10003,
                Name = "Ajay Thakur Physicians Office",
                OtherDesignation = "Ajay Thakur Physicians Office",
                NPI = "1654654650",
                IsActive = true,
                CreatedAt = new DateTime(2023, 10, 30, 18, 38, 36),
                CompanyId = 13574,
                AddressLine1 = "123 Med practice Way",
                City = "Orlando",
                StateOrProvince = "FL",
                PostalCode = "32803"
            };

            return Task.FromResult(organization);            
        }

        Task<Member> GetMemberByIdAsync(int memberID) 
        {
            Member member = new Member()
            {
                MemberId = 3006205,
                UserName = "admin-athakur",
                MemberTypeId = 2016,
                FirstName = "SutureAdmin",
                LastName = "Ajay Thakur",
                SigningName = "SutureAdmin Ajay Thakur",
            };
            return Task.FromResult(member);
        }
    }
}
