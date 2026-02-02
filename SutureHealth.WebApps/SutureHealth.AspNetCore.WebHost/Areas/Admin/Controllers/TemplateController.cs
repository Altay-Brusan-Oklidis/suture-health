using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kendo.Mvc.UI;
using SutureHealth.AspNetCore.Areas.Admin.Models.Template;
using SutureHealth.AspNetCore.Mvc.Attributes;
using SutureHealth.Documents.Services;
using SutureHealth.Application;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Template")]
    [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
    public class TemplateController : Controller
    {
        protected IDocumentServicesProvider DocumentService { get; set; }

        public TemplateController
        (
            IDocumentServicesProvider documentService
        )
        {
            DocumentService = documentService;
        }

        [HttpGet]
        [RequireAuthorizedOrganization]
        [Route("/Admin/Organization/{organizationId:int}/Template/Search", Name = "AdminTemplateOrganizationSearch")]
        public IActionResult OrganizationSearch(Organization organization)
        {
            return View(new OrganizationSearchModel()
            {
                CurrentUser = CurrentUser,
                OrganizationId = organization.OrganizationId,
                OrganizationName = organization.Name,
                AddTemplateUrl = Url.Page("/Create", new { area = "Template", organizationId = organization.OrganizationId })
            });
        }

        [HttpPost]
        [RequireAuthorizedOrganization]
        [Route("/Admin/Organization/{organizationId:int}/Template/Search/DataSource", Name = "AdminTemplateTemplateDataSource")]
        public async Task<IActionResult> TemplateDataSource(Organization organization)
        {
            var templates = await DocumentService.GetParentTemplatesByFacilityIdAsync(organization.OrganizationId);
            return Json(new DataSourceResult()
            {
                Total = templates.Count(),
                Data = templates.Select(t => new TemplateListItem(t, Url))
            });
        }

        [HttpPost]
        [RequireAuthorizedOrganization]
        [Route("/Admin/Organization/{organizationId:int}/Template/Update", Name = "AdminTemplateUpdateTemplate")]
        public async Task<IActionResult> UpdateTemplate(Organization organization, [FromForm] TemplateListItem item)
        {
            await DocumentService.SaveTemplateConfigurationAsync(new SaveTemplateConfigurationRequest()
            {
                TemplateId = item.TemplateId,
                Enabled = item.IsApiEnabled,
                TemplateProcessingMode = item.TemplateProcessingModeId switch
                {
                    1 => Documents.TemplateProcessingMode.ParentCoordinates,
                    2 => Documents.TemplateProcessingMode.DDP,
                    _ => throw new ArgumentException("TemplateProcessingModeId must be 1 or 2.", nameof(item.TemplateProcessingModeId))
                },
                DocumentTypeKey = item.ApiDocumentKey ?? string.Empty
            }, CurrentUser.UserName);

            return Json(new DataSourceResult()
            {
                Total = 1,
                Data = new TemplateListItem[]
                {
                    new TemplateListItem((await DocumentService.GetParentTemplatesByFacilityIdAsync(organization.OrganizationId)).First(t => t.TemplateId == item.TemplateId), Url)
                }
            });
        }

        [HttpPost]
        [RequireAuthorizedTemplate]
        [Route("{templateId:int}/Ocr/Scan", Name = "AdminTemplateScanTemplate")]
        public async Task<IActionResult> ScanTemplate(SutureHealth.Documents.Template template)
        {
            await DocumentService.StartOcrAnalyzeTemplateConfigurationAsync(template.TemplateId, template.StorageKey, CurrentUser.UserName);
            return Ok();
        }
    }
}
