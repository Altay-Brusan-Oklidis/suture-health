using System.Linq;
using System.Threading.Tasks;

namespace SutureHealth.Application.Services
{
    public interface IOrganizationMemberService
    {
        Task<OrganizationMember> GetAutomatedOrganizationMemberByOrganizationIdAsync(int organizationId);
        IQueryable<OrganizationMember> GetOrganizationMembers(bool? IsActive = null);
        IQueryable<OrganizationMember> GetOrganizationMembersByMemberId(params int[] memberIds);
        IQueryable<OrganizationMember> GetOrganizationMembersByOrganizationId(params int[] organizationIds);
        IQueryable<OrganizationMember> GetAdminOrganizationMembersByOrganizationId(params int[] organizationIds);
    }
}
