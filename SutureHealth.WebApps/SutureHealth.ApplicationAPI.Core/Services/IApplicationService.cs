using System.Collections.Generic;
using System.Threading.Tasks;

namespace SutureHealth.Application.Services
{
    public interface IApplicationService : IApplicationSettingService,
                                           IIntegratorService,
                                           IMemberService, 
                                           IOrganizationService,
                                           IOrganizationMemberService,
                                           IFhirUserConflationService
    {
        Task<IEnumerable<OrganizationMember>> GetSigningOrganizationMembersAsync(string searchText, string stateOrProvince = null, int count = 0);
    }
}
