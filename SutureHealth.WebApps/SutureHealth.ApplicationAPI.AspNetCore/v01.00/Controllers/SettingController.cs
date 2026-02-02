using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using AutoMapper;
using SutureHealth.Application.Services;
using SutureHealth.Application.v0100.Models;
using ProducesResponseTypeAttribute = SutureHealth.AspNetCore.Mvc.ProducesResponseTypeAttribute;
using SutureHealth.Application.v01._00.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection;
using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace SutureHealth.Application.v0100.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.AuthorizedUser)]
[Route("api/v{version:apiVersion}/settings")]
[ControllerName("ApiSetting")]
public class SettingController : SutureHealth.AspNetCore.Mvc.ControllerBase
{
    protected readonly IApplicationService ApplicationService;
    protected readonly IMapper Mapper;

    public SettingController(IApplicationService applicationService, IMapper mapper)
    {
        ApplicationService = applicationService;
        Mapper = mapper;
    }

    [HttpGet("")]
    [ProducesResponseType(HttpStatusCode.OK, Type = typeof(Models.Setting))]
    [ProducesResponseType(HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetAllSettings()
    {
        if (CurrentUser == null)
        {
            return Unauthorized();
        }
        else
        {
            var appSettings = Mapper.Map<IEnumerable<SettingDetail>>(await ApplicationService.GetApplicationSettings()
                                                                                             .Where(s => s.IsActive == true)
                                                                                             .ToArrayAsync());
            var orgSettings = Mapper.Map<IEnumerable<SettingDetail>>(await ApplicationService.GetOrganizationMembersByMemberId(CurrentUser.MemberId)
                                                                                             .Where(om => om.IsActive)
                                                                         .Join(ApplicationService.GetOrganizationSettings()
                                                                                                 .Where(s => s.IsActive == true),
                                                                               o => o.OrganizationId, s => s.ParentId, (o, s) => s)
                                                                         .ToArrayAsync());
            var memberSettings = Mapper.Map<IEnumerable<SettingDetail>>(await ApplicationService.GetMemberSettings(CurrentUser.MemberId)
                                                                                                .Where(s => s.IsActive == true)
                                                                                                .ToArrayAsync());

            return Ok(new Setting()
            {
                Application = appSettings,
                Organization = orgSettings,
                Member = memberSettings
            });
        }
    }

    [HttpGet("me")]
    [ProducesResponseType(HttpStatusCode.OK, Type = typeof(IEnumerable<Models.SettingDetail>))]
    [ProducesResponseType(HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetMemberSettings()
    {
        if (CurrentUser == null)
        {
            return Unauthorized();
        }
        else
        {
            var memberSettings = Mapper.Map<IEnumerable<SettingDetail>>(await ApplicationService.GetMemberSettings(CurrentUser.MemberId)
                                                                                                .Where(s => s.IsActive == true)
                                                                                                .ToArrayAsync());

            return Ok(memberSettings);
        }
    }

    [HttpGet("organization")]
    [ProducesResponseType(HttpStatusCode.OK, Type = typeof(IEnumerable<Models.SettingDetail>))]
    [ProducesResponseType(HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetOrganizationSettings()
    {
        if (CurrentUser == null)
        {
            return Unauthorized();
        }
        else
        {
            var organizationSettings = Mapper.Map<IEnumerable<SettingDetail>>(await ApplicationService.GetOrganizationMembersByMemberId(CurrentUser.MemberId)
                                                                                                      .Where(om => om.IsActive == true)
                                                                                 .Join(ApplicationService.GetOrganizationSettings()
                                                                                                         .Where(s => s.IsActive == true),
                                                                                       o => o.OrganizationId, s => s.ParentId, (o, s) => s)
                                                                                 .ToArrayAsync());

            return Ok(organizationSettings);
        }
    }

    [HttpGet("application")]
    [ProducesResponseType(HttpStatusCode.OK, Type = typeof(IEnumerable<Models.SettingDetail>))]
    [ProducesResponseType(HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetApplicationSettings()
    {
        if (CurrentUser == null)
        {
            return Unauthorized();
        }
        else
        {
            var applicationSettings = Mapper.Map<IEnumerable<SettingDetail>>(await ApplicationService.GetApplicationSettings()
                                                                                                     .Where(s => s.IsActive == true)
                                                                                                     .ToArrayAsync());

            return Ok(applicationSettings);
        }
    }

    /// <summary>
    /// Returns some miscellaneous settings like ShowNewInbox, AllowOtherToSignFromScreen and DocumentViewDuration
    /// according to the hierarchical levels (Member > Org > App)
    /// </summary>
    /// <returns></returns>
    [HttpGet("misc")]
    [ProducesResponseType(HttpStatusCode.OK, Type = typeof(MiscSettings))]
    [ProducesResponseType(HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetMiscSettings()
    {
        if (CurrentUser == null)
        {
            return Unauthorized();
        }
        else
        {
            var miscSettings = new MiscSettings();
            var memberSettings = Mapper.Map<IEnumerable<SettingDetail>>(await ApplicationService.GetMemberSettings(CurrentUser.MemberId)
                                        .Where(s => s.IsActive == true)
                                        .ToListAsync());
            var orgSettings = Mapper.Map<IEnumerable<SettingDetail>>(await ApplicationService.GetOrganizationMembersByMemberId(CurrentUser.MemberId)
                                        .Where(om => om.IsActive)
                                        .Join(ApplicationService.GetOrganizationSettings()
                                        .Where(s => s.IsActive == true),
                                         o => o.OrganizationId, s => s.ParentId, (o, s) => s)
                                        .ToListAsync());

            var organization = await ApplicationService.GetOrganizations()
                                .Where(o => o.OrganizationId == CurrentUser.PrimaryOrganizationId).FirstOrDefaultAsync();
            var companySettings = Mapper.Map<IEnumerable<SettingDetail>>(await ApplicationService.GetOrganizationSettings()
                                            .Where(o => o.ParentId == organization.CompanyId).ToArrayAsync());

            if (MiscSettings.HasDuplicateOrgSettings(orgSettings))
            {
                var primOrg = await ApplicationService.GetOrganizationMembersByMemberId(CurrentUser.MemberId)
                                        .Where(om => om.IsActive && om.IsPrimary).FirstOrDefaultAsync();
                orgSettings = MiscSettings.RemoveDuplicateOrgSettings(orgSettings, primOrg.OrganizationId);
            }

            var appSettings = Mapper.Map<IEnumerable<SettingDetail>>(await ApplicationService.GetApplicationSettings()
                                        .Where(s => s.IsActive == true)
                                        .ToListAsync());

            miscSettings.FindMiscSettings(memberSettings, orgSettings, companySettings, appSettings);
            // Users who log in with Duo SSO SAML2 can use only the new inbox
            if (User != null)
                if (User.Identity.AuthenticationType == "AuthenticationTypes.Federation")
                    miscSettings.ShowNewInbox = false;

            return Ok(miscSettings);
        }
    }

    /// <summary>
    /// Saves the misc settings which are not null, into the Member Settings of the logged-in user.
    /// </summary>
    /// <param name="miscSettings"></param>
    /// <returns></returns>
    [HttpPost("misc")]
    [ProducesResponseType(HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> SaveMiscSettings([FromBody] MiscSettings miscSettings)
    {
        if (CurrentUser == null)
        {
            return Unauthorized();
        }
        else
        {
            var miscType = typeof(MiscSettings);

            foreach (var property in miscType.GetProperties())
            {
                await SaveMiscMemberSetting(miscSettings, property);
            }

            return Ok();
        }
    }

    /// <summary>
    /// Gets the IsFirstInboxTour Setting for the logged-in user
    /// </summary>
    /// <param name="firstInboxTour"></param>
    /// <returns></returns>
    [HttpGet("firstInboxTour")]
    [ProducesResponseType(HttpStatusCode.OK, Type = typeof(InboxTour))]
    [ProducesResponseType(HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> GetFirstInboxTour()
    {
        if (CurrentUser == null)
        {
            return Unauthorized();
        }

        if (!CurrentUser.IsUserSigningMember())
        {
            return Ok(new InboxTour()
            {
                UserId = CurrentUser.MemberId,
                isFirstInboxTourDone = true
            });
        }

        const string firstInboxTour = "IsFirstInboxTourDone";
        var isFirstTourDone = await ApplicationService.GetMemberSettings(CurrentUser.MemberId)
            .Where(s => s.Key.ToLower() == firstInboxTour.ToLower() && s.IsActive == true)
            .FirstOrDefaultAsync();

        return Ok(new InboxTour
        {
            UserId = CurrentUser.MemberId,
            isFirstInboxTourDone = isFirstTourDone is { ItemBool: true }
        });
    }

    /// <summary>
    /// Sets the isFirstInboxTourDone setting for the logged-in member to true
    /// </summary>
    /// <param name="firstInboxTour"></param>
    /// <returns></returns>
    [HttpPost("firstInboxTour")]
    [ProducesResponseType(HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> SetFirstInboxTour()
    {
        if (CurrentUser == null)
        {
            return Unauthorized();
        }
        var firstInboxTour = "IsFirstInboxTourDone";
        var isFirstTourDone = await ApplicationService.GetMemberSettings(CurrentUser.MemberId)
                                       .Where(s => s.Key.ToLower() == firstInboxTour.ToLower() && s.IsActive == true)
                                       .FirstOrDefaultAsync();

        if (isFirstTourDone == null || isFirstTourDone.ItemBool != true)
        {
            await ApplicationService.AddMemberSettingAsync(CurrentUser.MemberId, firstInboxTour, true, null, null, ItemType.Boolean);
        }

        return Ok();
    }

    private async Task SaveMiscMemberSetting(MiscSettings miscSettings, PropertyInfo property)
    {
        var settingName = property.Name;
        var settingValue = property.GetValue(miscSettings);
        if (settingValue == null)
            return;

        var existingSettingId = await ApplicationService.GetMemberSettings(CurrentUser.MemberId)
                                       .Where(s => s.Key.ToLower() == settingName.ToLower() && s.IsActive == true)
                                       .Select(s => s.SettingId)
                                       .FirstOrDefaultAsync();

        if (existingSettingId > 0)
        {
            await ApplicationService.DeleteMemberSettingAsync(existingSettingId);
        }

        if (property.PropertyType.FullName.Contains("System.Boolean"))
        {
            await ApplicationService.AddMemberSettingAsync(CurrentUser.MemberId, settingName, settingValue as bool?, null, null, ItemType.Boolean);
        }
        else if (property.PropertyType.FullName.Contains("System.Int32"))
        {
            await ApplicationService.AddMemberSettingAsync(CurrentUser.MemberId, settingName, null, settingValue as int?, null, ItemType.Integer);
        }
        else if (property.PropertyType.FullName.Contains("System.String"))
        {
            await ApplicationService.AddMemberSettingAsync(CurrentUser.MemberId, settingName, null, null, settingValue as string, ItemType.String);
        }

    }

    /// <summary>
    /// Delete one of the misc settings of a member who is logged-in.
    /// </summary>
    /// <param name="miscSettingName">the name of the one setting under member misc settings</param>
    /// <returns></returns>
    [HttpDelete("misc")]
    [ProducesResponseType(HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> DeleteMemberMiscSettings(string miscSettingName)
    {
        if (CurrentUser == null)
        {
            return Unauthorized();
        }
        else
        {
            var miscType = typeof(MiscSettings);

            if (miscType.GetProperties().Any(p => p.Name.ToLower() == miscSettingName.ToLower()))
            {
                var existingSettingId = await ApplicationService.GetMemberSettings(CurrentUser.MemberId)
                                       .Where(s => s.Key.ToLower() == miscSettingName.ToLower() && s.IsActive == true)
                                       .Select(s => s.SettingId)
                                       .FirstOrDefaultAsync();

                await ApplicationService.DeleteMemberSettingAsync(existingSettingId);
            }

            return Ok();
        }
    }

    [HttpGet("viewOnly")]
    [ProducesResponseType(HttpStatusCode.OK)]
    public async Task<IActionResult> GetIsViewOnly()
    {
        if (CurrentUser is null)
        {
            return Unauthorized();
        }
        string viewOnlyKey = "ViewOnly";
        var isViewOnlyser = ApplicationService.GetMemberSettings(CurrentUser.MemberId)
                                                                      .Where(ms => (ms.Key == viewOnlyKey && ms.IsActive == true))
                                                                      .FirstOrDefault();

        var isViewOnly = isViewOnlyser?.ItemBool ?? false; 
        return Ok(isViewOnly);
    }
}