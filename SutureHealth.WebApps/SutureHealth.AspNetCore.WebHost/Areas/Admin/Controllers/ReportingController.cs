using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SutureHealth.Application;
using SutureHealth.Application.Services;
using SutureHealth.AspNetCore.Areas.Admin.Models.Reporting;
using SutureHealth.AspNetCore.Identity;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Reporting;
using SutureHealth.Reporting.Account;
using SutureHealth.Reporting.Digest;
using SutureHealth.Reporting.Services;
using System.Text;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;
using ReportType = SutureHealth.AspNetCore.Mvc.Routing.ReportType;

namespace SutureHealth.AspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Reports")]
    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    public class ReportingController : Controller
    {
        IIdentityService IdentityService { get; }
        ILogger Logger { get; }
        IReportDataService ReportingService { get; }
        ReportGenerationOptions ReportOptions { get; }
        SutureUserManager UserManager { get; }

        public ReportingController
        (
            IIdentityService identityService,
            ILogger<ReportingController> logger,
            IReportDataService reportingService,
            UserManager<MemberIdentity> userManager,
            IOptions<ReportGenerationOptions> reportOptions
        )
        {
            IdentityService = identityService;
            Logger = logger;
            ReportingService = reportingService;
            ReportOptions = reportOptions.Value;
            UserManager = userManager as SutureUserManager;
        }

        [HttpGet(nameof(ReportType.UnconfirmedAccountReminder))]
        public async Task<IActionResult> GetUnconfirmedAccountReminder([FromQuery] int recipient)
        {
            var member = await UserManager.FindByIdAsync(recipient.ToString());
            if (member == null)
            {
                return NotFound();
            }

            return View("ReportView", new ReportViewModel
            {
                ComponentType = ReportOptions.ComponentType,
                CurrentUser = CurrentUser,
                Reports = new IReportViewModel[] {
                    new UnconfirmedAccountReminderViewModel(Notifications.Channel.Email, member)
                }
            });
        }

        [HttpGet("{reportType:ReportType}/{reportSubType:regex(^(password|registration)$)}")]
        public async Task<IActionResult> GetRegistrationConfirmation([FromQuery] int recipient, string reportSubType, Channel channel = Channel.Email)
        {
            var member = await UserManager.FindByIdAsync(recipient.ToString());
            if (member == null)
            {
                return NotFound();
            }


            var expirationDate = DateTime.UtcNow.AddHours(12);
            var publicIdentity = await IdentityService.CreatePublicIdentityAsync(member, IdentityUseType.OneTime, expirationDate: expirationDate);

            IEnumerable<IReportViewModel> models = Array.Empty<IReportViewModel>();
            string callbackUrl;
            switch (reportSubType)
            {
                case "password":
                    callbackUrl = Url.PageLink("/Account/ResetPassword", values: new
                    {
                        area = "Identity",
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await UserManager.GeneratePasswordResetTokenAsync(member))),
                        auth = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(publicIdentity.Value.ToString())),
                    }, protocol: "https");
                    models = new[] {
                        new PasswordResetConfirmationViewModel(Notifications.Channel.Email,
                            member,
                            "Forgot your password?",
                            callbackUrl,
                            expirationDate)
                    };
                    break;
                case "registration":
                    MemberIdentity creator = await UserManager.FindByIdAsync(member.CreatedBy.ToString());
                    MemberIdentity administrator = !(creator?.IsApplicationAdministrator() ?? true) ? creator : null;

                    if (administrator == null && member.PrimaryOrganizationId > 0)
                    {
                        administrator = await IdentityService.GetOrganizationMembersByOrganizationId(member.PrimaryOrganizationId)
                                                             .Where(om => om.IsActive && om.IsAdministrator && om.MemberId != member.MemberId)
                                                             .OrderByDescending(om => om.CreatedAt)
                                                             .Join(IdentityService.GetMemberIdentities(), om => om.MemberId, mi => mi.MemberId, (om, mi) => mi)
                                                             .FirstOrDefaultAsync();
                    }

                    callbackUrl = Url.PageLink("/Account/Registration", values: new
                    {
                        area = "Identity",
                        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await UserManager.GeneratePasswordResetTokenAsync(member))),
                        auth = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(publicIdentity.Value.ToString())),
                    }, protocol: "https");

                    var isAdministrator = await IdentityService.GetOrganizationMembersByMemberId(member.MemberId)
                                                               .AnyAsync(om => om.IsActive && om.IsAdministrator);
                    models = new[] {
                        new RegistrationConfirmationViewModel(Notifications.Channel.Email,
                            "Welcome to SutureHealth!",
                            member, isAdministrator, administrator, creator, callbackUrl, member.Email, expirationDate)
                    };
                    break;
            }

            return View("ReportView", new ReportViewModel
            {
                ComponentType = ReportOptions.ComponentType,
                CurrentUser = CurrentUser,
                Reports = models,
            });
        }

        [HttpGet("{reportType:ReportType}/{reportSubType:regex(^(assistant|sender|signer|collaborator)$)}")]
        public async Task<IActionResult> GetDigestReports(ReportType reportType, [FromQuery] int recipient, string reportSubType = null, Channel channel = Channel.Email)
        {
            IEnumerable<IReportViewModel> models = null;
            var reportData = null as IEnumerable<IDigestReportData>;

            switch (reportType, reportSubType)
            {
                case (ReportType.Digest, "assistant"):
                    reportData = await ReportingService.GetAssistantDigestReportData(recipient);
                    models = reportData.Select(data => new DigestReportViewModel<IDigestReportData>(data, Enum.Parse<SutureHealth.Notifications.Channel>(channel.ToString()), CurrentUser.Email, "You Have Documents To Review"));
                    break;
                case (ReportType.Digest, "sender"):
                    reportData = await ReportingService.GetOrganizationDigestReportData(recipient);
                    models = reportData.Select(data => new DigestReportViewModel<IDigestReportData>(data, Enum.Parse<SutureHealth.Notifications.Channel>(channel.ToString()), CurrentUser.Email, "Signature Status From SutureHealth"));
                    break;
                case (ReportType.Digest, "signer"):
                    reportData = await ReportingService.GetSignerDigestReportData(recipient);
                    models = reportData.Select(data => new DigestReportViewModel<IDigestReportData>(data, Enum.Parse<SutureHealth.Notifications.Channel>(channel.ToString()), CurrentUser.Email, "You Have Documents To Sign"));
                    break;
                case (ReportType.Digest, "collaborator"):
                    var member = await IdentityService.GetMemberIdentityByIdAsync(recipient);

                    reportData = member.CanSign ? await ReportingService.GetSignerDigestReportData(recipient) : await ReportingService.GetCollaboratorDigestReportData(recipient);
                    models = reportData.Select(data => new DigestReportViewModel<IDigestReportData>(data, Enum.Parse<SutureHealth.Notifications.Channel>(channel.ToString()), CurrentUser.Email, $"You Have Documents To {(member.CanSign ? "Sign" : "Review")}"));
                    break;
                default:
                    break;
            }

            return View("ReportView", new ReportViewModel
            {
                ComponentType = ReportOptions.ComponentType,
                CurrentUser = CurrentUser,
                Reports = models,
            });
        }
    }
}
