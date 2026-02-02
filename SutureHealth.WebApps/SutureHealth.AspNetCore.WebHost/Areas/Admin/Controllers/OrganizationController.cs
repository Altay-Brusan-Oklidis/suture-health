using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using SutureHealth.AspNetCore.Mvc.Extensions;
using SutureHealth.AspNetCore.Areas.Admin.Models;
using SutureHealth.AspNetCore.Areas.Admin.Models.Organization;
using SutureHealth.Application.Services;
using SutureHealth.Application;
using SutureHealth.AspNetCore.Mvc.Attributes;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Organization")]
    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    public class OrganizationController : Controller
    {
        protected IApplicationService SecurityService { get; }

        public OrganizationController
        (
            IApplicationService securityService
        )
        {
            SecurityService = securityService;
        }

        [HttpGet]
        [Route("", Name = "AdminOrganizationIndex")]
        public IActionResult Index(bool contentOnly = false)
        {
            return View(new IndexModel()
            {
                CreateOrganizationUrl = Url.RouteUrl("OrganizationCreate"),
                CurrentUser = CurrentUser,
                RequireClientHeader = !contentOnly
            });
        }

        [HttpPost]
        [Route("", Name = "AdminOrganizationOrganizationsDataSource")]
        public async Task<IActionResult> OrganizationsDataSource([DataSourceRequest] DataSourceRequest request)
        {
            request.Filters.Transform("TypeId", "OrganizationTypeId");
            request.Filters.Transform("ExternalName", "Name");
            request.Filters.Transform("InternalName", "OtherDesignation");
            request.Filters.Transform("State", "StateOrProvince");
            request.Filters.Transform("Zip", "PostalCode");
            request.Filters.Transform("IsPaid", "IsFree", fd => fd.Value = !((bool)fd.Value));
            //request.Filters.Transform("DateClosedTicks", "ClosedAt", fd =>
            //{
            //    fd.Operator = Convert.ToBoolean(fd.Value) ? Kendo.Mvc.FilterOperator.IsNotNull : Kendo.Mvc.FilterOperator.IsNull;
            //    fd.Value = null;
            //});
            request.Filters.Transform("ParentOrganizationId", "ParentId");

            request.Sorts.Transform("ExternalName", s => s.Member = "Name");
            request.Sorts.Transform("InternalName", s => s.Member = "OtherDesignation");
            request.Sorts.Transform("State", s => s.Member = "StateOrProvince");
            request.Sorts.Transform("Zip", s => s.Member = "PostalCode");
            //request.Sorts.Transform("DateClosedTicks", s => s.Member = "ClosedAt");
            request.Sorts.Transform("ParentOrganizationId", s => s.Member = "ParentId");
            request.Sorts.Transform("DateCreatedTicks", s => s.Member = "CreatedAt");

            return Json(await SecurityService.GetOrganizations().ToDataSourceResultAsync(request, o => new OrganizationListItem()
            {
                OrganizationId = o.OrganizationId,
                CompanyId = o.CompanyId,
                ParentOrganizationId = o.ParentId,
                ExternalName = o.Name,
                InternalName = o.OtherDesignation,
                Npi = o.NPI ?? string.Empty,
                MedicareNumber = o.MedicareNumber ?? string.Empty,
                DateCreated = o.CreatedAt.ToString("d"),
                DateCreatedTicks = o.CreatedAt.Ticks,
                //DateClosed = o.ClosedAt?.ToString("d") ?? string.Empty,
                //DateClosedTicks = o.ClosedAt?.Ticks ?? 0,
                TypeId = o.OrganizationTypeId,
                Type = o.OrganizationType?.Name ?? string.Empty,
                IsActive = o.IsActive,
                IsPaid = !o.IsFree,
                Phone = o.Contacts?.FirstOrDefault(c => c.Type == ContactType.Phone)?.Value.ToFormattedPhoneNumber() ?? string.Empty,
                Fax = o.Contacts?.FirstOrDefault(c => c.Type == ContactType.Fax)?.Value.ToFormattedPhoneNumber() ?? string.Empty,
                Address = $"{o.AddressLine1} {o.AddressLine2}".Trim(),
                City = o.City,
                State = o.StateOrProvince,
                Zip = o.PostalCode,
                EditOrganizationUrl = Url.RouteUrl("OrganizationProfile", new { organizationId = o.OrganizationId, contentOnly = true }),
                SettingsActionUrl = Url.RouteUrl("AdminOrganizationSettings", new { organizationId = o.OrganizationId }),
                TemplateManagementUrl = Url.RouteUrl("AdminTemplateOrganizationSearch", new { organizationId = o.OrganizationId })
            }));
        }

        [HttpGet]
        [Route("Types", Name = "AdminOrganizationOrganizationTypesDataSource")]
        public async Task<IActionResult> OrganizationTypesDataSource()
        {
            var items = await SecurityService.GetOrganizationTypes();

            return Json(new DataSourceResult()
            {
                Total = items.Count(),
                Data = items.Select(ot => new OrganizationTypeListItem()
                {
                    OrganizationTypeId = ot.OrganizationTypeId,
                    Name = ot.Name
                })
            });
        }

        [HttpGet]
        [Route("{organizationId:int}/Settings", Name = "AdminOrganizationSettings")]
        public async Task<IActionResult> Settings([FromRoute] int organizationId)
        {
            var organization = await SecurityService.GetOrganizationByIdAsync(organizationId);

            if (organization == null)
            {
                return NotFound();
            }

            return View(new SettingsModel()
            {
                CurrentUser = CurrentUser,
                OrganizationName = organization.Name,
                SettingsGrid = new Models.SettingsGridModel()
                {
                    ReadUrl = Url.RouteUrl("AdminOrganizationSettingsDataSource", new { organizationId = organizationId }),
                    CreateUrl = Url.RouteUrl("AdminOrganizationCreateSetting", new { organizationId = organizationId }),
                    UpdateUrl = Url.RouteUrl("AdminOrganizationUpdateSetting", new { organizationId = organizationId }),
                    DeleteUrl = Url.RouteUrl("AdminOrganizationDeleteSetting", new { organizationId = organizationId })
                }
            });
        }

        [HttpGet]
        [Route("{organizationId:int}/Settings/Data", Name = "AdminOrganizationSettingsDataSource")]
        public async Task<IActionResult> SettingsDataSource([FromRoute] int organizationId)
        {
            var settings = await SecurityService.GetOrganizationSettings(organizationId).ToArrayAsync();
            return Json(new DataSourceResult()
            {
                Data = settings.Select(s => new SettingsListItem(s)),
                Total = settings.Count()
            });
        }

        [HttpPost]
        [Route("{organizationId:int}/Settings/Update", Name = "AdminOrganizationUpdateSetting")]
        public async Task<IActionResult> UpdateSetting([FromRoute] int organizationId, [FromForm] SettingsListItem item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var setting = await SecurityService.SetOrganizationSettingAsync(
                 item.SettingId,
                 item.Name,
                 item.Type == ItemType.Boolean.ToString() ? Convert.ToBoolean(item.Value) : null,
                 item.Type == ItemType.Integer.ToString() ? Convert.ToInt32(item.Value) : null,
                 item.Type == ItemType.String.ToString() ? item.Value : null,
                 Enum.Parse<ItemType>(item.Type),
                 item.Active);

            return Json(new DataSourceResult()
            {
                Data = new SettingsListItem[] { new SettingsListItem(setting) },
                Total = 1
            });
        }

        [HttpPost]
        [Route("{organizationId:int}/Settings/Create", Name = "AdminOrganizationCreateSetting")]
        public async Task<IActionResult> CreateSetting([FromRoute] int organizationId, [FromForm] SettingsListItem item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var setting = await SecurityService.AddOrganizationSettingAsync(
                 organizationId,
                 item.Name,
                 item.Type == ItemType.Boolean.ToString() ? Convert.ToBoolean(item.Value) : null,
                 item.Type == ItemType.Integer.ToString() ? Convert.ToInt32(item.Value) : null,
                 item.Type == ItemType.String.ToString() ? item.Value : null,
                 Enum.Parse<ItemType>(item.Type),
                 item.Active);

            return Json(new DataSourceResult()
            {
                Data = new SettingsListItem[] { new SettingsListItem(setting) },
                Total = 1
            });
        }

        [HttpPost]
        [Route("{organizationId:int}/Settings/Delete", Name = "AdminOrganizationDeleteSetting")]
        public async Task<IActionResult> DeleteSetting([FromForm] SettingsListItem item)
        {
            await SecurityService.DeleteOrganizationSettingAsync(item.SettingId);

            return Json(new DataSourceResult()
            {
                Data = new SettingsListItem[] { item },
                Total = 1
            });
        }

        [HttpPost]
        [RequireAuthorizedOrganization]
        [Route("{organizationId:int}/ToggleActivationStatus", Name = "AdminOrganizationToggleActivationStatus")]
        public async Task<IActionResult> ToggleActivationStatus(Organization organization)
        {
            return Json(new ToggleActivationStatusResponse()
            {
                Success = (await SecurityService.ToggleOrganizationActiveStatusAsync(organization.OrganizationId, CurrentUser.Id)) != organization.IsActive
            });
        }
    }
}
