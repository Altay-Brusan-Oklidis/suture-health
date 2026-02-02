using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Documents.Services;
using SutureHealth.Application.Services;
using SutureHealth.Storage;
using System.ComponentModel.DataAnnotations;

namespace SutureHealth.AspNetCore.Areas.Template.Pages
{
    public class CreateModel : BasePageModel
    {
        private const int MAX_UPLOAD_BYTES = 2097152;

        public IApplicationService SecurityService { get; set; }
        public IDocumentServicesProvider DocumentService { get; set; }
        public IBinaryStorageService StorageService { get; set; }

        public long MaxUploadBytes => MAX_UPLOAD_BYTES;
        public bool HasMultipleOrganizations => Offices != null && Offices.Count() > 1;
        public IEnumerable<Office> Offices { get; set; }
        public string ExistingTemplateNames { get; set; }   // NOTE: This is used by the InternalName remote validator.  Saves us from having the DB hammered as every keystroke invokes the action.

        [BindProperty]
        public int OrganizationId { get; set; }
        [Required(ErrorMessage = "REQUIRED")]
        [BindProperty]
        public int TemplateTypeId { get; set; }
        [BindProperty]
        public IFormFile PdfContents { get; set; }
        [Required(ErrorMessage = "REQUIRED")]
        [StringLength(100, ErrorMessage = "Internal Name must be 100 characters or less.")]
        [PageRemote(PageHandler = "InternalNameValidation", HttpMethod = "POST", AdditionalFields = "ExistingTemplateNames,__RequestVerificationToken", ErrorMessage = "The name you have chosen is already in use.")]
        [BindProperty]
        public string InternalName { get; set; }

        public CreateModel
        (
            IApplicationService securityService,
            IDocumentServicesProvider documentService,
            IBinaryStorageService storageService
        )
        {
            SecurityService = securityService;
            DocumentService = documentService;
            StorageService = storageService;
        }

        public async Task<IActionResult> OnGet(int? organizationId)
        {
            RequireClientHeader = SecurityService.ShowLegacyNavBar(CurrentUser.IsUserSender(), CurrentUser.MemberId);
            if (CurrentUser.IsApplicationAdministrator())
            {
                if (!organizationId.HasValue)
                {
                    return RedirectToRoute("AdminOrganizationIndex");
                }

                Offices = null;
                OrganizationId = organizationId.Value;
            }
            else
            {
                Offices = await GetOfficeSelections();
                OrganizationId = Offices.Count() == 1 ? Offices.First().OrganizationId : Offices.FirstOrDefault(o => o.OrganizationId == (organizationId ?? o.OrganizationId))?.OrganizationId ?? 0;
            }
            ExistingTemplateNames = string.Join("\n", (await DocumentService.GetActiveTemplatesByOrganizationIdAsync(OrganizationId)).Select(t => t.Name));

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var templateTypes = await DocumentService.GetPdfTemplateTypesByOrganizationIdAsync(OrganizationId);
            var existingTemplates = await DocumentService.GetActiveTemplatesByOrganizationIdAsync(OrganizationId);
            var pdfBytes = Array.Empty<byte>();

            if (!CurrentUser.IsApplicationAdministrator())
            {
                Offices = await GetOfficeSelections();

                if (!Offices.Select(o => o.OrganizationId).Contains(OrganizationId))
                {
                    ModelState.AddModelError("OrganizationId", "REQUIRED");
                }
            }

            if (templateTypes.FirstOrDefault(tt => tt.TemplateTypeId == TemplateTypeId) == null)
            {
                ModelState.AddModelError("TemplateTypeId", "REQUIRED");
            }
            if (existingTemplates.Select(t => t.Name).Contains((InternalName ?? string.Empty).Trim(), StringComparer.CurrentCultureIgnoreCase))
            {
                ModelState.AddModelError("InternalName", "The name you have chosen is already in use.");
            }
            
            if (PdfContents != null)
            {
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    await PdfContents.CopyToAsync(memoryStream);
                    pdfBytes = memoryStream.ToArray();
                }
            }
            if (!pdfBytes.Any())
            {
                ModelState.AddModelError("PdfContents", "REQUIRED");
            }
            if (pdfBytes.Length > MAX_UPLOAD_BYTES)
            {
                ModelState.AddModelError("PdfContents", "The file you have uploaded is too large.");
            }
            if (pdfBytes.Any() && !DocumentService.ValidatePdf(pdfBytes))
            {
                ModelState.AddModelError("PdfContents", "The file you have uploaded could not be read.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var templateId = await DocumentService.CreateOrganizationTemplateAsync(
                OrganizationId,
                InternalName,
                TemplateTypeId,
                await StorageService.SaveToBinaryStorageAsync(BinaryStorageType.Templates, pdfBytes),
                CurrentUser.Id);

            return RedirectToRoute("TemplateAnnotationEdit", new { templateId = templateId });
        }

        public async Task<IActionResult> OnGetTemplateTypes(int organizationId)
        {
            var validOrganizationIds = CurrentUser.IsApplicationAdministrator() ? new int[] { organizationId } : (await GetOfficeSelections()).Select(o => o.OrganizationId);

            if (!validOrganizationIds.Contains(organizationId))
            {
                return BadRequest();
            }

            return new JsonResult(new
            {
                Data = (await DocumentService.GetPdfTemplateTypesByOrganizationIdAsync(organizationId)).Select(tt => new TemplateType()
                {
                    TemplateTypeId = tt.TemplateTypeId,
                    Category = tt.CategoryName,
                    Name = tt.Name,
                    ShortName = tt.ShortName
                }).OrderBy(tt => tt.Category).ThenBy(tt => tt.Name)
            });
        }

        public JsonResult OnPostInternalNameValidation([FromForm] string existingTemplateNames)
        {
            return new JsonResult(!(existingTemplateNames ?? string.Empty).Split("\n").Contains(InternalName, StringComparer.InvariantCultureIgnoreCase));
        }

        protected async Task<IEnumerable<Office>> GetOfficeSelections()
            => await SecurityService.GetOrganizationMembersByMemberId(CurrentUser.Id)
                                    .Where(om => om.IsActive)
                                    .Select(om => new Office()
            {
                OrganizationId = om.OrganizationId,
                Name = string.IsNullOrWhiteSpace(om.Organization.OtherDesignation) ? om.Organization.Name : om.Organization.OtherDesignation
            })
            .OrderBy(sli => sli.Name)
            .ToArrayAsync();

        public class Office
        {
            public int OrganizationId { get; set; }
            public string Name { get; set; }
        }

        public class TemplateType
        {
            public int TemplateTypeId { get; set; }
            public string Name { get; set; }
            public string ShortName { get; set; }
            public string Category { get; set; }
        }
    }
}
