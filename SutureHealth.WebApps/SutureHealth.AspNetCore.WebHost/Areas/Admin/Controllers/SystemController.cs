using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using SutureHealth.AspNetCore.Areas.Admin.Models;
using SutureHealth.AspNetCore.Areas.Admin.Models.System;
using SutureHealth.Application.Services;
using Microsoft.EntityFrameworkCore;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/System")]
    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    public class SystemController : Controller
    {
        protected IApplicationService SecurityService { get; }

        public SystemController
        (
            IApplicationService securityService
        )
        {
            SecurityService = securityService;
        }

        [HttpGet]
        [Route("", Name = "AdminSystemIndex")]
        public async Task<IActionResult> Index()
        {
            return View(new IndexModel()
            {
                CurrentUser = CurrentUser,
                Settings = new IndexModel.SettingsTab()
                {
                    SettingsGrid = new SettingsGridModel()
                    {
                        ReadUrl = Url.RouteUrl("AdminSystemSettingsDataSource"),
                        UpdateUrl = Url.RouteUrl("AdminSystemUpdateSetting"),
                        DeleteUrl = Url.RouteUrl("AdminSystemDeleteSetting"),
                        CreateUrl = Url.RouteUrl("AdminSystemCreateSetting")
                    }
                },
            });
        }

        #region Settings Tab

        [HttpGet]
        [Route("Settings", Name = "AdminSystemSettingsDataSource")]
        public async Task<IActionResult> SettingsDataSource()
        {
            var settings = await SecurityService.GetApplicationSettings().ToArrayAsync();
            return Json(new DataSourceResult()
            {
                Data = settings.OrderBy(s => s.Key)
                               .Select(s => new SettingsListItem(s)),
                Total = settings.Length
            });
        }

        [HttpPost]
        [Route("Settings/Update", Name = "AdminSystemUpdateSetting")]
        public async Task<IActionResult> UpdateSetting([FromForm] SettingsListItem item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var setting = await SecurityService.SetApplicationSettingAsync(
                item.SettingId,
                item.Name,
                item.Type == Application.ItemType.Boolean.ToString() ? Convert.ToBoolean(item.Value) : null,
                item.Type == Application.ItemType.Integer.ToString() ? Convert.ToInt32(item.Value) : null,
                item.Type == Application.ItemType.String.ToString() ? item.Value : null,
                Enum.Parse<Application.ItemType>(item.Type),
                item.Active);

            return Json(new DataSourceResult()
            {
                Data = new SettingsListItem[] { new SettingsListItem(setting) },
                Total = 1
            });
        }

        [HttpPost]
        [Route("Settings/Create", Name = "AdminSystemCreateSetting")]
        public async Task<IActionResult> CreateSetting([FromForm] SettingsListItem item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var setting = await SecurityService.AddApplicationSettingAsync(
                item.Name,
                item.Type == Application.ItemType.Boolean.ToString() ? Convert.ToBoolean(item.Value) : null,
                item.Type == Application.ItemType.Integer.ToString() ? Convert.ToInt32(item.Value) : null,
                item.Type == Application.ItemType.String.ToString() ? item.Value : null,
                Enum.Parse<Application.ItemType>(item.Type),
                item.Active);

            return Json(new DataSourceResult()
            {
                Data = new SettingsListItem[] { new SettingsListItem(setting) },
                Total = 1
            });
        }

        [HttpPost]
        [Route("Settings/Delete", Name = "AdminSystemDeleteSetting")]
        public async Task<IActionResult> DeleteSetting([FromForm] SettingsListItem item)
        {
            await SecurityService.DeleteApplicationSettingAsync(item.SettingId);

            return Json(new DataSourceResult()
            {
                Data = new SettingsListItem[] { item },
                Total = 1
            });
        }

        #endregion
    }
}
