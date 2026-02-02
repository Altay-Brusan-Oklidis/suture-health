using Kendo.Mvc;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Areas.Identity.Models.Organization;
using SutureHealth.AspNetCore.Mvc.Attributes;
using SutureHealth.AspNetCore.Mvc.Extensions;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using Microsoft.EntityFrameworkCore;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("Organization")]
    public class OrganizationController : Controller
    {
        protected IApplicationService ApplicationService { get; set; }
        protected IIdentityService IdentityService { get; set; }

        public OrganizationController
        (
            IApplicationService applicationService,
            IIdentityService identityService
        )
        {
            ApplicationService = applicationService;
            IdentityService = identityService;
        }

        [HttpGet("", Name = "Organization")]
        public IActionResult Index(bool contentOnly = false)
        {
            return RedirectToRoute("OrganizationProfile", new { organizationId = CurrentUser.PrimaryOrganizationId, contentOnly });
        }

        [HttpGet("Users", Name = "OrganizationUsersIndex")] // This route catches the redirect from the legacy site
        [HttpGet("Members", Name = "OrganizationMembersIndex")]
        public IActionResult UsersIndex(bool contentOnly = false)
        {
            return RedirectToRoute("OrganizationUsers", new { organizationId = CurrentUser.PrimaryOrganizationId, contentOnly });
        }

        [HttpGet]
        [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
        [Route("Create", Name = "OrganizationCreate")]
        public async Task<IActionResult> Create()
        {
            return View("Profile", await InitializeProfileModel(null, null));
        }

        [HttpGet("{organizationId:int}", Name = "OrganizationProfile")]
        [RequireAuthorizedOrganization]
        public async Task<IActionResult> Profile(Organization organization, bool contentOnly = false)
        {
            return View(await InitializeProfileModel(null, organization, contentOnly));
        }

        protected async Task<ProfileModel> InitializeProfileModel(ProfileModel model = null, Organization organization = null, bool contentOnly = false)
        {
            model ??= organization == null ? new ProfileModel() : new ProfileModel()
            {
                ExternalName = organization.Name,
                InternalName = organization.OtherDesignation,
                Address1 = organization.AddressLine1,
                Address2 = organization.AddressLine2,
                City = organization.City,
                State = organization.StateOrProvince,
                ZipCode = organization.PostalCode,
                Phone = organization.Contacts.OrderByDescending(c => c.IsPrimary).FirstOrDefault(c => c.Type == ContactType.Phone)?.Value,
                Fax = organization.Contacts.OrderByDescending(c => c.IsPrimary).FirstOrDefault(c => c.Type == ContactType.Fax)?.Value,
                Npi = organization.NPI,
                MedicareCertificationNumber = organization.MedicareNumber,
                OrganizationTypeId = organization.OrganizationTypeId,
                ParentOrganizationId = organization.CompanyId,
                CloseDate = organization.ClosedAt,
                IsActive = organization.IsActive
            };

            model.RequireClientHeader = !contentOnly;
            model.CurrentUser = CurrentUser;
            model.OrganizationId = (organization?.OrganizationId).GetValueOrDefault();
            model.OrganizationTypes = CurrentUser.IsApplicationAdministrator() ?
                                            (await ApplicationService.GetOrganizationTypes()).Select(ot => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(ot.Name, ot.OrganizationTypeId.ToString())) :
                                            Array.Empty<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            model.IsEditing = organization != null;

            if (model.ParentOrganizationId.HasValue && CurrentUser.IsApplicationAdministrator())
            {
                var parentOrganization = organization?.OrganizationId == model.ParentOrganizationId.Value ?
                                            organization :
                                            await ApplicationService.GetOrganizationByIdAsync(model.ParentOrganizationId.Value);

                model.SelectedParentOrganization = new ProfileModel.OrganizationListItem()
                {
                    OrganizationId = model.ParentOrganizationId.Value,
                    Name = $"{parentOrganization.Name}{(!string.IsNullOrWhiteSpace(parentOrganization.OtherDesignation) && !string.Equals(parentOrganization.Name, parentOrganization.OtherDesignation, StringComparison.InvariantCultureIgnoreCase) ? $" ({parentOrganization.OtherDesignation})" : string.Empty)}"
                };
            }

            return model;
        }

        [HttpPost]
        [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
        [Route("DataSource", Name = "OrganizationParentOrganizationDataSource")]
        public async Task<IActionResult> ParentOrganizationDataSource([DataSourceRequest] DataSourceRequest request)
        {
            var search = request.Filters?.OfType<FilterDescriptor>().FirstOrDefault()?.Value.ToString() ?? string.Empty;
            var organizations = await ApplicationService.GetOrganizationsByName(search).ToArrayAsync();
            return Json(new DataSourceResult()
            {
                Total = organizations.Count(),
                Data = organizations.Select(o => new ProfileModel.OrganizationListItem()
                {
                    OrganizationId = o.OrganizationId,
                    Name = $"{o.Name}{(!string.IsNullOrWhiteSpace(o.OtherDesignation) && !string.Equals(o.Name, o.OtherDesignation, StringComparison.InvariantCultureIgnoreCase) ? $" ({o.OtherDesignation})" : string.Empty)}"
                })
            });
        }
        
        [HttpPost("{organizationId:int}", Name = "OrganizationSaveProfile")]
        [RequireAuthorizedOrganization]
        public async Task<IActionResult> SaveProfile(Organization organization, ProfileModel model, bool contentOnly = false)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", await InitializeProfileModel(model, organization, contentOnly));
            }

            var isAdmin = CurrentUser.IsApplicationAdministrator();

            await ApplicationService.UpdateOrganizationAsync(organization.OrganizationId, new Application.Organizations.UpdateOrganizationRequest
            {
                Name = model.ExternalName,
                OtherDesignation = model.InternalName,
                AddressLine1 = model.Address1,
                AddressLine2 = model.Address2,
                City = model.City,
                StateOrProvince = model.State,
                PostalCode = model.ZipCode,
                Phone = model.Phone,
                Fax = model.Fax,
                Npi = model.Npi,
                MedicareNumber = model.MedicareCertificationNumber ?? string.Empty,
                OrganizationTypeId = isAdmin && model.OrganizationTypeId.HasValue ? model.OrganizationTypeId.Value : null,
                CompanyIdSpecified = isAdmin,
                CompanyId = isAdmin && model.ParentOrganizationId.HasValue ? model.ParentOrganizationId.Value : null,
                ClosedAtSpecified = isAdmin,
                ClosedAt = model.CloseDate
            }, CurrentUser.Id);

            model.SaveAlert = new()
            {
                Text = "Changes saved successfully",
                FadeSeconds = 4,
                Style = AspNetCore.Models.AlertViewModel.ContextClass.Success
            };

            return isAdmin ?
                        RedirectToRoute("AdminOrganizationIndex", new { contentOnly }) :
                        View("Profile", await InitializeProfileModel(model, organization, contentOnly));
        }

        public static SemaphoreSlim saveOrganizationSemaphore = new(1);

        [HttpPost]
        [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
        [Route("Create", Name = "OrganizationSaveNewOrganization")]
        public async Task<IActionResult> SaveNewOrganization([FromForm] ProfileModel model)
        {
            await saveOrganizationSemaphore.WaitAsync();

            try
            {                
                long.TryParse(model.Npi, out var npiNumber);
                var orgWithCertainNPI = await ApplicationService.GetOrganizationByNPIAsync(npiNumber);

                if (orgWithCertainNPI != null)
                {
                    ModelState.AddModelError("NPI", "This NPI is already existing.");
                }

                if (!ModelState.IsValid)
                {
                    return View("Profile", await InitializeProfileModel(model, null));
                }

                var isAdmin = CurrentUser.IsApplicationAdministrator();

                if (orgWithCertainNPI == null)
                {
                    await ApplicationService.CreateOrganizationAsync(new Application.Organizations.CreateOrganizationRequest
                    {
                        Name = model.ExternalName,
                        OtherDesignation = model.InternalName,
                        AddressLine1 = model.Address1,
                        AddressLine2 = model.Address2,
                        City = model.City,
                        StateOrProvince = model.State,
                        PostalCode = model.ZipCode,
                        Phone = model.Phone,
                        Fax = model.Fax,
                        Npi = model.Npi,
                        MedicareNumber = model.MedicareCertificationNumber ?? string.Empty,
                        OrganizationTypeId = isAdmin && model.OrganizationTypeId.HasValue ? model.OrganizationTypeId.Value : null,
                        CompanyId = isAdmin && model.ParentOrganizationId.HasValue ? model.ParentOrganizationId.Value : null
                    }, CurrentUser.Id);
                }
                var result = Redirect(isAdmin ? Url.RouteUrl("AdminOrganizationIndex") : Url.DefaultLandingPage(CurrentUser));
                return result;
            }
            finally { saveOrganizationSemaphore.Release(); }
        }

        [HttpGet("{organizationId:int}/Members", Name = "OrganizationUsers")]
        [RequireAuthorizedOrganization]
        public async Task<IActionResult> Users(Organization organization, [FromQuery] bool newUser, bool contentOnly = false)
        {
            var administrators = await ApplicationService.GetOrganizationMembersByOrganizationId(organization.OrganizationId)
                                                         .Where(om => om.IsAdministrator && om.IsActive)
                                                         .Select(om => om.MemberId)
                                                         .ToArrayAsync();
            return View(new UsersViewModel()
            {
                CurrentUser = CurrentUser,
                RequireKendo = true,
                OrganizationId = organization.OrganizationId,
                IsAdministrator = administrators.Contains(CurrentUser.MemberId),
                OrganizationHasAdministrator = administrators.Any(),
                NewUserAddedAlert = newUser ?
                                        new()
                                        {
                                            Text = "New user added successfully",
                                            FadeSeconds = 4,
                                            Style = AspNetCore.Models.AlertViewModel.ContextClass.Success
                                        } :
                                        null,
                RequireClientHeader = !contentOnly
            });
        }

        [HttpPost("{organizationId:int}/Members", Name = "OrganizationUsersGrid")]
        [RequireAuthorizedOrganization]
        public async Task<IActionResult> UsersGrid(Organization organization)
        {
            var organizationMembers = IdentityService.GetOrganizationMembersByOrganizationId(organization.OrganizationId)
                                                        .Where(om => IdentityService.GetOrganizationMembersByOrganizationId(organization.OrganizationId).Where(om => om.IsAdministrator && om.IsActive && om.MemberId == CurrentUser.MemberId).Any() ||
                                                                        (om.IsActive && om.IsAdministrator && om.Member.IsActive));
            var memberDetails = IdentityService.GetMemberIdentities()
                                                        .Where(om => !IdentityService.GetMemberSettings().Where(s => s.ParentId == om.MemberId && s.Key == "HideFromUserManagement" && s.IsActive == true && s.ItemBool == true).Any())
                                                        .Join(organizationMembers, mi => mi.MemberId, om => om.MemberId, (mi, om) => new { OrganizationMember = om, Member = om.Member, MemberIdentity = mi });

            var gridItems = (await memberDetails.ToArrayAsync()).Select(md => new UserListItem
            {
                UserId = md.Member.MemberId,
                DisplayName = $"{md.Member.LastName}, {md.Member.FirstName}" +
                                                                (!string.IsNullOrWhiteSpace(md.Member.Suffix) ? $" {md.Member.Suffix}" : string.Empty) +
                                                                (!string.IsNullOrWhiteSpace(md.Member.ProfessionalSuffix) ? $" {md.Member.ProfessionalSuffix}" : string.Empty),
                UserName = md.Member.UserName,
                Email = md.Member.Contacts.FirstOrDefault(c => c.Type == ContactType.Email)?.Value,
                Active = md.OrganizationMember.IsActive,
                Admin = md.OrganizationMember.IsAdministrator,
                Locked = md.MemberIdentity.LockoutEnd.HasValue && md.MemberIdentity.LockoutEnd.Value > DateTimeOffset.UtcNow,
                Registered = !md.MemberIdentity.MustRegisterAccount,
                DateDeactivatedDisplay = md.OrganizationMember.DeactivatedAt?.ToShortDateString() ?? string.Empty,
                DateDeactivated = md.OrganizationMember.DeactivatedAt,
                CreatedDateDisplay = md.OrganizationMember.CreatedAt.GetValueOrDefault().ToShortDateString(),
                CreatedDate = md.OrganizationMember.CreatedAt,
                LastLoginDate = md.Member.LastLoggedInAt,
                EditUserUrl = Url.RouteUrl("OrganizationMembersEdit", new { organizationId = organization.OrganizationId, memberId = md.Member.MemberId }),
                ChangePasswordModalUrl = Url.RouteUrl("MemberResetPasswordModal", new { memberId = md.Member.MemberId }),
                UnlockAccountModalUrl = Url.RouteUrl("MemberUnlockAccountModal", new { memberId = md.Member.MemberId }),
                SendRegistrationEmailModalUrl = Url.RouteUrl("MemberSendRegistrationEmailModal", new { memberId = md.Member.MemberId }),
                Role = md.Member.RoleDescription(),
                Profession = md.Member.MemberType.GetEnumDescription()
            });

            return Json(new DataSourceResult()
            {
                Data = gridItems,
                Total = gridItems.Count()
            });
        }

        [HttpPost("IsNpiAvailable", Name = "OrganizationIsNpiAvailable")]
        public async Task<IActionResult> IsNpiAvailable(string npi, int? organizationId = null)
        {
            var organization = string.IsNullOrWhiteSpace(npi) ? null : await ApplicationService.GetOrganizations().Where(o => o.NPI == npi).FirstOrDefaultAsync();

            return Json(organization == null || (organizationId.HasValue && organization.OrganizationId == organizationId.Value));
        }

        [HttpPost("IsMedicareNumberAvailable", Name = "OrganizationIsMedicareNumberAvailable")]
        public async Task<IActionResult> IsMedicareNumberAvailable(string medicareCertificationNumber, int? organizationId = null)
        {
            var organization = string.IsNullOrWhiteSpace(medicareCertificationNumber) ? null : await ApplicationService.GetOrganizations().Where(o => o.MedicareNumber == medicareCertificationNumber).FirstOrDefaultAsync();

            return Json(organization == null || (organizationId.HasValue && organization.OrganizationId == organizationId.Value));
        }
    }
}
