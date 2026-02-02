using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SutureHealth.AspNetCore.Areas.Network.Models.Invitation;
using SutureHealth.AspNetCore.Areas.Network.Models.Network;
using SutureHealth.Providers;
using SutureHealth.Providers.Services;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.Application;
using Microsoft.EntityFrameworkCore;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Network.Controllers
{
    [Area("Network")]
    [Route("Network/Invitation")]
    public class InvitationController : Controller
    {
        protected INetworkServicesProvider ProviderService { get; }
        IApplicationService SecurityService { get; }
        protected ILogger<InvitationController> Logger { get; }

        public InvitationController
        (
            INetworkServicesProvider providerService,
            IApplicationService securityService,
            ILogger<InvitationController> logger
        )
        {
            ProviderService = providerService;
            SecurityService = securityService;
            Logger = logger;
        }

        #region Invitation Modal
        [HttpGet]
        [Route("Modal", Name = "NetworkInvitationModal")]
        public PartialViewResult InvitationModal(int numberToBeInvited)
        {
            return PartialView("_InvitationModal", new InvitationModal()
            {
                CompanyQuantityText = numberToBeInvited == 1 ? $"{numberToBeInvited} company" : $"{numberToBeInvited} companies"
            });
        }

        [HttpPost]
        [Route("Modal/Footer", Name = "NetworkInvitationModalFooter")]
        public PartialViewResult InvitationModalFooter(InvitationModalFooterRequest request)
        {
            string usageRequirementText, timeExpectationText;

            switch (request.UsageRequirement)
            {
                case CommitmentLevel.Recommended:
                    usageRequirementText = "We highly prefer to sign all documents electronically through SutureHealth.";
                    timeExpectationText = $"As of {DateTime.Now.AddDays((int)request.TimeExpectation):d} we expect to receive all documents in SutureHealth.";
                    break;
                case CommitmentLevel.Require:
                    usageRequirementText = "We now exclusively sign all documents electronically through SutureHealth.";
                    timeExpectationText = $"As of {DateTime.Now.AddDays((int)request.TimeExpectation):d} we no longer accept documents any other way.";
                    break;
                case CommitmentLevel.NotRequired:
                default:
                    usageRequirementText = "We highly prefer to sign all documents electronically through SutureHealth.";
                    timeExpectationText = $"As of {DateTime.Now.AddDays((int)request.TimeExpectation):d} we prefer to receive all documents in SutureHealth.";
                    break;
            }

            return PartialView("_InvitationModalFooter", new InvitationModalFooter()
            {
                UsageRequirementText = usageRequirementText,
                TimeExpectationText = timeExpectationText,
                DownloadSampleUrl = Url.RouteUrl("NetworkInvitationSample", new { timeExpectation = request.TimeExpectation.ToString(), usageRequirement = request.UsageRequirement.ToString() })
            });
        }
        #endregion

        [HttpGet]
        [Route("Sample", Name ="NetworkInvitationSample")]
        public async Task<IActionResult> SampleInvitation(MemberIdentity sutureUser, TimeExpectation timeExpectation, CommitmentLevel usageRequirement, bool download = false)
        {
            var user = await SecurityService.GetMemberByIdAsync(sutureUser.Id);
            var primaryFacility = await SecurityService.GetOrganizationByIdAsync(sutureUser.PrimaryOrganizationId);
            var physicians = sutureUser.MemberTypeId != 2000 ? await GetSignersForUser(user) : Array.Empty<Member>();
            if (download)
            {
                this.Response.Headers.Add("Content-Disposition", "attachment; filename=Sample Invitation.doc");
                this.Response.Headers.Add("Content-Type", "application/msword");
                this.Response.Headers.Add("Cache-Control", "no-cache");
            }
            
            return View("FaxInvitation", new FaxInvitation()
            {
                ReceiveDocumentTime = timeExpectation,
                UsageRequirement = usageRequirement,
                IsDownloading = download,
                DownloadUrl = Url.RouteUrl("NetworkInvitationSample", new { timeExpectation, usageRequirement, download = true }),
                FacilityName = primaryFacility.Name,
                SubmitterFullName = $"{user.FirstName} {user.LastName} {user.ProfessionalSuffix}",
                PhysicianFullNames = physicians.Select(u => $"{u.FirstName} {u.LastName} {u.ProfessionalSuffix}")
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Salesforce/{invitationId}/Email", Name = "NetworkInvitationEmail")]
        public async Task<IActionResult> EmailInvitation(string invitationId)
        {
            return await GetInvitationViewById(invitationId, "EmailInvitation", new EmailInvitation());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Salesforce/{invitationId}/Fax", Name = "NetworkInvitationFax")]
        public async Task<IActionResult> FaxInvitation(string invitationId)
        {
            return await GetInvitationViewById(invitationId, "FaxInvitation", new FaxInvitation()
            {
                IsDownloading = true
            });
        }

        [HttpPost]
        [Route("", Name = "NetworkInvitationInvite")]
        public async Task<IActionResult> InviteSpecificProviders([FromBody] InviteSpecificProvidersRequest request)
        {
            try
            {
                var member = await SecurityService.GetMemberByIdAsync(CurrentUser.Id);
                var primaryFacility = await SecurityService.GetOrganizationByIdAsync(CurrentUser.PrimaryOrganizationId);
                var inviters = await GetSignersForUser(member);
                var administrator = (await SecurityService.GetOrganizationMembersByOrganizationId(CurrentUser.PrimaryOrganizationId)
                                                          .Where(om => om.IsActive && om.IsAdministrator)
                                                          .OrderByDescending(om => om.IsPrimary)
                                                          .FirstAsync())
                                                          .Member;

                await ProviderService.CreateInvitation(new InvitationCreateRequest
                {
                    Invitees = request.SelectedProviders,
                    Inviters = inviters.Select(inviter => new Inviter
                    {
                        Role = GetRolesForUser(inviter),
                        UserID = inviter.MemberId
                    }).Where(x => x != null),
                    ReceiveDocumentTime = request.ReceiveDocumentTime,
                    Submitter = new SutureHealth.Providers.SutureUser
                    {
                        Role = GetRolesForUser(member),
                        UserId = CurrentUser.Id,
                        Company = new SutureFacility
                        {
                            FacilityId = primaryFacility.OrganizationId
                        }
                    },
                    UsageRequirement = request.UsageRequirement,
                    CreateDate = DateTime.UtcNow,
                    Administrator = new SutureHealth.Providers.SutureUser
                    {
                        // THIS IS PROBABLY WRONG BUT GONNA TRY ANYWAY!!! TODO: GET DONNIE TO ASK ABOUT THIS
                        Company = new SutureFacility
                        {
                            FacilityId = primaryFacility.OrganizationId
                        },
                        UserId = administrator.MemberId,
                        Role = GetRolesForUser(administrator, true)
                    }
                });

                return Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "InviteSpecificProviders Error");

                throw;
            }
        }

        protected async Task<IActionResult> GetInvitationViewById(string invitationId, string viewName, InvitationContent model)
        {
            var invitation = await this.ProviderService.GetInvitationById(invitationId);
            var submitter = await SecurityService.GetMemberByIdAsync(invitation.SubmittedBy.UserId);
            var primaryFacility = await SecurityService.GetOrganizationByIdAsync(CurrentUser.PrimaryOrganizationId);
            var inviters = submitter.MemberTypeId != 2000 ? await GetSignersForUser(submitter) : Array.Empty<Member>();

            model.FacilityName = primaryFacility.Name;
            model.PhysicianFullNames = inviters.Select(u => $"{u.FirstName} {u.LastName} {u.ProfessionalSuffix}");
            model.ReceiveDocumentTime = invitation.TimeFrame;
            model.UsageRequirement = invitation.CommitmentLevel;
            model.SubmitterFullName = $"{submitter.FirstName} {submitter.LastName} {submitter.ProfessionalSuffix}";

            return View(viewName, model);
        }

        protected string GetRolesForUser(Member user, bool isSutureAdmin = false)
        {
            List<string> roles = new List<string>();

            if (isSutureAdmin)
            {
                roles.Add("SH Admin");
            }

            if (user.IsCollaborator)
            {
                roles.Add("Collaborator");
            }

            switch (user.MemberTypeId)
            {
                case 2000:
                    roles.Add("Physician");
                    break;
                case 2001:
                    roles.Add("Nurse Practitioner");
                    break;
                case 2008:
                    roles.Add("Physician Assistant");
                    break;
                case 2002:
                    roles.Add("Nurse");
                    break;
                case 2015:
                    roles.Add("Manager");
                    break;
                default:
                    roles.Add("Staff");
                    break;
            }

            return string.Join(";", roles);
        }

        protected async Task<IEnumerable<Member>> GetSignersForUser(Member member) =>
            member.MemberTypeId == 2000 ? new Member[] { member } 
                                        : await SecurityService.GetSupervisorsForMemberId(member.MemberId)
                                                               .Select(mr => mr.Supervisor)
                                                               .ToArrayAsync();
    }
}
