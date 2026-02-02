using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using SutureHealth.AspNetCore.Mvc.Extensions;
using SutureHealth.AspNetCore.Areas.Admin.Models;
using SutureHealth.AspNetCore.Areas.Admin.Models.Member;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.Mvc.Attributes;
using SutureHealth.Application;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Member")]
    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    public class MemberController : Controller
    {
        protected IApplicationService SecurityService { get; }
        protected IIdentityService IdentityService { get; }
        protected ILogger<MemberController> Logger { get; }

        public MemberController
        (
            IApplicationService securityService,
            IIdentityService identityService,
            ILogger<MemberController> logger
        )
        {
            SecurityService = securityService;
            IdentityService = identityService;
            Logger = logger;
        }

        [HttpGet]
        [Route("Search", Name = "AdminMemberSearch")]
        public IActionResult Search()
        {
            return View(new SearchModel()
            {
                CurrentUser = CurrentUser,
                AddDialog = new Models.AddEntityToOrganizationDialogModel()
                {
                    ConfirmButtonLabel = "Add User",
                    ConfirmFunctionName = "AddMemberDialogConfirm",
                    DialogName = "AddMemberDialog",
                    DialogTitle = "Add User",
                    OrganizationFieldName = "AddMemberOrganization",
                    OrganizationDataSourceUrl = Url.RouteUrl("AdminMemberOrganizationsDataSource")
                },
                OrganizationDataSourceUrl = Url.RouteUrl("AdminMemberOrganizationsDataSource")
            });
        }

        [HttpPost]
        [Route("Search", Name = "AdminMemberMembersDataSource")]
        public async Task<IActionResult> MembersDataSource([DataSourceRequest] DataSourceRequest request, CancellationToken cancellationToken)
        {
            var query = IdentityService.GetMemberIdentities();
            if (request.Filters.GetFilterDescriptor("Email") is FilterDescriptor emailDescriptor)
            {
                var searchValue = emailDescriptor.Value.ToString();
                request.Filters.Remove("Email");
                query = query.Where(m => EF.Functions.Like(m.Email, $"%{searchValue}%"));
            }

            if (request.Filters.GetFilterDescriptor("MemberTypeId") is FilterDescriptor memberTypeDescriptor)
            {
                var searchValue = System.Convert.ToInt32(memberTypeDescriptor.Value.ToString());
                request.Filters.Remove("MemberTypeId");
                query = query.Where(m => m.MemberTypeId == searchValue);
            }

            if (request.Filters.GetFilterDescriptor("Role") is FilterDescriptor roleDescriptor)
            {
                var searchValue = (SearchModel.RoleTypes)System.Convert.ToInt32(roleDescriptor.Value.ToString());
                request.Filters.Remove("Role");
                query = searchValue switch
                {
                    SearchModel.RoleTypes.Sender => query.Where(m => m.MemberTypeId == (int)Application.MemberType.Staff),
                    SearchModel.RoleTypes.Collaborator => query.Where(m => m.IsCollaborator == true && m.CanSign == false),
                    SearchModel.RoleTypes.Signer => query.Where(m => m.CanSign == true && m.IsCollaborator == false),
                    SearchModel.RoleTypes.Staff => query.Where(m => m.IsCollaborator == false && m.CanSign == false && m.MemberTypeId != (int)Application.MemberType.Staff),
                    SearchModel.RoleTypes.SignerCollaborator => query.Where(m => m.CanSign == true && m.IsCollaborator == true),
                    _ => query
                };
            }

            if (request.Filters.GetFilterDescriptor("OrganizationId") is FilterDescriptor organizationIdDescriptor && int.TryParse(organizationIdDescriptor.Value.ToString(), out var organizationId))
            {
                request.Filters.Remove("OrganizationId");
                query = query.Join(IdentityService.GetOrganizationMembersByOrganizationId(organizationId), m => m.MemberId, om => om.MemberId, (m, om) => m);
            }

            request.Filters.Transform("IsPaid", "IsPayingClient");
            request.Filters.Transform("IsRegistered", "MustRegisterAccount");
            request.Sorts.Transform("IsPaid", sd => sd.Member = "IsPayingClient");
            request.Sorts.Transform("DateCreated", sd => sd.Member = "CreatedAt");
            request.Sorts.Transform("DateLastLoggedIn", sd => sd.Member = "LastLoggedInAt");
            request.Sorts.Transform("IsRegistered", sd =>
            {
                sd.Member = "MustRegisterAccount";
                sd.SortDirection = sd.SortDirection switch
                {
                    ListSortDirection.Ascending => ListSortDirection.Descending,
                    ListSortDirection.Descending => ListSortDirection.Ascending,
                    _ => ListSortDirection.Descending
                };
            });

            return Json(await query.ToDataSourceResultAsync(request, m => new MemberListItem()
            {
                MemberId = m.MemberId,
                FirstName = m.FirstName,
                LastName = m.LastName,
                UserName = m.UserName,
                Email = m.Email,
                MemberTypeId = m.MemberTypeId,
                MemberType = m.MemberType.GetEnumDescription(),
                Role = string.Join(" | ", new string[]
                {
                    m.MemberType == Application.MemberType.Staff ? "Sender" : null,
                    m.CanSign ? "Signer" : null,
                    m.IsCollaborator ? "Collaborator" : null
                }.Where(r => !string.IsNullOrWhiteSpace(r)).DefaultIfEmpty("Staff")),
                IsActive = m.IsActive,
                IsPaid = m.IsPayingClient,
                Npi = m.NPI,
                DateCreated = m.CreatedAt,
                DateCreatedDisplay = m.CreatedAt?.ToString("d") ?? string.Empty,
                IsLocked = m.LockoutEnd.HasValue && m.LockoutEnd.Value > DateTimeOffset.UtcNow,
                IsRegistered = !m.MustRegisterAccount,
                DateLastLoggedIn = m.LastLoggedInAt,
                DateLastLoggedInDisplay = m.LastLoggedInAt?.ToString("d") ?? string.Empty,

                DetailUrl = Url.RouteUrl("AdminMemberMemberDetail", new { memberId = m.MemberId }),
                EditActionUrl = Url.RouteUrl("MemberEdit", new { memberId = m.MemberId }),
                ChangePasswordModalUrl = Url.RouteUrl("MemberResetPasswordModal", new { memberId = m.MemberId }),
                ToggleActiveModalUrl = Url.RouteUrl("AdminMemberToggleActiveModal", new { memberId = m.MemberId }),
                SendRegistrationEmailModalUrl = Url.RouteUrl("MemberSendRegistrationEmailModal", new { memberId = m.MemberId }),
                UnlockActionUrl = Url.RouteUrl("MemberUnlock", new { memberId = m.MemberId }),
                SettingsActionUrl = Url.RouteUrl("AdminMemberSettings", new { memberId = m.MemberId }),
                CommunicationPreferencesActionUrl = Url.RouteUrl("MemberCommunicationPreferences", new { memberId = m.MemberId })
            }, cancellationToken));
        }

        [HttpGet]
        [Route("{memberId:int}/SearchDetail", Name = "AdminMemberMemberDetail")]
        public async Task<IActionResult> MemberDetail(int memberId)
        {
            return PartialView("_MemberDetail", new MemberDetailModel()
            {
                GridName = $"MemberDetail-{memberId}",
                Organizations = await SecurityService.GetOrganizationMembersByMemberId(memberId)
                                                     .Select(om => new MemberDetailModel.MemberDetailListItem()
                                                     {
                                                         OrganizationId = om.OrganizationId,
                                                         Name = om.Organization.Name,
                                                         InternalName = om.Organization.OtherDesignation,
                                                         IsActive = om.IsActive,
                                                         IsPrimary = om.IsPrimary,
                                                         IsAdministrator = om.IsAdministrator
                                                     })
                                                     .ToArrayAsync(),
                AddOrganizationDialog = new AddEntityToOrganizationDialogModel()
                {
                    DialogName = $"AddOrganization_{memberId}",
                    DialogTitle = "Add User to Organization",
                    OrganizationFieldName = $"AddOrganizationMember-{memberId}",
                    ConfirmButtonLabel = "Edit User",
                    OrganizationDataSourceUrl = Url.RouteUrl("AdminMemberAddOrganizationDataSource", new { memberId = memberId }),
                    ConfirmFunctionName = $"AddOrganizationDialogConfirm_{memberId}"
                }
            });
        }

        [HttpGet]
        [RequireMember]
        [Route("{memberId:int}/ToggleActive", Name = "AdminMemberToggleActiveModal")]
        public async Task<IActionResult> ToggleActiveModal([FromServices] SutureHealth.Requests.Services.IRequestServicesProvider requestService, Member member)
        {
            return PartialView("_ToggleActiveModal", new ToggleActiveModel()
            {
                UserName = member.UserName,
                IsCurrentlyActive = member.IsActive,
                HasPendingRequests = member.IsActive && await requestService.GetServiceableRequests()
                                                            .Where(sr => sr.SignerMemberId == member.MemberId && sr.RequestStatus == null)
                                                            .AnyAsync(),
                HasOrganizationRelationship = member.IsActive || await SecurityService.GetOrganizationMembersByMemberId(member.MemberId).AnyAsync(),
                IsSoleAdministrator = member.IsActive && await SecurityService.GetOrganizationMembers()
                                                            .Where(om => om.IsActive && om.IsAdministrator)
                                                            .Join(SecurityService.GetOrganizationMembersByMemberId(member.MemberId).Where(om => om.IsActive && om.IsAdministrator), om => om.OrganizationId, m_om => m_om.OrganizationId, (om, m_om) => om)
                                                            .GroupBy(om => om.OrganizationId, (id, oms) => oms.Count())
                                                            .AnyAsync(count => count == 1),
                ToggleActiveActionUrl = Url.RouteUrl("AdminMemberToggleActive", new { memberId = member.MemberId })
            });
        }

        [HttpPost]
        [RequireMember]
        [Route("{memberId:int}/ToggleActive", Name = "AdminMemberToggleActive")]
        public async Task<IActionResult> ToggleActive(Member member)
        {
            return (await SecurityService.ToggleMemberActiveStatusAsync(member.MemberId)) != member.IsActive ?
                Ok() :
                BadRequest();
        }

        [HttpPost]
        [Route("Search/Organizations", Name = "AdminMemberOrganizationsDataSource")]
        [Route("{memberId:int}/Search/Organizations", Name = "AdminMemberAddOrganizationDataSource")]
        public async Task<IActionResult> OrganizationsDataSource([DataSourceRequest] DataSourceRequest request, [FromRoute] int? memberId = null)
        {
            var search = request.Filters?.OfType<FilterDescriptor>().FirstOrDefault()?.Value.ToString() ?? string.Empty;
            var organizations = await SecurityService.GetOrganizationsByName(search)
                                                     .OrderBy(m => m.Name)
                                                     .Take(50)
                                                     .ToArrayAsync();
            return Json(new DataSourceResult()
            {
                Total = organizations.Count(),
                Data = organizations.Select(o => memberId.HasValue ?
                                                 new MemberDetailModel.OrganizationListItem()
                                                 {
                                                     OrganizationId = o.OrganizationId,
                                                     Name = $"{o.Name}{(!string.IsNullOrWhiteSpace(o.OtherDesignation) && !string.Equals(o.Name, o.OtherDesignation, StringComparison.InvariantCultureIgnoreCase) ? $" ({o.OtherDesignation})" : string.Empty)}",
                                                     EditMemberUrl = Url.RouteUrl("OrganizationMembersEdit", new { organizationId = o.OrganizationId, memberId = memberId.Value })
                                                 } :
                                                 new SearchModel.OrganizationListItem()
                                                 {
                                                     OrganizationId = o.OrganizationId,
                                                     Name = $"{o.Name}{(!string.IsNullOrWhiteSpace(o.OtherDesignation) && !string.Equals(o.Name, o.OtherDesignation, StringComparison.InvariantCultureIgnoreCase) ? $" ({o.OtherDesignation})" : string.Empty)}",
                                                     CreateMemberUrl = Url.RouteUrl("OrganizationMembersCreate", new { organizationId = o.OrganizationId })
                                                 } as OrganizationSearchListItem)
            });
        }

        [HttpGet]
        [RequireMember]
        [Route("{memberId:int}/Settings", Name = "AdminMemberSettings")]
        public IActionResult Settings(Member member)
        {
            return View(new SettingsModel()
            {
                CurrentUser = CurrentUser,
                UserName = member.UserName,
                SettingsGrid = new Models.SettingsGridModel()
                {
                    ReadUrl = Url.RouteUrl("AdminMemberSettingsDataSource", new { memberId = member.MemberId }),
                    CreateUrl = Url.RouteUrl("AdminMemberCreateSetting", new { memberId = member.MemberId }),
                    UpdateUrl = Url.RouteUrl("AdminMemberUpdateSetting", new { memberId = member.MemberId }),
                    DeleteUrl = Url.RouteUrl("AdminMemberDeleteSetting", new { memberId = member.MemberId })
                }
            });
        }

        [HttpGet]
        [RequireMember]
        [Route("{memberId:int}/Settings/Data", Name = "AdminMemberSettingsDataSource")]
        public IActionResult SettingsDataSource(Member member)
        {
            var settings = SecurityService.GetMemberSettings(member.MemberId);
            return Json(new DataSourceResult()
            {
                Data = settings.Select(s => new SettingsListItem(s)),
                Total = settings.Count()
            });
        }

        [HttpPost]
        [RequireMember]
        [Route("{memberId:int}/Settings/Update", Name = "AdminMemberUpdateSetting")]
        public async Task<IActionResult> UpdateSetting(Member member, [FromForm] SettingsListItem item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var setting = await SecurityService.SetMemberSettingAsync(
                item.SettingId,
                item.Name,
                item.Type == ItemType.Boolean.ToString() ? Convert.ToBoolean(item.Value) : null,
                item.Type == ItemType.Integer.ToString() ? Convert.ToInt32(item.Value) : null,
                item.Type == ItemType.String.ToString() ? item.Value : null,
                Enum.Parse<ItemType>(item.Type),
                item.Active);

            Logger.LogInformation(
                "{0} updated \"{1}\" setting of type {2} with value {3} for {4}.",
                CurrentUser,
                setting.Key,
                setting.ItemType.ToString(),
                setting.ItemType == ItemType.Boolean ? setting.ItemBool
                : setting.ItemType == ItemType.Integer ? setting.ItemInt
                : setting.ItemString,
                member);

            return Json(new DataSourceResult()
            {
                Data = new SettingsListItem[] { new SettingsListItem(setting) },
                Total = 1
            });
        }

        [HttpPost]
        [RequireMember]
        [Route("{memberId:int}/Settings/Create", Name = "AdminMemberCreateSetting")]
        public async Task<IActionResult> CreateSetting(Member member, [FromForm] SettingsListItem item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var setting = await SecurityService.AddMemberSettingAsync(
                member.MemberId,
                item.Name,
                item.Type == ItemType.Boolean.ToString() ? Convert.ToBoolean(item.Value) : null,
                item.Type == ItemType.Integer.ToString() ? Convert.ToInt32(item.Value) : null,
                item.Type == ItemType.String.ToString() ? item.Value : null,
                Enum.Parse<ItemType>(item.Type),
                item.Active);

            Logger.LogInformation(
                "{0} added \"{1}\" setting of type {2} with value {3} for {4}.",
                CurrentUser,
                setting.Key,
                setting.ItemType.ToString(),
                setting.ItemType == ItemType.Boolean ? setting.ItemBool
                : setting.ItemType == ItemType.Integer ? setting.ItemInt
                : setting.ItemString,
                member);

            return Json(new DataSourceResult()
            {
                Data = new SettingsListItem[] { new SettingsListItem(setting) },
                Total = 1
            });
        }

        [HttpPost]
        [Route("{memberId:int}/Settings/Delete", Name = "AdminMemberDeleteSetting")]
        public async Task<IActionResult> DeleteSetting(Member member, [FromForm] SettingsListItem item)
        {
            await SecurityService.DeleteMemberSettingAsync(item.SettingId);

            Logger.LogInformation(
                "{0} deleted \"{1}\" setting of type {2} with value {3} for {4}.",
                CurrentUser,
                item.Name,
                item.Type,
                item.Value,
                member);

            return Json(new DataSourceResult()
            {
                Data = new SettingsListItem[] { item },
                Total = 1
            });
        }
    }
}
