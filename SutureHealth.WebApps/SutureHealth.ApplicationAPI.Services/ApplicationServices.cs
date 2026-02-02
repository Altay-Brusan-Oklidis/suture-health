using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SutureHealth.Application.Members;
using SutureHealth.Application.Organizations;
using SutureHealth.Diagnostics;
using SutureHealth.Storage;

namespace SutureHealth.Application.Services
{
    public class ApplicationServices<TDbContext> : IApplicationService,
                                                   IApplicationSettingService,
                                                   IIntegratorService,
                                                   IMemberService,
                                                   IOrganizationService,
                                                   IOrganizationMemberService
        where TDbContext : ApplicationDbContext
    {
        protected TDbContext ApplicationContext { get; set; }
        protected ILogger<IApplicationService> Logger { get; }
        protected ITracingService Tracer { get; }
        protected IStorageService StorageService { get; }
        protected IConfiguration ConfigurationService { get; }

        protected readonly string MemberImageBucketFolder;
        protected readonly string OrganizationImageBucketFolder;

        public ApplicationServices
        (
            TDbContext accountContext,
            ILogger<IApplicationService> logger,
            ITracingService tracer,
            IStorageService storageService,
            IConfiguration configurationService
        )
        {
            ApplicationContext = accountContext;
            Logger = logger;
            Tracer = tracer;
            StorageService = storageService;
            ConfigurationService = configurationService;

            var section = ConfigurationService.GetSection("SutureHealth");
            MemberImageBucketFolder = section.GetValue("S3MemberImagesStorageBucket", "");
            OrganizationImageBucketFolder = section.GetValue("S3OrganizationImagesStorageBucket", "");
        }

        #region IApplicationServices

        //async Task<bool> IApplicationService.CanMemberEditOrganizationProfileAsync(int memberId, int? organizationId = null)
        //    => await this.ApplicationContext.OrganizationMembers.AsNoTracking()
        //                                                        .Where(om => om.MemberId == memberId && om.IsActive && om.CanEditProfile && (organizationId == null || om.OrganizationId == organizationId))
        //                                                        .AnyAsync();


        async Task<IEnumerable<OrganizationMember>> IApplicationService.GetSigningOrganizationMembersAsync(string searchText, string organizationStateOrProvinceFilter, int count)
            => await ApplicationContext.GetSigningOrganizationMembersAsync(searchText, organizationStateOrProvinceFilter, count);

        #endregion
        #region IApplicationSettingService

        async Task IApplicationSettingService.DeleteApplicationSettingAsync(int settingId)
            => await DeleteSettingAsync(ApplicationContext.ApplicationSettings, settingId);

        async Task IApplicationSettingService.DeleteMemberSettingAsync(int settingId)
            => await DeleteSettingAsync(ApplicationContext.MemberSettings, settingId);

        async Task IApplicationSettingService.DeleteOrganizationSettingAsync(int settingId)
            => await DeleteSettingAsync(ApplicationContext.OrganizationSettings, settingId);

        async Task DeleteSettingAsync<TSetting>(DbSet<TSetting> set, int settingId)
            where TSetting : GenericSetting
        {
            var setting = await set.FirstOrDefaultAsync(s => s.SettingId == settingId);
            if (setting != null)
            {
                set.Remove(setting);
                await ApplicationContext.SaveChangesAsync();
            }
        }

        public IQueryable<ApplicationSetting> GetApplicationSettings()
            => ApplicationContext.ApplicationSettings.AsNoTracking();

        IQueryable<MemberSetting> IApplicationSettingService.GetMemberSettings()
            => ApplicationContext.MemberSettings.AsNoTracking();

        IQueryable<MemberSetting> IApplicationSettingService.GetMemberSettings(int memberId)
            => ApplicationContext.MemberSettings.AsNoTracking()
                                                .Where(ms => ms.ParentId == memberId);

        public IQueryable<OrganizationSetting> GetOrganizationSettings()
            => ApplicationContext.OrganizationSettings.AsNoTracking();

        public IQueryable<OrganizationSetting> GetOrganizationSettings(int organizationId)
            => ApplicationContext.OrganizationSettings.AsNoTracking()
                                                      .Where(ms => ms.ParentId == organizationId);

        async Task<ApplicationSetting> IApplicationSettingService.SetApplicationSettingAsync(int settingId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active)
            => await SetSettingAsync(await ApplicationContext.ApplicationSettings.FirstAsync(s => s.SettingId == settingId), key, valueBool, valueInt, valueString, type, active);

        async Task<MemberSetting> IApplicationSettingService.SetMemberSettingAsync(int settingId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active)
            => await SetSettingAsync(await ApplicationContext.MemberSettings.FirstAsync(s => s.SettingId == settingId), key, valueBool, valueInt, valueString, type, active);

        async Task<OrganizationSetting> IApplicationSettingService.SetOrganizationSettingAsync(int settingId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active)
            => await SetSettingAsync(await ApplicationContext.OrganizationSettings.FirstAsync(s => s.SettingId == settingId), key, valueBool, valueInt, valueString, type, active);

        public async Task<ApplicationSetting> AddApplicationSettingAsync(string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active = true)
            => await SetSettingAsync(new ApplicationSetting(), key, valueBool, valueInt, valueString, type, active);

        public async Task<MemberSetting> AddMemberSettingAsync(int memberId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active = true)
            => await SetSettingAsync(new MemberSetting() { ParentId = memberId }, key, valueBool, valueInt, valueString, type, active);

        public async Task<OrganizationSetting> AddOrganizationSettingAsync(int organizationId, string key, bool? valueBool, int? valueInt, string valueString, ItemType? type, bool active = true)
            => await SetSettingAsync(new OrganizationSetting() { ParentId = organizationId }, key, valueBool, valueInt, valueString, type, active);

        protected async Task<TSetting> SetSettingAsync<TSetting>
        (
            TSetting setting,
            string key,
            bool? valueBool,
            int? valueInt,
            string valueString,
            ItemType? type,
            bool active
        )
            where TSetting : GenericSetting
        {
            var dbSet = setting switch
            {
                MemberSetting => ApplicationContext.MemberSettings as DbSet<TSetting>,
                OrganizationSetting => ApplicationContext.OrganizationSettings as DbSet<TSetting>,
                ApplicationSetting => ApplicationContext.ApplicationSettings as DbSet<TSetting>,
                _ => throw new NotImplementedException($"No implementation exists for {setting.GetType().Name}")
            };

            if (ApplicationContext.Entry(setting).State == EntityState.Detached)
            {
                dbSet.Add(setting);
            }

            setting.Key = key;
            setting.ItemBool = valueBool;
            setting.ItemInt = valueInt;
            setting.ItemString = valueString;
            setting.ItemType = type;
            setting.IsActive = active;

            await ApplicationContext.SaveChangesAsync();

            return setting;
        }

        public HierarchicalSetting GetHierarchicalSetting(string key, int memberId, int? organizationId = null)
            => ApplicationContext.GetHierarchicalSetting(key, memberId, organizationId);

        public bool ShowLegacyNavBar(bool isSender, int memberId, bool contentOnly)
        {
            if (isSender || contentOnly)
            {
                return false;
            }

            try
            {
                var setting = ApplicationContext.MemberSettings.Single(
                    memberSetting => memberSetting.ParentId == memberId && memberSetting.Key == "InboxPreference");
                return setting.ItemInt == 0;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #endregion
        #region IIntegratorServices

        async Task<Integrator> IIntegratorService.GetIntegratorByIdAsync(Guid initiator)
            => await ApplicationContext.Integrators.Include(i => i.Organizations).ThenInclude(o => o.Organization)
                                                .Where(i => string.Equals(i.ApiKey, initiator.ToString()))
                                                .SingleOrDefaultAsync();

        async Task<Organization> IIntegratorService.GetOrganizationByApiKeyAsync(string apiKey)
            => await (from io in ApplicationContext.IntegratorOrganizations
                      where string.Equals(io.ApiKey, apiKey, StringComparison.OrdinalIgnoreCase)
                      select io.Organization).SingleOrDefaultAsync();

        #endregion
        #region IMemberServices

        async Task<int> IMemberService.CreateMemberAsync(UpdateMemberRequest request, int createdByMemberId) =>
            await ApplicationContext.UpsertMemberAsync(null, request, createdByMemberId);

        public IQueryable<Member> GetMembers()
            => ApplicationContext.Members.AsNoTracking().Include(m => m.Contacts);

        async Task<Member> GetMemberByExpression(Expression<Func<Member, bool>> expression)
            => await ApplicationContext.Members.Where(expression).Include(m => m.Contacts).SingleOrDefaultAsync();

        public async Task<Member> GetMemberByIdAsync(int memberID)
            => await GetMemberByExpression(m => m.MemberId == memberID);

        async Task<Member> IMemberService.GetMemberByNameAsync(string userName)
            => await GetMemberByExpression(m => m.UserName.ToUpper() == userName.ToUpper());

        async Task<Member> IMemberService.GetMemberByNPIAsync(long npi)
            => await GetMemberByExpression(m => m.NPI == npi.ToString());

        async Task<Member> IMemberService.GetMemberByOrganizationMemberIdAsync(long organizationMemberID)
            => await ApplicationContext.OrganizationMembers.Where(m => m.OrganizationMemberId == organizationMemberID)
                                                           .Include(m => m.Member).ThenInclude(m => m.Contacts)
                                                           .Select(om => om.Member)
                                                           .SingleOrDefaultAsync();

        IQueryable<Member> IMemberService.GetMembersByOrganizationId(int organizationId)
            => ApplicationContext.OrganizationMembers.Where(om => om.OrganizationId == organizationId)
                                                  .Include(om => om.Member).ThenInclude(m => m.Contacts)
                                                  .Select(om => om.Member);

        IQueryable<Member> IMemberService.GetMembersByOrganizationIds(int[] organizationIds)
            => ApplicationContext.OrganizationMembers.Where(om => (organizationIds.Contains(om.OrganizationId)))
                                                    .Include(om => om.Member)
                                                    .Select(om => om.Member);

        IQueryable<Member> IMemberService.GetMembersById(params int[] memberId)
            => ApplicationContext.Members.Include(m => m.Contacts).Where(m => memberId.Contains(m.MemberId));

        IQueryable<MemberRelationship> IMemberService.GetMemberRelationships()
            => ApplicationContext.MemberRelationships.AsNoTracking()
                                                     .Include(mr => mr.Subordinate)
                                                     .Include(mr => mr.Supervisor);

        IQueryable<MemberRelationship> IMemberService.GetSupervisorsForMemberId(int memberId)
            => ApplicationContext.MemberRelationships.AsNoTracking()
                                                     .Include(mr => mr.Subordinate)
                                                     .Include(mr => mr.Supervisor)
                                                     .Where(mr => mr.SubordinateMemberId == memberId);

        IQueryable<MemberRelationship> IMemberService.GetSubordinatesForMemberId(int memberId)
            => ApplicationContext.MemberRelationships.AsNoTracking()
                                                     .Include(mr => mr.Subordinate)
                                                     .Include(mr => mr.Supervisor)
                                                     .Where(mr => mr.SupervisorMemberId == memberId);

        async Task IMemberService.UpdateMemberAsync(int memberId, UpdateMemberRequest request, int updatedByMemberId) =>
            await ApplicationContext.UpsertMemberAsync(memberId, request, updatedByMemberId);

        async Task<bool> IMemberService.ToggleMemberActiveStatusAsync(int memberId)
            => await ApplicationContext.ToggleMemberActiveStatusAsync(memberId);

        async Task IMemberService.CreateMemberImage(MemberImage memberImage)
            => await ApplicationContext.CreateMemberImage(memberImage);

        async Task IMemberService.CreateMemberImages(MemberImage[] memberImages)
            => await ApplicationContext.CreateMemberImages(memberImages);

        async Task<MemberImage> IMemberService.GetMemberImage(int memberId, bool isPrimary, MemberImageSizeType sizeType)
        => await ApplicationContext.MemberImages
                                    .FirstOrDefaultAsync(mi => (mi.MemberId == memberId &&
                                                                mi.IsPrimary == isPrimary &&
                                                                mi.SizeType == sizeType));

        async Task<IEnumerable<MemberImage>> IMemberService.GetMemberImages(int memberId)
            => ApplicationContext.MemberImages.Where(mi => mi.MemberId == memberId);

        async Task<IQueryable<MemberImage>> IMemberService.GetMemberImages(int[] memberIds, bool isPrimary, MemberImageSizeType sizeType)
            => ApplicationContext.MemberImages.Where(mi => memberIds.Any(mId => mId == mi.MemberId) &&
                                                           mi.IsPrimary == isPrimary &&
                                                           mi.SizeType == sizeType);

        async Task IMemberService.DeleteMemberImages(MemberImage[] memberImages)
            => await ApplicationContext.DeleteMemberImages(memberImages);

        public async Task<StorageServiceResponse> InsertMemberImageToS3(string memberImageId, IFormFile image)
        {
            StorageServiceResponse response = null;
            using (var imageStream = new MemoryStream())
            {
                await image.CopyToAsync(imageStream);
                StorageServicePutRequest memberImagePutRequest = new StorageServicePutRequest()
                {
                    ContentType = image.ContentType,
                    Container = MemberImageBucketFolder,
                    InputStream = imageStream,
                    Key = memberImageId.ToString()
                };
                response = await StorageService.PutObjectAsync(memberImagePutRequest);
            }
            return response;
        }

        public async Task<StorageServiceResponse> DeleteMemberImageFromS3(string memberImageId)
        {
            StorageServiceRequest deleteRequest = new()
            {
                Key = memberImageId.ToString(),
                Container = MemberImageBucketFolder
            };

            var response = await StorageService.DeleteObjectAsync(deleteRequest);
            return response;
        }

        public async Task<StorageServiceResponse> UpdateMemberImageS3(string memberImageId, IFormFile image, string newMemberImageId = null)
        {
            StorageServiceResponse response = null;

            var deleteResponse = await DeleteMemberImageFromS3(memberImageId);
            using (var imageStream = new MemoryStream())
            {
                await image.CopyToAsync(imageStream);
                StorageServicePutRequest memberImagePutRequest = new StorageServicePutRequest()
                {
                    ContentType = image.ContentType,
                    Container = MemberImageBucketFolder,
                    InputStream = imageStream,
                    Key = newMemberImageId == null ? newMemberImageId.ToString() : memberImageId.ToString()
                };
                response = await StorageService.PutObjectAsync(memberImagePutRequest);
            }
            return response;
        }

        public async Task<ImageStream> GetMemberImageFromS3(string memberImageId)
        {
            StorageServiceRequest getImageRequest = new StorageServiceRequest()
            {
                Key = memberImageId.ToString(),
                Container = MemberImageBucketFolder,
            };
            try
            {
                var response = await StorageService.GetObjectAsync(getImageRequest);
                using (var responseStream = response.ResponseStream)
                {
                    var stream = new MemoryStream();
                    await responseStream.CopyToAsync(stream);
                    stream.Position = 0;
                    return new ImageStream()
                    {
                        Stream = stream,
                        ContentType = response.ContentType
                    };
                }
            }
            catch (Exception e)
            {
                return new ImageStream()
                {
                    Error = "There is no associated image with given member Id"
                };
            }
        }

        public async Task<bool> IsMemberSurrogateSenderAsync(int memberId)
        {
            var member = await GetMemberByIdAsync(memberId);
            return await IsMemberSurrogateSenderAsync(member);
        }

        public async Task<bool> IsMemberSurrogateSenderAsync(MemberBase member)
        {
            ArgumentNullException.ThrowIfNull(nameof(member));
            if (!member.IsUserSender())
                return false;

            return (await GetSurrogateSendingOrganizationsAsync(member.MemberId)).Any();
        }

        #endregion
        #region IOrganizationServices

        public async Task<IEnumerable<(OrganizationMember OrganizationMember, BillableEntity BillableEntity)>>
            GetSurrogateSendingOrganizationsAsync(int memberId)
        {
            const int SurrogateSendingSysServiceId = 4;

            var xs = await GetOrganizationMembersByMemberId(memberId)
                .Where(om => om.IsActive)
                .Join(
                    ApplicationContext.BillableEntites
                        .Where(be => be.IsSubscribed && be.SystemServiceId == SurrogateSendingSysServiceId),
                    om => om.OrganizationId,
                    be => be.OrganizationId,
                    (om, be) => new { om, be })
                .ToListAsync();

            return xs.Select(x => (OrganizationMember: x.om, BillableEntity: x.be));
        }

        async Task<int> IOrganizationService.CreateOrganizationAsync(CreateOrganizationRequest request, int createdByMemberId)
            => await ApplicationContext.CreateOrganizationAsync(request, createdByMemberId);

        IQueryable<Organization> IOrganizationService.GetOrganizations()
            => ApplicationContext.Organizations.AsNoTracking()
                                               .Include(o => o.OrganizationType)
                                               .Include(o => o.Contacts);

        public IQueryable<Organization> GetOrganizationsByName(string searchText)
            => ApplicationContext.Organizations.AsNoTracking()
                                               .Where(o => EF.Functions.Like(o.Name, $"%{searchText}%") || EF.Functions.Like(o.OtherDesignation, $"%{searchText}%"));

        public IQueryable<Organization> GetOrganizationsByIdAsync(params int[] organizationIds)
            => ApplicationContext.Organizations.Where(o => organizationIds.Any(id => o.OrganizationId == id))
                                               .Include(o => o.Contacts);

        async Task<Organization> IOrganizationService.GetOrganizationByIdAsync(int organizationId)
            => await GetOrganizationsByIdAsync(organizationId).SingleOrDefaultAsync();

        async Task<Organization> IOrganizationService.GetOrganizationByNPIAsync(long npi)
            => await ApplicationContext.Organizations.Where(o => o.NPI == npi.ToString())
                                                     .SingleOrDefaultAsync();

        async Task<Organization> IOrganizationService.GetOrganizationByOrganizationMemberIdAsync(long organizationMemberID)
            => await ApplicationContext.OrganizationMembers.Where(m => m.OrganizationMemberId == organizationMemberID)
                                                           .Include(m => m.Organization).ThenInclude(m => m.Contacts)
                                                           .Select(om => om.Organization)
                                                           .SingleOrDefaultAsync();

        async Task<IEnumerable<OrganizationType>> IOrganizationService.GetOrganizationTypes()
            => await ApplicationContext.OrganizationTypes.AsNoTracking()
                                                         .OrderBy(ot => ot.Name)
                                                         .ToArrayAsync();


        async Task<bool> IOrganizationService.ToggleOrganizationActiveStatusAsync(int organizationId, int updatedByMemberId)
            => await ApplicationContext.ToggleOrganizationActiveStatusAsync(organizationId, updatedByMemberId);

        async Task IOrganizationService.UpdateOrganizationAsync(int organizationId, UpdateOrganizationRequest request, int updatedByMemberId)
            => await ApplicationContext.UpdateOrganizationAsync(organizationId, request, updatedByMemberId);

        async Task IOrganizationService.CreateOrganizationImage(OrganizationImage organizationImage)
            => await ApplicationContext.CreateOrganizationImage(organizationImage);

        async Task<OrganizationImage> IOrganizationService.GetOrganizationImage(int organizationId, bool isPrimary)
            => await ApplicationContext.OrganizationImages
                                    .FirstOrDefaultAsync(oi => (oi.OrganizationId == organizationId && oi.IsPrimary == isPrimary));

        async Task<IEnumerable<OrganizationImage>> IOrganizationService.GetOrganizationImages(int organizationId)
            => ApplicationContext.OrganizationImages.Where(oi => oi.OrganizationId == organizationId);

        async Task<IEnumerable<OrganizationImage>> IOrganizationService.GetMultipleOrganizationImages(int[] organizationIds)
            => ApplicationContext.OrganizationImages.Where(oi => organizationIds.Contains(oi.OrganizationId) &&
                                                                                          !oi.IsPrimary);

        void IOrganizationService.DeleteOrganizationImages(OrganizationImage[] organizationImages)
            => ApplicationContext.DeleteOrganizationImages(organizationImages);

        public async Task<StorageServiceResponse> InsertOrganizationImageToS3(string organizationImageId, IFormFile image)
        {
            StorageServiceResponse response = null;
            using (var imageStream = new MemoryStream())
            {
                await image.CopyToAsync(imageStream);
                StorageServicePutRequest memberImagePutRequest = new StorageServicePutRequest()
                {
                    ContentType = image.ContentType,
                    Container = OrganizationImageBucketFolder,
                    InputStream = imageStream,
                    Key = organizationImageId
                };
                response = await StorageService.PutObjectAsync(memberImagePutRequest);
            }
            return response;
        }

        public async Task<StorageServiceResponse> DeleteOrganizationImageFromS3(string organizationImageId)
        {
            StorageServiceRequest deleteRequest = new StorageServiceRequest()
            {
                Key = organizationImageId,
                Container = OrganizationImageBucketFolder
            };

            var response = await StorageService.DeleteObjectAsync(deleteRequest);
            return response;
        }

        public async Task<StorageServiceResponse> UpdateOrganizationImage(string organizationImageId, IFormFile image)
        {
            StorageServiceResponse response = null;

            var deleteResponse = await DeleteOrganizationImageFromS3(organizationImageId.ToString());
            using (var imageStream = new MemoryStream())
            {
                await image.CopyToAsync(imageStream);
                StorageServicePutRequest organizationImagePutRequest = new StorageServicePutRequest()
                {
                    ContentType = image.ContentType,
                    Container = OrganizationImageBucketFolder,
                    InputStream = imageStream,
                    Key = organizationImageId
                };
                response = await StorageService.PutObjectAsync(organizationImagePutRequest);
            }
            return response;
        }

        public async Task<ImageStream> GetOrganizationImageFromS3(string organizationImageId)
        {
            StorageServiceRequest getImageRequest = new StorageServiceRequest()
            {
                Key = organizationImageId,
                Container = OrganizationImageBucketFolder,
            };
            try
            {
                var response = await StorageService.GetObjectAsync(getImageRequest);
                using (var responseStream = response.ResponseStream)
                {
                    var stream = new MemoryStream();
                    await responseStream.CopyToAsync(stream);
                    stream.Position = 0;
                    return new ImageStream()
                    {
                        Stream = stream,
                        ContentType = response.ContentType
                    };
                }
            }
            catch (Exception e)
            {
                return new ImageStream()
                {
                    Error = "There is no associated image with given organization Id"
                };
            }
        }

        async Task<bool> IOrganizationService.IsSubscribedToInboxMarketingAsync(int organizationId)
            => await ApplicationContext.IsSubscribedToInboxMarketingAsync(organizationId);

        async Task<bool> IOrganizationService.UpdateInboxMarketingSubscriptionAsync(int organizationId, bool active)
            => await ApplicationContext.UpdateInboxMarketingSubscriptionAsync(organizationId, active);

        public IQueryable<BillableEntity> GetBillableEntities(params int[] organizationIds)        
           => ApplicationContext.BillableEntites.Where(be => organizationIds.Contains(be.OrganizationId));
        

        public async Task<int> GetMaxUploadKilobytesAsync(int? organizationId = null)
        {
            if (organizationId.HasValue &&
                (await GetOrganizationSettings().FirstOrDefaultAsync(s => s.ParentId == organizationId && s.Key == "DocumentSizeLimit" && s.IsActive == true && s.ItemInt != null)) is OrganizationSetting organizationSetting)
            {
                return organizationSetting.ItemInt.Value;
            }

            return (await GetApplicationSettings().FirstOrDefaultAsync(s => s.Key == "DocumentSizeLimit" && s.IsActive == true && s.ItemInt != null))?.ItemInt ?? 2048;
        }

        #endregion
        #region IOrganizationMemberService

        async Task<OrganizationMember> IOrganizationMemberService.GetAutomatedOrganizationMemberByOrganizationIdAsync(int organizationId)
        {
            var now = DateTime.UtcNow;
            return await ApplicationContext.OrganizationMembers.Where(om => om.OrganizationId == organizationId)
                                                            .Where(om => om.IsActive == true)
                                                            .Where(om => (om.EffectiveAt ?? now) <= now)
                                                            .Where(om => om.IsAutomatedUser == true)
                                                            .OrderByDescending(om => om.OrganizationMemberId)
                                                            .FirstOrDefaultAsync();
        }

        IQueryable<OrganizationMember> GetOrganizationMembers(bool? isActive = null)
            => ApplicationContext.OrganizationMembers.Include(om => om.Organization)
                                                     .Include(om => om.Organization).ThenInclude(m => m.Contacts)
                                                     .Include(om => om.Member).ThenInclude(m => m.Contacts)
                                                     .Where(om => om.IsActive == (isActive ?? om.IsActive))
                                                     .AsNoTracking();

        IQueryable<OrganizationMember> IOrganizationMemberService.GetOrganizationMembers(bool? isActive)
            => GetOrganizationMembers(isActive);

        public IQueryable<OrganizationMember> GetOrganizationMembersByMemberId(params int[] memberIds)
            => GetOrganizationMembers().Where(om => memberIds.Any(id => om.MemberId == id));

        IQueryable<OrganizationMember> IOrganizationMemberService.GetOrganizationMembersByOrganizationId(params int[] organizationIds)
            => GetOrganizationMembers().Where(om => organizationIds.Any(id => om.OrganizationId == id));

        IQueryable<OrganizationMember> IOrganizationMemberService.GetAdminOrganizationMembersByOrganizationId(params int[] organizationIds)
            => ApplicationContext.OrganizationMembers.Where(om => (organizationIds.Any(id => om.OrganizationId == id) && om.IsAdministrator));

        #endregion

        #region IFhirUserConflationService

        async Task<FhirUserConflation?> IFhirUserConflationService.GetConflatedUser(string fhirId) => await ApplicationContext
            .FhirUserConflations.Where(entry => entry.FhirId == fhirId).SingleOrDefaultAsync();

        async Task IFhirUserConflationService.ConflateUser(FhirUserConflation conflation)
        {
            ApplicationContext.FhirUserConflations.Add(conflation);
            await ApplicationContext.SaveChangesAsync();
        }

        #endregion
    }
}
