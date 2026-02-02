using Kendo.Mvc.Extensions;
using Moq;
using SutureHealth.Requests.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.AspNetCore.WebHost.Testing.Area.Admin.Controllers
{
    public class RequestServiceProviderMock: Mock<IRequestServicesProvider>
    {
        public RequestServiceProviderMock()
        {
            _ = Setup(x => x.GetServiceableRequestPdfByIdAsync(It.IsAny<int>()))
                            .Returns(GetServiceableRequestPdfByIdAsync);
        }

        Task<IReadOnlyDictionary<int, byte[]>> GetServiceableRequestPdfByIdAsync(params int[] sutureSignRequestIds) 
        {
            if(sutureSignRequestIds.Length !=1)
                return null;
            byte[] bytes = new byte[] { 1, 2, 3, 4, 5 };

            Dictionary<int, byte[]> d = new Dictionary<int, byte[]>();
            d.Add(1, bytes);

            IReadOnlyDictionary<int, byte[]> readOnlyDictionary = new ReadOnlyDictionary<int, byte[]>(d);

            return Task.FromResult(readOnlyDictionary);
        }

    }
}
