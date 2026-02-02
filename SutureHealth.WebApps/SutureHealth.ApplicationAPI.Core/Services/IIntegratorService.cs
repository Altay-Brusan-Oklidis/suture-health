using System;
using System.Threading.Tasks;

namespace SutureHealth.Application.Services
{
    public interface IIntegratorService
    {
        Task<Integrator> GetIntegratorByIdAsync(Guid initiator);
        Task<Organization> GetOrganizationByApiKeyAsync(string apiKey);
    }
}
