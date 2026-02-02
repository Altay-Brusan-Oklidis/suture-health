using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SutureHealth.Application.Organizations;
using SutureHealth.Storage;

namespace SutureHealth.Application.Services
{
    public interface IOrganizationService
    {
        Task<int> CreateOrganizationAsync(CreateOrganizationRequest request, int createdByMemberId);

        Task<Organization> GetOrganizationByIdAsync(int organizationId);
        Task<Organization> GetOrganizationByNPIAsync(long npi);
        Task<Organization> GetOrganizationByOrganizationMemberIdAsync(long organizationMemberId);

        IQueryable<Organization> GetOrganizations();
        IQueryable<Organization> GetOrganizationsByIdAsync(params int[] organizationIds);
        Task<IEnumerable<OrganizationType>> GetOrganizationTypes();
        IQueryable<Organization> GetOrganizationsByName(string searchText);

        Task<bool> ToggleOrganizationActiveStatusAsync(int organizationId, int updatedByMemberId);
        Task UpdateOrganizationAsync(int organizationId, UpdateOrganizationRequest request, int updatedByMemberId);
        Task CreateOrganizationImage(OrganizationImage organizationImage);
        Task<OrganizationImage> GetOrganizationImage(int organizationId, bool isPrimary);
        Task<IEnumerable<OrganizationImage>> GetOrganizationImages(int organizationId);
        Task<IEnumerable<OrganizationImage>> GetMultipleOrganizationImages(int[] organizationIds);
        void DeleteOrganizationImages(OrganizationImage[] organizationImages);
        Task<StorageServiceResponse> InsertOrganizationImageToS3(string organizationImageId, IFormFile image);
        Task<StorageServiceResponse> DeleteOrganizationImageFromS3(string organizationImageId);
        Task<StorageServiceResponse> UpdateOrganizationImage(string organizationImageId, IFormFile image);
        Task<ImageStream> GetOrganizationImageFromS3(string organizationImageId);
        Task<bool> IsSubscribedToInboxMarketingAsync(int organizationId);
        Task<bool> UpdateInboxMarketingSubscriptionAsync(int organizationId, bool active);
        IQueryable<BillableEntity> GetBillableEntities(params int[] organizationIds);
        Task<int> GetMaxUploadKilobytesAsync(int? organizationId = null);
        Task<IEnumerable<(OrganizationMember OrganizationMember, BillableEntity BillableEntity)>> GetSurrogateSendingOrganizationsAsync(int memberId);
    }
}
