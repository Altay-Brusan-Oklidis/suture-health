using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SutureHealth.AspNetCore.Areas.Identity.Models.Member;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.AspNetCore.Mvc.Attributes;
using SutureHealth.AspNetCore.Mvc.Rendering;
using SutureHealth.AspNetCore.Mvc.Extensions;
using SutureHealth.Requests.Services;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.Reporting.Services;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;
using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Route("Member")]
    public class MemberController : Controller
    {
        private const string MEMBER_KEY_UNSUBSCRIBE_REASON = "CommunicationPreferences.UnsubscribeReason";

        protected IApplicationService ApplicationService { get; }
        protected IRequestServicesProvider RequestService { get; }
        protected SutureUserManager SutureUserManager { get; }
        protected ISchedulingService ScheduleService { get; }
        protected IIdentityService IdentityService { get; }
        protected ILogger<MemberController> Logger { get; }

        public MemberController
        (
            IApplicationService applicationService,
            IRequestServicesProvider requestService,
            SutureUserManager sutureUserManager,
            ISchedulingService scheduleService,
            IIdentityService identityService,
            ILogger<MemberController> logger
        )
        {
            ApplicationService = applicationService;
            RequestService = requestService;
            SutureUserManager = sutureUserManager;
            ScheduleService = scheduleService;
            IdentityService = identityService;
            Logger = logger;
        }

        [HttpGet("Account", Name = "MemberAccountIndex")]
        public IActionResult AccountIndex(bool contentOnly)
        {
            return RedirectToRoute("MemberAccount", new { memberId = CurrentUser.Id, contentOnly = contentOnly });
        }

        [HttpGet("{memberId:int}/Account", Name = "MemberAccount")]
        [RequireMember(RequireAuthenticatedMember = true)]
        public async Task<IActionResult> Account
        (
            Member member,
            [FromQuery] string returnUrl = null,
            [FromQuery] bool mobileConfirmed = false,
            bool contentOnly = false
        )
        {
            var emailAddress = member.Contacts?.GetPrimaryEmailAddress();
            var mobileNumber = member.Contacts?.GetPrimaryMobileNumber();

            bool isExternalUser = false;
            var externalUser = await ApplicationService.GetMemberSettings(member.MemberId)
                                           .Where(s => s.Key == "isExternalUser" && s.IsActive == true)
                                           .FirstOrDefaultAsync();
            if (externalUser != null)
            {
                isExternalUser = true;
            }
            var user = new AccountModel()
            {
                FirstName = member.FirstName,
                MiddleName = member.MiddleName,
                LastName = member.LastName,
                UserName = member.UserName,
                Suffix = member.Suffix,
                ProfessionalSuffix = member.ProfessionalSuffix,
                MobileNumber = mobileNumber?.Value,
                Email = emailAddress?.Value,
                OfficePhone = member.Contacts.Where(c => c.Type == ContactType.OfficePhone && c.IsActive).OrderByDescending(c => c.IsPrimary).FirstOrDefault()?.Value,
                OfficePhoneExtension = member.Contacts.Where(c => c.Type == ContactType.OfficePhoneExt && c.IsActive).OrderByDescending(c => c.IsPrimary).FirstOrDefault()?.Value,
                Npi = member.NPI,
                IsNpiRequired = (new MemberType[] { MemberType.Physician, MemberType.NursePractitioner, MemberType.PhysicianAssistant }).Contains(member.MemberType),
                CanSetSigningName = member.MemberType == MemberType.Physician || member.CanSign,
                SigningName = member.SigningName,
                CurrentUser = CurrentUser,
                SendConfirmationEmailActionUrl = Url.RouteUrl("MemberSendConfirmationEmail", new { memberId = member.MemberId }),
                SendMobileNumberConfirmationActionUrl = Url.RouteUrl("MemberSendMobileNumberConfirmation", new { memberId = member.MemberId }),
                ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.DefaultLandingPage(CurrentUser),
                CommunicationPreferences = await GetCommunicationPreferencesByMemberId(member),
                IsExternalUser = isExternalUser,
                RequireClientHeader = !contentOnly
            };


            await AppendAccountAlerts(user, member, mobileConfirmed ? AccountModel.PostSaveType.MobileVerification : AccountModel.PostSaveType.None);

            return View(user);
        }

        protected async Task AppendAccountAlerts(AccountModel model, Member member, AccountModel.PostSaveType postSave = AccountModel.PostSaveType.None)
        {
            var emailAddress = member.Contacts?.GetPrimaryEmailAddress();
            var mobileNumber = member.Contacts?.GetPrimaryMobileNumber();
            var confirmationAudits = (emailAddress?.IsConfirmed ?? false) && (mobileNumber == null || mobileNumber.IsConfirmed) ?
                                        Array.Empty<MemberAuditEvent>() :
                                        await IdentityService.GetAuditEvents(member.MemberId)
                                                             .Where(e => (e.AuditEventId == AuditEvents.EmailConfirmed || e.AuditEventId == AuditEvents.PhoneConfirmed) && !e.Succeeded)
                                                             .ToArrayAsync();

            model.ConfirmEmailAlert = emailAddress != null && !emailAddress.IsConfirmed && confirmationAudits.Any(e => e.AuditEventId == AuditEvents.EmailConfirmed) ?
                                            new AspNetCore.Models.AlertViewModel()
                                            {
                                                Text = "You should have received an email from SutureHealth to complete the verification process. If you did not receive it, you can check your spam filter or resend it.",
                                                CanClose = true,
                                                Style = AspNetCore.Models.AlertViewModel.ContextClass.Warning,
                                                Link = new AspNetCore.Models.AlertViewModel.AlertLink()
                                                {
                                                    Id = "ReSendEmailConfirmationAlertLink",
                                                    Text = "Re-Send Email",
                                                    NavigationUrl = "javascript:void(0);"
                                                }
                                            } :
                                            null;

            if (!(emailAddress?.IsConfirmed ?? false) || (mobileNumber != null && !mobileNumber.IsConfirmed))
            {
                model.ConfirmationWizard = new()
                {
                    VerifyMobileNumberCodeActionUrl = Url.RouteUrl("MemberVerifyMobileNumberCode", new { memberId = member.MemberId })
                };

                if (!(emailAddress?.IsConfirmed ?? false))
                {
                    model.ConfirmationWizard.EmailConfirmation = new()
                    {
                        Email = emailAddress?.Value
                    };
                }

                if (!(mobileNumber?.IsConfirmed ?? true))
                {
                    model.ConfirmationWizard.PhoneConfirmation = new()
                    {
                        MobileNumber = long.Parse(mobileNumber.Value).ToString("###-###-####"),
                        MobileConfirmationReturnUrl = System.Net.WebUtility.UrlDecode(Url.RouteUrl("MemberAccount", new { memberId = member.MemberId, returnUrl = model.ReturnUrl, mobileConfirmed = true }))
                    };
                }
            }

            if (postSave != AccountModel.PostSaveType.None)
            {
                model.SaveAlert = new AspNetCore.Models.AlertViewModel()
                {
                    Text = postSave switch
                    {
                        AccountModel.PostSaveType.MobileVerification => "Verification successful",
                        _ => "Changes saved successfully"
                    },
                    FadeSeconds = 4,
                    Style = AspNetCore.Models.AlertViewModel.ContextClass.Success
                };
            }
        }

        [HttpPost("IsUserNameAvailable", Name = "MemberIsUserNameAvailable")]
        public async Task<IActionResult> IsUserNameAvailable(string userName, int? memberId = null)
        {
            var user = string.IsNullOrWhiteSpace(userName) ? null : await ApplicationService.GetMemberByNameAsync(userName.Trim());

            return Json(user == null || (memberId.HasValue && user.MemberId == memberId.Value));
        }

        [HttpPost("IsNpiAvailable", Name = "MemberIsNpiAvailable")]
        public async Task<IActionResult> IsNpiAvailable(string npi, int? memberId = null)
        {
            var user = long.TryParse(npi, out var npiValue) ? await ApplicationService.GetMemberByNPIAsync(npiValue) : null;

            return Json(user == null || (memberId.HasValue && user.MemberId == memberId.Value));
        }

        [HttpGet("Create", Name = "MemberCreate")]
        [HttpGet("/Organization/{organizationId:int}/Members/Create", Name = "OrganizationMembersCreate")]
        public async Task<IActionResult> Create([FromQuery] string returnUrl = null, int? organizationId = null)
        {
            var model = await InitializeEditModelAsync(null, null, organizationId);
            model.CancelUrl = GetEditReturnUrl(returnUrl, organizationId);
            return View("Edit", model);
        }

        [HttpGet("{memberId:int}", Name = "MemberEdit")]
        [HttpGet("/Organization/{organizationId:int}/Members/{memberId:int}", Name = "OrganizationMembersEdit")]
        [RequireMember(RequireAuthorizedAdministrator = true)]
        public async Task<IActionResult> Edit(Member member, [FromQuery] string returnUrl = null, int? organizationId = null)
        {
            var model = await InitializeEditModelAsync(member, null, organizationId);
            model.CancelUrl = GetEditReturnUrl(returnUrl, organizationId);
            return View(model);
        }

        [HttpPost("Relationships", Name = "MemberRelationshipsNew")]
        [HttpPost("{memberId:int}/Relationships", Name = "MemberRelationships")]
        public async Task<IActionResult> Relationships([FromForm] RelationshipsRequest request, int? memberId = null)
        {
            var members = null as IEnumerable<Member>;
            var activeRelationships = Array.Empty<int>() as IEnumerable<int>;

            if (!(request?.Organizations?.Any() ?? false) || !Enum.TryParse<MemberType>(request?.MemberTypeId.ToString(), out var memberType) || (new MemberType[] { MemberType.Unknown, MemberType.Staff }).Contains(memberType))
            {
                return PartialView("_Relationships", new RelationshipsModel()
                {
                    Relationships = Array.Empty<RelationshipsModel.Relationship>()
                });
            }

            if (!CurrentUser.IsApplicationAdministrator())
            {
                var organizationMembers = await ApplicationService.GetOrganizationMembersByMemberId(CurrentUser.Id).ToArrayAsync();
                if (request.Organizations.Count() > request.Organizations.Join(organizationMembers.Where(om => om.IsAdministrator), oid => oid, o => o.OrganizationId, (oid, o) => oid).Count())
                {
                    return StatusCode(401);
                }
            }

            members = (await ApplicationService.GetOrganizationMembersByOrganizationId(request.Organizations.ToArray())
                                                  .Where(om => om.IsActive && om.Member.IsActive && om.MemberId != memberId)
                                                  .ToArrayAsync())
                                                  .GroupBy(om => om.MemberId, (id, grp) => grp.First().Member);
            switch ((MemberType)request.MemberTypeId)
            {
                case MemberType.Physician:
                    members = members.Join(MemberTypeExtensions.SignerTypes.Except(new MemberType[] { MemberType.Physician }), m => m.MemberTypeId, mt => (int)mt, (m, mt) => m);
                    break;
                case MemberType.NursePractitioner:
                case MemberType.PhysicianAssistant:
                    members = members.Join(MemberTypeExtensions.SignerTypes, m => m.MemberTypeId, mt => (int)mt, (m, mt) => m);
                    break;
                default:
                    members = members.Join(MemberTypeExtensions.SignerTypes, m => m.MemberTypeId, mt => (int)mt, (m, mt) => m)
                                     .Where(m => m.MemberType == MemberType.Physician || m.CanSign);
                    break;
            }

            if (memberId.HasValue)
            {
                var member = await ApplicationService.GetMemberByIdAsync(memberId.Value);
                var supervisors = ApplicationService.GetSupervisorsForMemberId(memberId.Value);
                var subordinates = ApplicationService.GetSubordinatesForMemberId(memberId.Value);
                activeRelationships = await supervisors.Where(m => m.IsActive == true)
                                                       .Select(m => m.SupervisorMemberId)
                                                       .Union(subordinates.Where(m => m.IsActive == true).Select(m => m.SubordinateMemberId))
                                                       .ToArrayAsync();
            }

            return PartialView("_Relationships", new RelationshipsModel()
            {
                Relationships = members.GroupJoin(activeRelationships, m => m.MemberId, id => id, (m, id) => new { Member = m, RelationshipActive = id.Any() })
                                       .Select(r => new RelationshipsModel.Relationship()
                                       {
                                           MemberId = r.Member.MemberId,
                                           Active = r.RelationshipActive,
                                           Name = $"{r.Member.LastName}, {r.Member.FirstName} {r.Member.Suffix} {r.Member.ProfessionalSuffix}".Trim(),
                                           Occupation = r.Member.MemberType.GetEnumDescription(),
                                           Role = r.Member.RoleDescription()
                                       })
                                       .OrderBy(r => r.Name)
                                       .ToArray()
            });
        }

        [HttpPost("{memberId:int}/Account", Name = "MemberSaveAccount")]
        [RequireMember(RequireAuthenticatedMember = true)]
        public async Task<IActionResult> SaveAccount
        (
            Member member,
            AccountModel user,
            [FromServices] SignInManager<MemberIdentity> signInManager,
            [FromServices] IServiceScopeFactory scopeFactory,
            bool contentOnly = false
        )
        {
            bool isExternalUser = false;
            var externalUser = await ApplicationService.GetMemberSettings(member.MemberId)
                                           .Where(s => s.Key == "isExternalUser" && s.IsActive == true)
                                           .FirstOrDefaultAsync();
            if (externalUser != null)
            {
                isExternalUser = true;
            }

            var notificationSelections = user.CommunicationPreferences?.Selections ?? Array.Empty<CommunicationPreferencesSelection>();
            var mobileNumber = member.Contacts?.GetPrimaryMobileNumber();

            user.CurrentUser = CurrentUser;
            user.SendConfirmationEmailActionUrl = Url.RouteUrl("MemberSendConfirmationEmail", new { memberId = member.MemberId });
            user.SendMobileNumberConfirmationActionUrl = Url.RouteUrl("MemberSendMobileNumberConfirmation", new { memberId = member.MemberId });
            user.CommunicationPreferences = await GetCommunicationPreferencesByMemberId(member);
            user.CommunicationPreferences.Selections = notificationSelections;
            user.IsNpiRequired = (new MemberType[] { MemberType.Physician, MemberType.NursePractitioner, MemberType.PhysicianAssistant }).Contains(member.MemberType);
            user.CanSetSigningName = member.MemberType == MemberType.Physician || member.CanSign;
            user.ReturnUrl = Url.IsLocalUrl(user.ReturnUrl) ? user.ReturnUrl : Url.DefaultLandingPage(CurrentUser);
            user.RequireClientHeader = !contentOnly;

            // #2441: As of writing, we don't know why the model state dictionary contains validations for the Npi property beyond the attributes specified in the model.  Remove all Npi validations
            //        and manually validate until a better solution is found.
            ModelState.Remove("Npi");
            if (!string.IsNullOrWhiteSpace(user.Npi))
            {
                if (user.Npi.Trim().Length != 10 || !long.TryParse(user.Npi, out _))
                {
                    ModelState.AddModelError("Npi", "INVALID");
                }
                else if (user.Npi != member.NPI && ((await ApplicationService.GetMemberByNPIAsync(long.Parse(user.Npi))) is Member))
                {
                    ModelState.AddModelError("Npi", "NPI IS TAKEN");
                }
            }
            if (!string.IsNullOrWhiteSpace(user.UserName))
            {
                if (isExternalUser && user.UserName != user.CurrentUser.UserName)
                {
                    ModelState.AddModelError("Username", "Cannot Change Username of External Users");
                }
            }
            if ((new MemberType[] { MemberType.Physician, MemberType.NursePractitioner, MemberType.PhysicianAssistant }).Contains(member.MemberType) && string.IsNullOrWhiteSpace(user?.Npi))
            {
                ModelState.AddModelError("Npi", "REQUIRED");
            }
            if (!(member.MemberType == MemberType.Physician || member.CanSign))
            {
                ModelState.Remove("SigningName");
            }

            if (!ModelState.IsValid)
            {
                return View("Account", user);
            }

            await SaveCommunicationPreferencesByMemberId(member.MemberId, notificationSelections);
            await ApplicationService.UpdateMemberAsync(member.MemberId, new Application.Members.UpdateMemberRequest()
            {
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Suffix = user.Suffix ?? string.Empty,
                ProfessionalSuffix = user.ProfessionalSuffix ?? string.Empty,
                Npi = user.Npi ?? string.Empty,
                SigningName = member.MemberType == MemberType.Physician || member.CanSign ? (user.SigningName ?? string.Empty) : null,
                UserName = user.UserName,
                Email = user.Email,
                MobilePhone = user.MobileNumber ?? string.Empty,
                OfficePhone = user.OfficePhone ?? string.Empty,
                OfficePhoneExtension = user.OfficePhoneExtension ?? string.Empty
            }, CurrentUser.Id);
            await signInManager.RefreshSignInAsync(CurrentUser);

            // If a phone number was provided and it's different from what we had, send a text.
            if (!string.IsNullOrWhiteSpace(user.MobileNumber) && (!string.Equals(mobileNumber?.Value, user.MobileNumber)))
            {
                var memberIdentity = await SutureUserManager.FindByIdAsync(member.MemberId.ToString());

                memberIdentity.MobileNumber = user.MobileNumber;    // The identity fetched above will be a cached older copy if it is the CurrentUser.
                await SutureUserManager.SendMobileNumberConfirmationAsync(memberIdentity);
            }

            // Prevents cache issues upon save
            using (var serviceScope = scopeFactory.CreateScope())
            {
                var memberService = serviceScope.ServiceProvider.GetRequiredService<IMemberService>();

                member = await memberService.GetMemberByIdAsync(member.MemberId);
                await AppendAccountAlerts(user, member, AccountModel.PostSaveType.Page);
            }

            return View("Account", user);
        }

        [HttpPost("Create", Name = "MemberSaveNewMember")]
        [HttpPost("/Organization/{organizationId:int}/Members/Create", Name = "OrganizationMembersSaveNewMember")]
        public async Task<IActionResult> SaveNewMember
        (
            [FromForm] EditModel model,
            [FromQuery] string returnUrl = null,
            int? organizationId = null
        )
        {
            model = await InitializeEditModelAsync(null, model, organizationId);

            var org = ApplicationService.GetOrganizationByIdAsync((int)model.DefaultOrganizationId).GetAwaiter().GetResult();
            if (org.IsFree && model.MemberTypeId == 2003)
            {
                int maxAllowedUser = 0;
                var communityUsersOrgSetting = ApplicationService.GetOrganizationSettings()
                    .FirstOrDefault(orgSetting => orgSetting.ParentId == org.OrganizationId && orgSetting.Key == "CommunityUsers");

                if (communityUsersOrgSetting != null &&
                    communityUsersOrgSetting.ItemInt != null &&
                    communityUsersOrgSetting.ItemInt != default)
                {
                    maxAllowedUser = (int)communityUsersOrgSetting.ItemInt;
                }
                else
                {
                    maxAllowedUser = (int)ApplicationService.GetApplicationSettings().FirstOrDefault(appsetting => appsetting.Key == "CommunityUsers").ItemInt;

                }
                var orgSenderMembers = ApplicationService.GetMembersByOrganizationId(org.OrganizationId).Where(m => m.MemberTypeId == 2003);
                if (orgSenderMembers.Count() >= maxAllowedUser)
                {
                    model.IsUserLimitReached = true;
                    model.MaxAllowedCoummunityUsers = maxAllowedUser;
                    return View("Edit", model);
                }
            }

            ValidateEditModel(model, true);
            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            var authorizedOrganizationIds = CurrentUser.IsApplicationAdministrator() && organizationId.HasValue ?
                                            new int[] { organizationId.Value } :
                                            await ApplicationService.GetOrganizationMembersByMemberId(CurrentUser.Id)
                                                                 .Where(om => om.IsAdministrator)
                                                                 .Select(om => om.OrganizationId)
                                                                 .ToArrayAsync();

            var member = new MemberIdentity
            {
                CanSign = (new MemberType[] { MemberType.NursePractitioner, MemberType.PhysicianAssistant }).Cast<int>().Contains(model.MemberTypeId.GetValueOrDefault()) ? model.CanSignDocuments : model.MemberTypeId == (int)MemberType.Physician,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                MemberTypeId = model.MemberTypeId.Value,
                MobileNumber = model.MobileNumber ?? string.Empty,
                OfficeNumber = model.OfficePhone ?? string.Empty,
                OfficeExtension = string.IsNullOrWhiteSpace(model.OfficePhone) ? string.Empty : model.OfficePhoneExtension ?? string.Empty,
                NPI = model.Npi ?? string.Empty,
                ProfessionalSuffix = model.ProfessionalSuffix ?? string.Empty,
                Suffix = model.Suffix ?? string.Empty,
                SigningName = model.SigningName ?? string.Empty,
                UserName = model.UserName,
            };
            var relatedMembers = (model.Relationships ?? Array.Empty<EditModel.Relationship>()).Where(r => r.Active).Select(r => r.MemberId);
            var organizationMembers = (model.Offices ?? Array.Empty<EditModel.Office>()).Join(authorizedOrganizationIds, o => o.OrganizationId, id => id, (o, id) => new OrganizationMember
            {
                OrganizationId = o.OrganizationId,
                IsActive = o.Active,
                IsAdministrator = o.UserManagement,
                IsBillingAdministrator = o.BillingAdministrator,
                IsPrimary = o.OrganizationId == model.DefaultOrganizationId
            });

            await SutureUserManager.CreateAsync(member, CurrentUser, organizationId.Value, relatedMembers, organizationMembers);

            var setting = await ApplicationService.AddMemberSettingAsync(member.MemberId, AuthorizationPolicies.ShowPatientsInfo, model.ShowPatientsInfo, null, null, ItemType.Boolean);

            Logger.LogInformation(
                "{0} added new {1} with \"{2}\" authorization setting with value {3}.",
                CurrentUser,
                member,
                AuthorizationPolicies.ShowPatientsInfo,
                setting.ItemBool);

            return Redirect(GetEditReturnUrl(returnUrl, organizationId, true));
        }

        [HttpPost("{memberId:int}", Name = "MemberSaveMember")]
        [HttpPost("/Organization/{organizationId:int}/Members/{memberId:int}", Name = "OrganizationMembersSaveMember")]
        [RequireMember(RequireAuthorizedAdministrator = true)]
        public async Task<IActionResult> SaveMember
        (
            Member member,
            [FromForm] EditModel model,
            [FromQuery] string returnUrl = null,
            int? organizationId = null
        )
        {
            model = await InitializeEditModelAsync(member, model, organizationId);
            ValidateEditModel(model, false);
            if (model.HasPendingDocuments)
            {
                ModelState.Remove("MemberTypeId");
                ModelState.Remove("CanSignDocuments");
                model.MemberTypeId = member.MemberTypeId;
                model.CanSignDocuments = member.CanSign;
            }
            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            await ApplicationService.UpdateMemberAsync(member.MemberId, new Application.Members.UpdateMemberRequest()
            {
                CanSign = (new MemberType[] { MemberType.NursePractitioner, MemberType.PhysicianAssistant }).Cast<int>().Contains(model.MemberTypeId.GetValueOrDefault()) ? model.CanSignDocuments : model.MemberTypeId == (int)MemberType.Physician,
                Email = model.Email,
                FirstName = model.FirstName ?? string.Empty,
                LastName = model.LastName ?? string.Empty,
                MemberTypeId = model.MemberTypeId,
                MobilePhone = model.MobileNumber ?? string.Empty,
                OfficePhone = model.OfficePhone ?? string.Empty,
                OfficePhoneExtension = string.IsNullOrWhiteSpace(model.OfficePhone) ? string.Empty : model.OfficePhoneExtension ?? string.Empty,
                Npi = model.Npi ?? string.Empty,
                ProfessionalSuffix = model.ProfessionalSuffix ?? string.Empty,
                Suffix = model.Suffix ?? string.Empty,
                SigningName = model.SigningName ?? string.Empty,
                UserName = model.UserName,
                OrganizationMembers = (model.Offices ?? Array.Empty<EditModel.Office>())
                                        .Join(CurrentUser.IsApplicationAdministrator() && model.Offices != null ?
                                            model.Offices.Select(o => o.OrganizationId) :
                                            await ApplicationService.GetOrganizationMembersByMemberId(CurrentUser.Id)
                                                                 .Where(om => om.IsAdministrator)
                                                                 .Select(om => om.OrganizationId)
                                                                 .ToArrayAsync(), o => o.OrganizationId, id => id, (o, id) => new Application.Members.UpdateMemberRequest.OrganizationMember()
                                                                 {
                                                                     OrganizationId = o.OrganizationId,
                                                                     IsActive = o.Active || o.HasPendingDocuments,
                                                                     IsAdministrator = o.UserManagement,
                                                                     IsBillingAdministrator = o.BillingAdministrator,
                                                                     IsPrimary = o.OrganizationId == model.DefaultOrganizationId
                                                                 }),
                RelatedMemberIds = (model.Relationships ?? Array.Empty<EditModel.Relationship>()).Where(r => r.Active).Select(r => r.MemberId)
            }, CurrentUser.Id);

            var showPatientsInfoSetting = await ApplicationService
                .GetMemberSettings(member.MemberId).Where(ms =>
                    ms.Key == AuthorizationPolicies.ShowPatientsInfo)
                .FirstOrDefaultAsync();

            if (showPatientsInfoSetting is null)
            {
                var setting = await ApplicationService.AddMemberSettingAsync(
                    member.MemberId,
                    AuthorizationPolicies.ShowPatientsInfo,
                    model.ShowPatientsInfo,
                    null,
                    null,
                    ItemType.Boolean);

                Logger.LogInformation(
                    "{0} added \"{1}\" authorization setting for {2} with value {3}.",
                    CurrentUser,
                    AuthorizationPolicies.ShowPatientsInfo,
                    member,
                    setting.ItemBool);
            }
            else
            {
                var setting = await ApplicationService.SetMemberSettingAsync(
                    showPatientsInfoSetting.SettingId,
                    AuthorizationPolicies.ShowPatientsInfo,
                    model.ShowPatientsInfo,
                    null,
                    null,
                    ItemType.Boolean);

                Logger.LogInformation(
                    "{0} changed \"{1}\" authorization setting for {2} to {3}.",
                    CurrentUser,
                    AuthorizationPolicies.ShowPatientsInfo,
                    member,
                    setting.ItemBool);
            }

            return Redirect(GetEditReturnUrl(returnUrl, organizationId));
        }

        [HttpGet("{memberId:int}/ResetPassword", Name = "MemberResetPasswordModal")]
        [RequireMember(RequireAuthorizedAdministrator = true)]
        public IActionResult ResetPasswordModal(MemberIdentity member)
        {
            return PartialView("_ResetPasswordModal", new ResetPasswordModel()
            {
                UserName = member.UserName,
                ResetPasswordActionUrl = Url.RouteUrl("MemberResetPassword", new { memberId = member.MemberId }),
                IsRegistrationRequired = member.MustRegisterAccount
            });
        }

        [HttpPost("{memberId:int}/ResetPassword", Name = "MemberResetPassword")]
        [RequireMember(RequireAuthorizedAdministrator = true)]
        public async Task<IActionResult> ResetPassword(MemberIdentity member)
        {
            if (member.MustRegisterAccount)
            {
                return BadRequest();
            }

            await SutureUserManager.ResetPasswordAsync(member);
            return Ok();
        }

        [HttpGet("ChangePassword", Name = "MemberChangePasswordModal")]
        public IActionResult ChangePasswordModal()
        {
            return PartialView("_ChangePasswordModal", new ChangePasswordModel());
        }

        [HttpPost("ChangePassword", Name = "MemberChangePassword")]
        public async Task<IActionResult> ChangePassword([FromServices] SignInManager<MemberIdentity> signInManager, [FromForm] ChangePasswordModel model)
        {
            IdentityResult result = new();

            if (!ModelState.IsValid)
            {
                return PartialView("_ChangePasswordModal", model);
            }

            result = await SutureUserManager.ChangePasswordAsync(CurrentUser, model.OldPassword, model.Password);
            if (result.Succeeded)
            {
                await signInManager.RefreshSignInAsync(CurrentUser);

                return Ok();
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return PartialView("_ChangePasswordModal", model);
            }
        }

        [HttpGet]
        [RequireMember(RequireAuthorizedAdministrator = true)]
        [Route("{memberId:int}/Unlock", Name = "MemberUnlockAccountModal")]
        public IActionResult UnlockAccountModal(MemberIdentity member)
            => PartialView("_UnlockAccountModal", new UnlockAccountModel()
            {
                UserName = member.UserName,
                UnlockAccountActionUrl = Url.RouteUrl("MemberUnlock", new { memberId = member.MemberId })
            });

        [HttpPost]
        [RequireMember(RequireAuthorizedAdministrator = true)]
        [Route("{memberId:int}/Unlock", Name = "MemberUnlock")]
        public async Task<IActionResult> Unlock(MemberIdentity member)
        {
            await SutureUserManager.SetLockoutEndDateAsync(member, null);
            return Ok();
        }

        [HttpGet]
        [RequireMember(RequireAuthorizedAdministrator = true)]
        [Route("{memberId:int}/SendRegistrationEmail", Name = "MemberSendRegistrationEmailModal")]
        public IActionResult SendRegistrationEmailModal(MemberIdentity member)
            => PartialView("_SendRegistrationEmailModal", new SendRegistrationEmailModel()
            {
                UserName = member.UserName,
                IsRegistered = !member.MustRegisterAccount,
                SendRegistrationEmailActionUrl = Url.RouteUrl("MemberSendRegistrationEmail", new { memberId = member.MemberId })
            });

        [HttpPost]
        [RequireMember(RequireAuthorizedAdministrator = true)]
        [Route("{memberId:int}/SendRegistrationEmail", Name = "MemberSendRegistrationEmail")]
        public async Task<IActionResult> SendRegistrationEmail(MemberIdentity member)
        {
            if (!member.MustRegisterAccount)
            {
                return BadRequest();
            }

            await SutureUserManager.SendRegistrationConfirmationAsync(member);

            return Ok();
        }

        [HttpPost]
        [RequireMember(RequireAuthenticatedMember = true)]
        [Route("{memberId:int}/Account/SendConfirmationEmail", Name = "MemberSendConfirmationEmail")]
        public async Task<IActionResult> SendConfirmationEmail(MemberIdentity member)
        {
            await SutureUserManager.SendEmailConfirmationAsync(member);

            return Ok();
        }

        [HttpPost]
        [RequireMember(RequireAuthenticatedMember = true)]
        [Route("{memberId:int}/Account/SendMobileNumberConfirmation", Name = "MemberSendMobileNumberConfirmation")]
        public async Task<IActionResult> SendMobileNumberConfirmation(MemberIdentity member)
        {
            if (string.IsNullOrWhiteSpace(member.MobileNumber))
            {
                return BadRequest();
            }

            await SutureUserManager.SendMobileNumberConfirmationAsync(member);

            return Ok();
        }

        [HttpPost]
        [RequireMember(RequireAuthenticatedMember = true)]
        [Route("{memberId:int}/Account/VerifyMobileNumberCode", Name = "MemberVerifyMobileNumberCode")]
        public async Task<IActionResult> VerifyMobileNumberCode(MemberIdentity member, [FromForm] string code)
        {
            if (!(await SutureUserManager.ConfirmMobileAsync(member, code)).Succeeded)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpGet]
        [RequireMember]
        [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
        [Route("{memberId:int}/CommunicationPreferences", Name = "MemberCommunicationPreferences")]
        public async Task<IActionResult> CommunicationPreferences(Member member)
        {
            var preferences = await GetCommunicationPreferencesByMemberId(member);

            preferences.ShowInstructions = false;
            if (!(member.Contacts?.GetPrimaryMobileNumber()?.IsConfirmed ?? false))
            {
                ModelState.AddModelError(string.Empty, "NOTE: This user does not have a confirmed mobile number. Text notifications cannot be configured.");
            }

            return View("CommunicationPreferences", new CommunicationPreferencesViewModel()
            {
                CommunicationPreferences = preferences,
                UserName = member.UserName,
                ReturnUrl = Url.RouteUrl("AdminMemberSearch"),
                CurrentUser = CurrentUser
            });
        }

        [HttpPost]
        [RequireMember]
        [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
        [Route("{memberId:int}/CommunicationPreferences", Name = "MemberSaveCommunicationPreferences")]
        public async Task<IActionResult> SaveCommunicationPreferences(Member member, [FromForm] CommunicationPreferencesModel model)
        {
            var viewModel = ((await CommunicationPreferences(member)) as ViewResult).Model as CommunicationPreferencesViewModel;
            var isUnsubscribed = model.Unsubscribe?.IsUnsubscribed ?? false;

            if (isUnsubscribed)
            {
                await SaveCommunicationPreferencesByMemberId(member.MemberId, model.Unsubscribe.Reason);
            }
            else
            {
                await SaveCommunicationPreferencesByMemberId(member.MemberId, model?.Selections ?? Array.Empty<CommunicationPreferencesSelection>(), true);
            }

            viewModel.CommunicationPreferences.Unsubscribe.IsUnsubscribed = isUnsubscribed;
            viewModel.CommunicationPreferences.Selections = isUnsubscribed ? Array.Empty<CommunicationPreferencesSelection>() : model?.Selections;
            viewModel.CompletionAlert = new AspNetCore.Models.AlertViewModel()
            {
                Style = AspNetCore.Models.AlertViewModel.ContextClass.Success,
                Text = isUnsubscribed ? "The user has been unsubscribed" : "Changes saved successfully",
                FadeSeconds = 4,
                CanClose = false
            };

            return View("CommunicationPreferences", viewModel);
        }

        protected string GetEditReturnUrl(string returnUrl, int? defaultOrganizationId, bool newUser = false)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            if (CurrentUser.IsApplicationAdministrator())
            {
                return Url.RouteUrl("AdminMemberSearch");
            }

            return defaultOrganizationId.HasValue ?
                        Url.RouteUrl("OrganizationUsers", new { organizationId = defaultOrganizationId.Value, newUser = newUser }) :
                        Url.RouteUrl("OrganizationUsersIndex", new { newUser = newUser });
        }

        protected async Task<EditModel> InitializeEditModelAsync
        (
            Member member = null,
            EditModel model = null,
            int? defaultOrganizationId = null
        )
        {
            Func<EditModel.Office, IEnumerable<EditModel.Office>, EditModel.Office> configureOffice = (office, configuredOffices) =>
            {
                var configuredOffice = configuredOffices.FirstOrDefault();

                if (configuredOffice != null)
                {
                    office.Active = configuredOffice.Active;
                    office.BillingAdministrator = configuredOffice.BillingAdministrator;
                    office.UserManagement = configuredOffice.UserManagement;
                    office.HasPendingDocuments = office.HasPendingDocuments || configuredOffice.HasPendingDocuments;
                }

                return office;
            };
            Func<Organization, string> organizationName = o => !string.IsNullOrWhiteSpace(o.OtherDesignation) && !string.Equals(o.OtherDesignation, o.Name, StringComparison.InvariantCultureIgnoreCase) ?
                                                                    $"{o.Name} ({o.OtherDesignation})" :
                                                                    o.Name;

            var sutureAdminSelectedOrganization = CurrentUser.IsApplicationAdministrator() && defaultOrganizationId.HasValue ?
                                            (await ApplicationService.GetOrganizationsByIdAsync(defaultOrganizationId.Value).FirstOrDefaultAsync()) : null;
            var adminOffices = CurrentUser.IsApplicationAdministrator() ?
                                    (member != null ? await ApplicationService.GetOrganizationMembersByMemberId(member.MemberId).Where(om => om.IsActive).Select(om => om.Organization).ToArrayAsync() : Array.Empty<Organization>())
                                    .Union(sutureAdminSelectedOrganization != null ? (sutureAdminSelectedOrganization.CompanyId.HasValue ?
                                                await ApplicationService.GetOrganizations().Where(o => o.CompanyId == sutureAdminSelectedOrganization.CompanyId.Value && o.IsActive).ToArrayAsync() :
                                                new[] { sutureAdminSelectedOrganization }) : Array.Empty<Organization>())
                                    .GroupBy(o => o.OrganizationId, (key, grp) => grp.First())
                                    .Select(o => new EditModel.Office()
                                    {
                                        OrganizationId = o.OrganizationId,
                                        Name = organizationName(o),
                                        CanEdit = true,
                                        Active = o.OrganizationId == defaultOrganizationId,
                                        BillingAdministrator = false,
                                        UserManagement = false
                                    }) :
                                    await ApplicationService.GetOrganizationMembersByMemberId(CurrentUser.Id)
                                                            .Where(om => om.IsAdministrator)
                                                            .Select(om => new EditModel.Office()
                                                            {
                                                                OrganizationId = om.OrganizationId,
                                                                Name = organizationName(om.Organization),
                                                                CanEdit = true,
                                                                Active = member == null && om.OrganizationId == defaultOrganizationId,
                                                                BillingAdministrator = false,
                                                                UserManagement = false
                                                            })
                                                            .ToArrayAsync();
            var memberOrganizationMembers = member == null ? Array.Empty<OrganizationMember>() : await ApplicationService.GetOrganizationMembersByMemberId(member.MemberId).ToArrayAsync();
            var memberOffices = memberOrganizationMembers.Where(om => om.IsActive)
                                                         .GroupJoin(member != null ?
                                                                    await RequestService.GetServiceableRequests()
                                                                                            .Where(sr => sr.SignerMemberId == member.MemberId && sr.RequestStatus == null)
                                                                                            .Select(sr => sr.SignerOrganizationId)
                                                                                            .Distinct()
                                                                                            .ToArrayAsync() :
                                                                    Array.Empty<int>(),
                                                                    om => om.OrganizationId, req => req, (om, req) => new { OrganizationMember = om, HasPendingRequests = req.Any() })
                                                         .Select(org => new EditModel.Office()
                                                         {
                                                             OrganizationId = org.OrganizationMember.OrganizationId,
                                                             Name = org.OrganizationMember.Organization.Name,
                                                             CanEdit = false,
                                                             Active = org.OrganizationMember.IsActive,
                                                             BillingAdministrator = org.OrganizationMember.IsBillingAdministrator,
                                                             UserManagement = org.OrganizationMember.IsAdministrator,
                                                             HasPendingDocuments = org.HasPendingRequests
                                                         });
            var canEditDefault = CurrentUser.IsApplicationAdministrator() ||
                                    !(memberOrganizationMembers.Where(om => om.IsActive))
                                                               .GroupJoin(adminOffices, mo => mo.OrganizationId, ao => ao.OrganizationId, (mo, aos) => new { CanEdit = aos.Any(), IsDefault = mo.IsPrimary })
                                                               .Any(mo => mo.IsDefault && !mo.CanEdit);
            if (model == null)
            {
                model = new EditModel();

                if (member != null)
                {
                    var mobileNumber = member.Contacts.Where(c => c.Type == ContactType.Mobile && c.IsActive).OrderByDescending(c => c.IsPrimary).FirstOrDefault();
                    var email = member.Contacts.Where(c => c.Type == ContactType.Email && c.IsActive).OrderByDescending(c => c.IsPrimary).FirstOrDefault();
                    var showPatientsInfo = ApplicationService
                        .GetMemberSettings(member.MemberId)
                        .Where(ms => ms.Key == AuthorizationPolicies.ShowPatientsInfo)
                        .Select(ms => ms.ItemBool)
                        .Cast<bool>()
                        .AsEnumerable()
                        .FirstOrDefault(false); // When we edit an app admin user, the default is false. ADO item #4235, Tech Req 1 / Scenario 1.

                    model.FirstName = member.FirstName;
                    model.LastName = member.LastName;
                    model.Suffix = member.Suffix;
                    model.ProfessionalSuffix = member.ProfessionalSuffix;
                    model.Npi = member.NPI;
                    model.SigningName = member.SigningName;
                    model.UserName = member.UserName;
                    model.Email = email?.Value;
                    model.MobileNumber = mobileNumber?.Value;
                    model.IsEmailConfirmed = email?.IsConfirmed ?? false;
                    model.IsMobileNumberConfirmed = mobileNumber?.IsConfirmed ?? false;
                    model.OfficePhone = member.Contacts.Where(c => c.Type == ContactType.OfficePhone && c.IsActive).OrderByDescending(c => c.IsPrimary).FirstOrDefault()?.Value;
                    model.OfficePhoneExtension = member.Contacts.Where(c => c.Type == ContactType.OfficePhoneExt && c.IsActive).OrderByDescending(c => c.IsPrimary).FirstOrDefault()?.Value;
                    model.CanSignDocuments = member.CanSign;
                    model.ShowPatientsInfo = showPatientsInfo;
                }
            }

            var modelOffices = adminOffices.Union(memberOffices.Except(memberOffices.Join(adminOffices, mo => mo.OrganizationId, ao => ao.OrganizationId, (mo, ao) => mo), new EditModel.OfficeComparer()))
                                           .GroupJoin(memberOffices, o => o.OrganizationId, mo => mo.OrganizationId, configureOffice)
                                           .GroupJoin(model?.Offices?.Join(adminOffices, mo => mo.OrganizationId, ao => ao.OrganizationId, (mo, ao) => mo) ?? Array.Empty<EditModel.Office>(), o => o.OrganizationId, mo => mo.OrganizationId, configureOffice)
                                           .OrderByDescending(o => o.CanEdit).ThenBy(o => o.Name)
                                           .ToArray();

            model.CurrentUser = CurrentUser;
            model.MemberId = member?.MemberId;
            model.MemberTypeId ??= member?.MemberTypeId;
            model.CanEditBillingAdministrator = CurrentUser.IsApplicationAdministrator() || !CurrentUser.IsUserSender();
            model.CanEditDefault = canEditDefault;
            model.Offices = modelOffices;
            model.DefaultOrganizationId = (canEditDefault ? model.DefaultOrganizationId : null) ?? memberOrganizationMembers.FirstOrDefault(om => om.IsPrimary)?.OrganizationId ?? defaultOrganizationId;
            model.Occupations = CurrentUser.IsApplicationAdministrator() ? SelectListItems.AllMemberTypes :
                                    (member != null ?
                                    MemberTypeExtensions.SenderTypes.Contains(member.MemberType) ? SelectListItems.SenderMemberTypes : SelectListItems.SignerMemberTypes :
                                    CurrentUser.IsUserSender() ? SelectListItems.SenderMemberTypes : SelectListItems.SignerMemberTypes);
            model.Occupations = (new SelectListItem[] { new SelectListItem("-- Select Profession --", string.Empty) }).Union(model.Occupations);
            model.CanSignDocuments = (new MemberType[] { MemberType.Physician, MemberType.NursePractitioner, MemberType.PhysicianAssistant }).Cast<int>().Contains(model.MemberTypeId.GetValueOrDefault()) ? model.CanSignDocuments : false;

            // The below loop is necessary because we use ASP taghelpers in the Edit view which favor using ModelState values over object model values if they exist.
            // Because web browers do not submit form values for disabled fields (such as offices the user isn't an Admin of), this can cause ModelState to have incorrect values.
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Offices[")))
            {
                ModelState.Remove(key);
            }

            return model;
        }

        protected void ValidateEditModel(EditModel model, bool isNew)
        {
            // #2441: As of writing, we don't know why the model state dictionary contains validations for the Npi property beyond the attributes specified in the model.  Remove all Npi validations
            //        and manually validate until a better solution is found.
            ModelState.Remove("Npi");
            if ((new MemberType[] { MemberType.Physician, MemberType.NursePractitioner, MemberType.PhysicianAssistant }).Cast<int>().Contains(model.MemberTypeId.GetValueOrDefault()) && string.IsNullOrWhiteSpace(model.Npi))
            {
                ModelState.AddModelError("Npi", "REQUIRED");
            }
            if (!string.IsNullOrWhiteSpace(model.Npi) && (model.Npi.Trim().Length != 10 || !long.TryParse(model.Npi, out _)))
            {
                ModelState.AddModelError("Npi", "INVALID");
            }
            if (model == null)
            {
                ModelState.AddModelError(string.Empty, "We did not understand the request you submitted.  Please try again.");
                return;
            }

            if (isNew && (model.Offices.All(o => !(o.Active || o.HasPendingDocuments)) || model.Offices.Any(o => o.CanEdit && (o.Active || o.HasPendingDocuments)) && !model.Offices.Any(o => (o.Active || o.HasPendingDocuments) && o.OrganizationId == model.DefaultOrganizationId)))
            {
                ModelState.AddModelError(string.Empty, "At least one facility must be active and set to default.");
            }
        }

        protected async Task<CommunicationPreferencesModel> GetCommunicationPreferencesByMemberId(Member member)
        {
            var days = Enumerable.Range((int)DayOfWeek.Monday, 5).Cast<DayOfWeek>();
            var memberReports = await GetEligibleReportsByChannelForMemberId(member.MemberId);
            var memberSchedule = await ScheduleService.GetMemberReportSchedules(member.MemberId).ToArrayAsync();
            var unsubscribeReason = !CurrentUser.IsApplicationAdministrator() ? null :
                                    (await ApplicationService.GetMemberSettings()
                                                             .Where(s => s.ParentId == member.MemberId && (s.IsActive ?? false) && s.Key == MEMBER_KEY_UNSUBSCRIBE_REASON)
                                                             .ToArrayAsync()).Select<MemberSetting, CommunicationPreferencesModel.UnsubscribeModel.UnsubscribeReason?>(s =>
                                                             {
                                                                 try
                                                                 {
                                                                     return Enum.Parse<CommunicationPreferencesModel.UnsubscribeModel.UnsubscribeReason>(s.ItemString);
                                                                 }
                                                                 catch
                                                                 {
                                                                     return null;
                                                                 }
                                                             }).FirstOrDefault();
            var selections = memberReports.SelectMany(r => r.Value, (kvp, r) => new { Channel = kvp.Key, ReportType = r, Schedule = memberSchedule.FirstOrDefault(s => s.ReportTypeId == r.ReportTypeId && s.Channel == kvp.Key) })
                                          .SelectMany(cr => days, (cr, d) => new { Channel = cr.Channel, ReportType = cr.ReportType, Schedule = cr.Schedule, Day = d })
                                          .Where(choice => choice.Schedule != null && (!choice.Schedule.DaysOfWeek.HasValue || choice.Schedule.DaysOfWeek.Value.HasFlag(Enum.Parse<Reporting.Scheduling.DaysOfWeek>(choice.Day.ToString()))))
                                          .Select(choice => new CommunicationPreferencesSelection()
                                          {
                                              ReportTypeId = choice.ReportType.ReportTypeId,
                                              ChannelId = (int)choice.Channel,
                                              Day = choice.Day
                                          })
                                          .ToArray();

            return new CommunicationPreferencesModel()
            {
                ShowInstructions = true,
                RequireAtLeastOneSelection = true,
                Days = days,
                Reports = memberReports.SelectMany(r => r.Value)
                                       .GroupBy(r => r.ReportTypeId, (id, r) => r.First())
                                       .OrderBy(r => r.Name)
                                       .Select((r, i) => new SelectListItem(r.Name, r.ReportTypeId.ToString(), i == 0)),
                ReportChannels = memberReports.SelectMany(r => r.Value, (kvp, r) => new { ReportTypeId = (int)r.ReportTypeId, Channel = kvp.Key })
                                              .GroupBy(cr => cr.ReportTypeId)
                                              .ToDictionary(cr => cr.Key, grp => grp.Select(cr => (int)cr.Channel)),
                Selections = selections,
                IsMobileNumberConfirmed = member.Contacts?.GetPrimaryMobileNumber()?.IsConfirmed ?? false,
                Unsubscribe = CurrentUser.IsApplicationAdministrator() ? new CommunicationPreferencesModel.UnsubscribeModel()
                {
                    IsUnsubscribed = unsubscribeReason.HasValue && !selections.Any(),
                    Reason = unsubscribeReason.GetValueOrDefault()
                } : null
            };
        }

        protected async Task SaveCommunicationPreferencesByMemberId(int memberId, IEnumerable<CommunicationPreferencesSelection> selections, bool forceRemoveUnsubscribeFlag = false)
        {
            var eligibleReports = await GetEligibleReportsByChannelForMemberId(memberId);
            var memberReports = eligibleReports.SelectMany(r => r.Value, (kvp, r) => new Reporting.Scheduling.MemberReportSchedule()
            {
                MemberId = memberId,
                Channel = kvp.Key,
                ReportTypeId = r.ReportTypeId,
                Time = null,
                DaysOfWeek = Enumerable.Aggregate(selections.Where(s => s.ReportTypeId == r.ReportTypeId && s.ChannelId == (int)kvp.Key)
                                                            .Select(s => Enum.Parse<Reporting.Scheduling.DaysOfWeek>((s.Day).ToString())),
                                                  Reporting.Scheduling.DaysOfWeek.None,
                                                  (days, day) => days | day)
            });

            if (forceRemoveUnsubscribeFlag || selections.Any())
            {
                var settingId = await ApplicationService.GetMemberSettings().Where(s => s.ParentId == memberId && s.Key == MEMBER_KEY_UNSUBSCRIBE_REASON)
                                                                            .Select(s => s.SettingId)
                                                                            .FirstOrDefaultAsync();

                if (settingId > 0)
                {
                    await ApplicationService.DeleteMemberSettingAsync(settingId);
                }
            }

            await ScheduleService.UpdateMemberReportScheduleAsync(memberId, memberReports);
        }

        protected async Task SaveCommunicationPreferencesByMemberId(int memberId, CommunicationPreferencesModel.UnsubscribeModel.UnsubscribeReason unsubscribeReason)
        {
            var settingId = await ApplicationService.GetMemberSettings().Where(s => s.ParentId == memberId && s.Key == MEMBER_KEY_UNSUBSCRIBE_REASON)
                                                                        .Select(s => s.SettingId)
                                                                        .FirstOrDefaultAsync();

            if (settingId > 0)
            {
                await ApplicationService.DeleteMemberSettingAsync(settingId);
            }
            await ApplicationService.AddMemberSettingAsync(memberId, MEMBER_KEY_UNSUBSCRIBE_REASON, null, null, unsubscribeReason.ToString(), ItemType.String);

            await SaveCommunicationPreferencesByMemberId(memberId, Array.Empty<CommunicationPreferencesSelection>());
        }

        protected async Task<IDictionary<Notifications.Channel, IEnumerable<Reporting.Scheduling.ReportType>>> GetEligibleReportsByChannelForMemberId(int memberId)
        {
            var member = await IdentityService.GetMemberIdentityByIdAsync(memberId);
            var memberOrganizations = await ApplicationService.GetOrganizationMembersByMemberId(memberId)
                                                              .Include(om => om.Organization)
                                                              .Where(om => om.IsActive)
                                                              .Select(om => om.Organization)
                                                              .ToArrayAsync();
            var memberOrganizationIds = memberOrganizations.Select(o => o.OrganizationId);
            var memberCompanyIds = memberOrganizations.Where(o => o.CompanyId.HasValue)
                                                      .Select(o => o.CompanyId.Value)
                                                      .Distinct();
            var memberOrganizationTypeIds = memberOrganizations.Where(o => o.OrganizationTypeId.HasValue)
                                                               .Select(o => o.OrganizationTypeId.Value)
                                                               .Distinct();
            var memberReports = (await ScheduleService.GetReportEligibilities())
                                                      .Where(re => (!re.MemberId.HasValue || re.MemberId.Value == memberId) &&
                                                                   (!re.MemberTypeId.HasValue || re.MemberTypeId.Value == member.MemberTypeId) &&
                                                                   (!re.CanSign.HasValue || re.CanSign.Value == member.CanSign) &&
                                                                   (!re.IsCollaborator.HasValue || re.IsCollaborator.Value == member.IsCollaborator) &&
                                                                   (!re.OrganizationId.HasValue || memberOrganizationIds.Contains(re.OrganizationId.Value)) &&
                                                                   (!re.CompanyId.HasValue || memberCompanyIds.Contains(re.CompanyId.Value)) &&
                                                                   (!re.OrganizationTypeId.HasValue || memberOrganizationTypeIds.Contains(re.OrganizationTypeId.Value)));
            var channels = (new[] { Notifications.Channel.Email, Notifications.Channel.Sms }).Where(c => c != Notifications.Channel.Sms || member.MobileNumberConfirmed);

            return channels.ToDictionary(c => c, c => memberReports.Where(r => !r.Channel.HasValue || r.Channel.Value == c).Select(r => r.ReportType).Distinct());
        }
    }
}
