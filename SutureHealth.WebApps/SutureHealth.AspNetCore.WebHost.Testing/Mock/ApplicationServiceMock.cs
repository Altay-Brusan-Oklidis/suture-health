using Microsoft.AspNetCore.Http;
using Moq;
using Moq.EntityFrameworkCore.DbAsyncQueryProvider;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.Storage;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SutureHealth.ApplicationAPI.Testing.MockServices
{
    public class ApplicationServiceMock : Mock<IApplicationService>
    {
        public ApplicationServiceMock()
        {
            _ = Setup(x => x.GetApplicationSettings()).Returns(GetApplicationSettings);
            _ = Setup(x => x.GetMemberSettings(It.IsAny<int>())).Returns(GetMemberSettings);
            _ = Setup(x => x.GetOrganizationByNPIAsync(It.IsAny<long>())).Returns(GetOrganizationByNPIAsync);
            _ = Setup(x => x.GetOrganizationSettings()).Returns(GetOrganizationSettings);
            _ = Setup(x => x.GetOrganizationMembers(It.IsAny<bool?>())).Returns(GetOrganizationMembers);
            _ = Setup(x => x.GetOrganizationMembersByMemberId(It.IsAny<int[]>())).Returns(GetOrganizationMembersByMemberId);
            _ = Setup(x => x.IsSubscribedToInboxMarketingAsync(It.IsAny<int>())).Returns(IsSubscribedToInboxMarketingAsync);
            _ = Setup(x => x.GetOrganizationImageFromS3(It.IsAny<string>())).Returns(GetOrganizationImageFromS3);
            _ = Setup(x => x.GetAdminOrganizationMembersByOrganizationId(It.IsAny<int[]>())).Returns(GetAdminOrganizationMembersByOrganizationId);
            _ = Setup(x => x.AddMemberSettingAsync(It.IsAny<int>(),
                                                   It.IsAny<string>(),
                                                   It.IsAny<bool?>(),
                                                   It.IsAny<int?>(),
                                                   It.IsAny<string>(),
                                                   It.IsAny<ItemType>(),
                                                   It.IsAny<bool>())).Returns(AddMemberSettingAsync);
            _ = Setup(x => x.GetOrganizations()).Returns(GetOrganizations);
            _ = Setup(x => x.InsertMemberImageToS3(It.IsAny<string>(),
                                                   It.IsAny<IFormFile>())).Returns(InsertMemberImageToS3);
            _ = Setup(x => x.UpdateOrganizationImage(It.IsAny<string>(),
                                       It.IsAny<IFormFile>())).Returns(UpdateOrganizationImage);
            _ = Setup(x => x.InsertOrganizationImageToS3(It.IsAny<string>(),
                                                   It.IsAny<IFormFile>())).Returns(InsertOrganizationImageToS3);
            _ = Setup(x => x.GetOrganizationImage(It.IsAny<int>(), It.IsAny<bool>())).Returns(GetOrganizationImage);
            _ = Setup(x => x.GetOrganizationImages(It.IsAny<int>())).Returns(GetOrganizationImages);
            //_ = Setup(x => x.UpdateMemberImageS3(It.IsAny<string>(),
            //                                       It.IsAny<IFormFile>())).Returns(UpdateMemberImageS3);
            _ = Setup(x => x.DeleteMemberImageFromS3(It.IsAny<string>())).Returns(DeleteMemberImageFromS3);
            _ = Setup(x => x.GetMemberImageFromS3(It.IsAny<string>())).Returns(GetMemberImageFromS3);
            _ = Setup(x => x.GetMemberImage(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<MemberImageSizeType>())).Returns(GetMemberImage);
            _ = Setup(x => x.GetMemberImages(It.IsAny<int>())).Returns(GetMemberImages);

        }

        private async Task<Organization> GetOrganizationByNPIAsync(long npiCode)
        {
            await Task.Delay(1000).WaitAsync(new TimeSpan(0, 0, 10));

            if (npiCode != 110101)
                return await Task.FromResult(null as Organization);

            var result = new Organization()
            {
                OrganizationId = 11010,
                Name = "org1",
                CompanyId = 12,
                IsActive = true,
                NPI = "110101"
            };

            return await Task.FromResult(result);
        }

        public IQueryable<ApplicationSetting> GetApplicationSettings()
        {
            return new InMemoryAsyncEnumerable<ApplicationSetting>(new List<ApplicationSetting>()
            {
                 new ApplicationSetting()
                {
                    ParentId = 0,
                    SettingId = 0,
                    Key ="EmailNow",
                    ItemBool = false,
                    ItemInt = null,
                    ItemString = null,
                    ItemType = (ItemType)1,
                    IsActive = true
                },
                new ApplicationSetting()
                {
                    ParentId = 1,
                    SettingId = 0,
                    Key ="DaysTillPasswordExpires",
                    ItemBool = null,
                    ItemInt = 20000,
                    ItemString = null,
                    ItemType = (ItemType)2,
                    IsActive = true
                }
            }.AsQueryable());
        }

        public IQueryable<MemberSetting> GetMemberSettings(int memberId)
        {
            return new InMemoryAsyncEnumerable<MemberSetting>(new List<MemberSetting>()
            {
                 new MemberSetting()
                 {
                    ParentId = 3000018,
                    SettingId = 0,
                    Key ="NetworkLastAccessDate",
                    ItemBool = null,
                    ItemInt = null,
                    ItemString = "637932493273327941",
                    ItemType = (ItemType)3,
                    IsActive = true
                 },
                 new MemberSetting()
                 {
                    ParentId = 3000018,
                    SettingId = 0,
                    Key ="ShowNewFeaturesNotification",
                    ItemBool = false,
                    ItemInt = null,
                    ItemString = null,
                    ItemType = (ItemType)1,
                    IsActive = true
                 },
                 new MemberSetting()
                 {
                    ParentId = 3000018,
                    SettingId = 0,
                    Key ="ViewFormsByStatus",
                    ItemBool = null,
                    ItemInt = null,
                    ItemString = "All|False",
                    ItemType = (ItemType)3,
                    IsActive = true
                 },
                 new MemberSetting()
                 {
                    ParentId = 3000018,
                    SettingId = 0,
                    Key ="IsFirstInboxTourDone",
                    ItemBool = true,
                    ItemInt = null,
                    ItemString = null,
                    ItemType = (ItemType)1,
                    IsActive = true
                 },
                 new MemberSetting()
                 {
                    ParentId = 5,
                    SettingId = 0,
                    Key ="IsFirstInboxTourDone",
                    ItemBool = false,
                    ItemInt = null,
                    ItemString = null,
                    ItemType = (ItemType)1,
                    IsActive = true
                 }
            }).Where(ms => ms.ParentId == memberId).AsQueryable();
        }

        public IQueryable<OrganizationSetting> GetOrganizationSettings()
        {
            return new InMemoryAsyncEnumerable<OrganizationSetting>(new List<OrganizationSetting>()
            {
                 new OrganizationSetting()
                 {
                    ParentId = 11010,
                    SettingId = 0,
                    Key ="Services",
                    ItemBool = null,
                    ItemInt = null,
                    ItemString = "Hospital",
                    ItemType = (ItemType)3,
                    IsActive = true
                 },
                 new OrganizationSetting()
                 {
                    ParentId = 11011,
                    SettingId = 0,
                    Key ="Services",
                    ItemBool = null,
                    ItemInt = null,
                    ItemString = "Hospital",
                    ItemType = (ItemType)3,
                    IsActive = true
                 },
                 new OrganizationSetting()
                 {
                    ParentId = 12,
                    SettingId = 0,
                    Key ="Services",
                    ItemBool = null,
                    ItemInt = null,
                    ItemString = "Hospital",
                    ItemType = (ItemType)3,
                    IsActive = true
                 },
                 new OrganizationSetting()
                 {
                    ParentId = 11009,
                    SettingId = 0,
                    Key ="Services",
                    ItemBool = null,
                    ItemInt = null,
                    ItemString = "Hospital",
                    ItemType = (ItemType)3,
                    IsActive = true
                 },
                  new OrganizationSetting()
                  {
                    ParentId = 11010,
                    SettingId = 0,
                    Key ="non-service",
                    ItemBool = null,
                    ItemInt = null,
                    ItemString = "duplicate test",
                    ItemType = (ItemType)3,
                    IsActive = true
                  },
                  new OrganizationSetting()
                  {
                    ParentId = 11010,
                    SettingId = 0,
                    Key ="non-service",
                    ItemBool = null,
                    ItemInt = null,
                    ItemString = "duplicate test",
                    ItemType = (ItemType)3,
                    IsActive = true
                  }
            }).AsQueryable();
        }

        public IQueryable<OrganizationMember> GetOrganizationMembers(bool? isActive = null)
        {
            return new InMemoryAsyncEnumerable<OrganizationMember>(new List<OrganizationMember>()
            {
                 new OrganizationMember()
                 {
                    OrganizationMemberId = 5000001,
                    MemberId = 3000018,
                    OrganizationId = 11010,
                    IsActive = true,
                    Organization = new Organization()
                     {
                        OrganizationId = 11010,
                        Name = "org1",
                        CompanyId = 12,
                        IsActive = true
                     }
                 },
                 new OrganizationMember()
                 {
                    OrganizationMemberId = 5000002,
                    MemberId = 3000018,
                    OrganizationId = 11011,
                    IsActive = true,
                    IsPrimary = true,
                    IsAdministrator = true,
                    Organization = new Organization()
                     {
                        OrganizationId = 11011,
                        Name = "org2",
                        CompanyId = 12,
                        IsActive = true
                     }
                 },
                 new OrganizationMember()
                 {
                    OrganizationMemberId = 5000003,
                    MemberId = 3000018,
                    OrganizationId = 11012,
                    IsActive = true,
                    Organization = new Organization()
                     {
                        OrganizationId = 11012,
                        Name = "org3",
                        CompanyId = 11,
                        IsActive = true
                     }
                 },
                 new OrganizationMember()
                 {
                    OrganizationMemberId = 5000004,
                    MemberId = 3000018,
                    OrganizationId = 11009,
                    IsActive = true,
                    Organization = new Organization()
                     {
                        OrganizationId = 11009,
                        Name = "org4",
                        CompanyId = 11,
                        IsActive = true
                     }
                 }
            }).AsQueryable();
        }

        public async Task<MemberSetting> AddMemberSettingAsync(int memberId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active = true)
        {
            return new MemberSetting()
            {

            };
        }

        public IQueryable<Organization> GetOrganizations()
        {
            return new InMemoryAsyncEnumerable<Organization>(new List<Organization>()
            {
                 new Organization()
                 {
                    OrganizationId = 11010,
                    Name = "org1",
                    CompanyId = 12,
                    IsActive = true
                 },
                 new Organization()
                 {
                    OrganizationId = 11011,
                    Name = "org2",
                    CompanyId = 12,
                    IsActive = true
                 },
                 new Organization()
                 {
                    OrganizationId = 11012,
                    Name = "org3",
                    CompanyId = 11,
                    IsActive = true
                 },
                 new Organization()
                 {
                    OrganizationId = 11009,
                    Name = "org4",
                    CompanyId = 11,
                    IsActive = true
                 }
            }).AsQueryable();
        }

        public IQueryable<OrganizationMember> GetOrganizationMembersByMemberId(params int[] memberIds)
        {
            return GetOrganizationMembers().Where(om => memberIds.Any(id => om.MemberId == id));
        }

        public async Task<StorageServiceResponse> InsertMemberImageToS3(string memberImageId, IFormFile image)
        {
            return await Task.FromResult<StorageServiceResponse>(
                new StorageServiceResponse()
                {
                    ContentLength = image.Length,
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                });
        }

        public async Task<StorageServiceResponse> UpdateMemberImageS3(string memberImageId, IFormFile image)
        {
            return await Task.FromResult<StorageServiceResponse>(
                new StorageServiceResponse()
                {
                    ContentLength = image.Length,
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                });
        }

        public async Task<ImageStream> GetMemberImageFromS3(string memberImageId)
        {
            var imgStream = new ImageStream
            {
                ContentType = "image/png",
                Stream = new MemoryStream(new byte[50])
            };

            return await Task.FromResult<ImageStream>(imgStream);
        }

        public async Task<StorageServiceResponse> DeleteMemberImageFromS3(string memberImageId)
        {
            return await Task.FromResult<StorageServiceResponse>(new StorageServiceResponse()
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
            });

        }

        public IQueryable<OrganizationMember> GetAdminOrganizationMembersByOrganizationId(params int[] organizationIds)
        {
            return GetOrganizationMembers().Where(om => (organizationIds.Any(id => om.OrganizationId == id) && om.IsAdministrator)); ;
        }

        public async Task<StorageServiceResponse> InsertOrganizationImageToS3(string orgImageId, IFormFile image)
        {
            return await Task.FromResult<StorageServiceResponse>(new StorageServiceResponse()
            {
                ContentLength = image.Length,
                HttpStatusCode = System.Net.HttpStatusCode.OK,
            });
        }

        //UpdateOrganizationImage
        public async Task<StorageServiceResponse> UpdateOrganizationImage(string orgImageId, IFormFile image)
        {
            return await Task.FromResult<StorageServiceResponse>(
                new StorageServiceResponse()
                {
                    ContentLength = image.Length,
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                });
        }

        public async Task<ImageStream> GetOrganizationImageFromS3()
        {
            var imgStream = new ImageStream
            {
                ContentType = "image/png",
                Stream = new MemoryStream(new byte[50])
            };

            return await Task.FromResult<ImageStream>(imgStream);
        }

        public async Task<MemberImage> GetMemberImage(int memberId, bool isPrimary)
        {
            return new MemberImage()
            {
                MemberImageId = Guid.NewGuid(),
                IsPrimary = isPrimary
            }; 
        }

        public async Task<IEnumerable<MemberImage>> GetMemberImages(int memberId)
        {
            return new List<MemberImage>()
            {
                new MemberImage()
                {
                    MemberImageId = Guid.NewGuid(),
                    IsPrimary = false
                }
            };
        }

        public async Task<OrganizationImage?> GetOrganizationImage(int organizationId, bool isPrimary)
        {
            if (organizationId == 11012)
                return null;

            return await Task.FromResult<OrganizationImage>(new OrganizationImage()
            {
                OrganizationImageId = Guid.NewGuid(),
                IsPrimary = isPrimary
            });
        }

        public async Task<IEnumerable<OrganizationImage>> GetOrganizationImages(int organizationId)
        {
            return new List<OrganizationImage>()
            {
                new OrganizationImage()
                {
                    OrganizationImageId = Guid.NewGuid(),
                    IsPrimary = false
                }
            };
        }

        public async Task<bool> IsSubscribedToInboxMarketingAsync(int organizationId)
        {
            if (organizationId == 11011 || organizationId == 11012)
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }
    }
}
