using Moq;
using SutureHealth.Application.Services;
using SutureHealth.ApplicationAPI.Testing.MockServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.AspNetCore.WebHost.Testing.Mock
{
    public class IdentityServiceMock : Mock<IIdentityService>
    {
        public IdentityServiceMock()
        {
            
        }
    }
}
