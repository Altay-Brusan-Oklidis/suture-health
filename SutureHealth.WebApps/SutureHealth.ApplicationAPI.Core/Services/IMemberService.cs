using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SutureHealth.Application.Members;
using SutureHealth.Storage;

namespace SutureHealth.Application.Services
{
    public interface IMemberService
    {
        Task<int> CreateMemberAsync(UpdateMemberRequest request, int createdByMemberId);

        Task<Member> GetMemberByIdAsync(int memberID);
        Task<Member> GetMemberByNameAsync(string userName);
        Task<Member> GetMemberByOrganizationMemberIdAsync(long organizationMemberId);
        Task<Member> GetMemberByNPIAsync(long npi);

        IQueryable<Member> GetMembers();
        IQueryable<Member> GetMembersById(params int[] memberId);
        IQueryable<Member> GetMembersByOrganizationId(int organizationId);
        IQueryable<Member> GetMembersByOrganizationIds(int[] organizationIds);

        IQueryable<MemberRelationship> GetMemberRelationships();
        IQueryable<MemberRelationship> GetSupervisorsForMemberId(int memberId);
        IQueryable<MemberRelationship> GetSubordinatesForMemberId(int memberId);

        Task UpdateMemberAsync(int memberId, UpdateMemberRequest request, int updatedByMemberId);
        Task<bool> ToggleMemberActiveStatusAsync(int memberId);
        Task CreateMemberImage(MemberImage memberImage);
        Task CreateMemberImages(MemberImage[] memberImages);
        Task<MemberImage> GetMemberImage(int memberId, bool isPrimary, MemberImageSizeType sizeType);
        Task<IEnumerable<MemberImage>> GetMemberImages(int memberId);
        Task<IQueryable<MemberImage>> GetMemberImages(int[] memberIds, bool isPrimary, MemberImageSizeType sizeType);
        Task DeleteMemberImages(MemberImage[] memberImages);
        Task<StorageServiceResponse> InsertMemberImageToS3(string memberImageId, IFormFile image);
        Task<StorageServiceResponse> DeleteMemberImageFromS3(string memberImageId);
        //TODO: remove both updates
        //Task<StorageServiceResponse> UpdateMemberImageS3(string memberImageId, IFormFile image);
        Task<StorageServiceResponse> UpdateMemberImageS3(string memberImageId, IFormFile image, string newMemberImageId = null);
        Task<ImageStream> GetMemberImageFromS3(string memberImageId);
        Task<bool> IsMemberSurrogateSenderAsync(int memberId);
        Task<bool> IsMemberSurrogateSenderAsync(MemberBase member);
    }
}
