using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SutureHealth.AspNetCore.Areas.Template.Models.Annotation;
using SutureHealth.AspNetCore.Mvc.Attributes;
using SutureHealth.Documents.Services;
using SutureHealth.Imaging;
using SutureHealth.Requests.Services;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Template.Controllers
{
    [Area("Template")]
    [Route("Template/{templateId:int}")]
    public class AnnotationController : Controller
    {
        protected IDocumentServicesProvider DocumentService { get; set; }
        protected IRequestServicesProvider RequestService { get; set; }

        public AnnotationController
        (
            IDocumentServicesProvider documentService,
            IRequestServicesProvider requestService
        )
        {
            DocumentService = documentService;
            RequestService = requestService;
        }

        [HttpGet]
        [RequireAuthorizedTemplate]
        [Route("Annotations", Name = "TemplateAnnotationEdit")]
        public async Task<IActionResult> Edit(SutureHealth.Documents.Template template)
        {
            var isParentTemplate = !template.ParentTemplateId.HasValue;
            if (!isParentTemplate && (await RequestService.GetServiceableRequests().Where(r => r.TemplateId == template.TemplateId).AnyAsync()))
            {
                return BadRequest();
            }

            return View(new EditViewModel()
            {
                CurrentUser = CurrentUser,
                OrganizationId = template.OrganizationId,
                TemplateId = template.TemplateId,
                TemplateName = template.Name,
                IsParentTemplate = isParentTemplate,
                Editor = new EditorViewModel(template, (await DocumentService.GetTemplatePngsWithDimensionsByIdAsync(template.TemplateId, 100)).Item1),
                SaveReturnUrl = isParentTemplate ?
                                    (CurrentUser.IsApplicationAdministrator() ? Url.RouteUrl("AdminTemplateOrganizationSearch", new { organizationId = template.OrganizationId }) : Url.RouteUrl("SendIndex", new { contentOnly = true })) :
                                    Url.RouteUrl("SendIndex", new { templateId = template.TemplateId, contentOnly = true }),
                CancelReturnUrl = CurrentUser.IsApplicationAdministrator() ?
                                    Url.RouteUrl("AdminTemplateOrganizationSearch", new { organizationId = template.OrganizationId }) :
                                    Url.RouteUrl("SendIndex", new { contentOnly = true }), 
                RequireClientHeader = false
            });
        }

        [HttpPost]
        [RequireAuthorizedTemplate]
        [Route("Annotations", Name = "TemplateAnnotationSave")]
        public async Task<IActionResult> Save(SutureHealth.Documents.Template template, [FromBody] EditPostModel model)
        {
            var errors = new List<string>();

            if (template.ParentTemplateId.HasValue && (await RequestService.GetServiceableRequests().Where(r => r.TemplateId == template.TemplateId).AnyAsync()))
            {
                errors.Add("This template is associated with a request and cannot be modified.");
            }
            if (model?.Annotations == null || !model.Annotations.Any(a => a.Type == Documents.AnnotationType.VisibleSignature))
            {
                errors.Add("A document must contain at least one signature field.");
            }

            if (errors.Any())
            {
                return Json(new SaveResponse()
                {
                    Errors = errors
                });
            }

            await DocumentService.CreateAnnotationsAsync(new CreateAnnotationsRequest()
            {
                TemplateId = template.TemplateId,
                ActivateTemplate = true,
                Annotations = model.Annotations.Select(a => new CreateAnnotationsRequest.Annotation()
                {
                    Type = a.Type,
                    Top = a.Top,
                    Left = a.Left,
                    Bottom = a.Top + a.Height,
                    Right = a.Left + a.Width,
                    PageHeight = a.PageHeight,
                    PageNumber = a.PageNumber,
                    Value = a.Value
                })
            });

            return Json(new SaveResponse()
            {
                Errors = Array.Empty<string>()
            });
        }

        [HttpGet]
        [RequireAuthorizedTemplate]
        [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
        [Route("Ocr", Name = "TemplateAnnotationOcrEdit")]
        public async Task<IActionResult> OcrEdit(SutureHealth.Documents.Template template)
        {
            var ocrDocument = await DocumentService.GetOcrDocumentResultByTemplateIdAsync(template.TemplateId);
            var templatePageImages = ocrDocument != null ? await DocumentService.GetTemplatePngsWithDimensionsByIdAsync(template.TemplateId, 100) : (null, null);
            if (ocrDocument == null)
            {
                return BadRequest();
            }

            return View(new OcrEditViewModel()
            {
                CurrentUser = CurrentUser,
                TemplateId = template.TemplateId,
                TemplatePages = templatePageImages.Item1.Select((img, i) => new OcrEditViewModel.TemplatePage()
                {
                    ImageBase64 = Convert.ToBase64String(img),
                    PageNumber = i + 1
                }),
                Annotations = (await DocumentService.GetTemplateAnnotationOcrMappingsByTemplateId(template.TemplateId)).Select(m =>
                {
                    var ocrResult = SearchOcrDocument(m.SearchText, ocrDocument).FirstOrDefault();

                    // Don't display annotations that can't be mapped onto a page
                    if (ocrResult == null)
                    {
                        return null;
                    }

                    var pageSize = templatePageImages.Item2[ocrResult.PageNumber - 1];

                    return new OcrAnnotation()
                    {
                        SearchText = m.SearchText,
                        MatchAll = m.MatchAll,
                        Type = (OcrAnnotation.AnnotationType)m.AnnotationFieldTypeId,
                        PageNumber = ocrResult.PageNumber,
                        TopPixel = (int)((ocrResult.BoundingBox.Top + m.OffsetY) * pageSize.Height),
                        LeftPixel = (int)((ocrResult.BoundingBox.Left + m.OffsetX) * pageSize.Width),
                        HeightPixels = (int)(m.Height * pageSize.Height),
                        WidthPixels = (int)(m.Width * pageSize.Width)
                    };
                }).Where(r => r != null),
                ReturnUrl = Url.RouteUrl("AdminTemplateOrganizationSearch", new { organizationId = template.OrganizationId })
            });
        }

        [HttpPost]
        [RequireAuthorizedTemplate]
        [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
        [Route("Ocr", Name = "TemplateAnnotationOcrSave")]
        public async Task<IActionResult> OcrSave(SutureHealth.Documents.Template template, [FromBody] IEnumerable<OcrAnnotation> annotations)
        {
            List<string> errors = new List<string>();
            var pageSizes = (await DocumentService.GetTemplatePngsWithDimensionsByIdAsync(template.TemplateId)).Item2;
            var ocrDocument = await DocumentService.GetOcrDocumentResultByTemplateIdAsync(template.TemplateId);
            var ocrResultsByAnnotation = (annotations ?? new OcrAnnotation[0]).Select(a =>
            {
                var ocrResult = SearchOcrDocument(a.SearchText, ocrDocument).FirstOrDefault();

                if (ocrResult == null)
                {
                    errors.Add($"The search string '{a.SearchText}' could not be found in the document.");
                    return new KeyValuePair<OcrAnnotation, IOcrResult>(a, null);
                }

                if (ocrResult.PageNumber != a.PageNumber)
                {
                    errors.Add($"The search string '{a.SearchText}' was found, but the annotation is defined on page {a.PageNumber} while the first occurrence of the search string was found on page {ocrResult.PageNumber}.");
                    return new KeyValuePair<OcrAnnotation, IOcrResult>(a, null);
                }

                return new KeyValuePair<OcrAnnotation, IOcrResult>(a, ocrResult);
            }).ToArray();

            if (errors.Any())
            {
                return Json(new OcrSaveResponse()
                {
                    Success = false,
                    Errors = errors
                });
            }

            await DocumentService.SaveTemplateAnnotationOcrMappingsAsync(template.TemplateId, ocrResultsByAnnotation.Select(r => new SutureHealth.Documents.AnnotationOCRMapping()
            {
                SearchText = r.Key.SearchText,
                AnnotationFieldTypeId = (int)r.Key.Type,
                Height = (decimal)r.Key.HeightPixels / pageSizes[r.Key.PageNumber - 1].Height,
                Width = (decimal)r.Key.WidthPixels / pageSizes[r.Key.PageNumber - 1].Width,
                OffsetY = ((decimal)r.Key.TopPixel / pageSizes[r.Key.PageNumber - 1].Height) - r.Value.BoundingBox.Top,
                OffsetX = ((decimal)r.Key.LeftPixel / pageSizes[r.Key.PageNumber - 1].Width) - r.Value.BoundingBox.Left,
                MatchAll = r.Key.MatchAll
            }));

            return Json(new OcrSaveResponse()
            {
                Success = true
            });
        }

        [HttpPost]
        [RequireAuthorizedTemplate]
        [Authorize(AuthorizationPolicies.ApplicationAdministrator)]
        [Route("Ocr/Query", Name = "TemplateAnnotationOcrQuery")]
        public async Task<JsonResult> OcrQuery(SutureHealth.Documents.Template template, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return Json(new OcrQueryResponse() { ResultFound = false });
            }
            searchText = System.Web.HttpUtility.UrlDecode(searchText);  // Manually decode as we encode on the client to avoid .NET form validation

            var ocrDocument = await DocumentService.GetOcrDocumentResultByTemplateIdAsync(template.TemplateId);
            var ocrResults = SearchOcrDocument(searchText, ocrDocument);
            var pageSizes = (await DocumentService.GetTemplatePngsWithDimensionsByIdAsync(template.TemplateId, 100)).Item2.ToArray();

            return Json(new OcrQueryResponse()
            {
                ResultFound = ocrResults.Any(),
                BindingResult = GetOcrQueryResult(ocrResults.FirstOrDefault(), pageSizes),
                OtherResults = ocrResults.Skip(1).Select(r => GetOcrQueryResult(r, pageSizes))
            });
        }

        protected IEnumerable<IOcrResult> SearchOcrDocument(string searchText, IOcrDocument ocrDocument)
        {
            return ocrDocument.TextWhere(t => t.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        protected OcrQueryResult GetOcrQueryResult(SutureHealth.Imaging.IOcrResult ocrResult, System.Drawing.Size[] pageSizes)
        {
            if (ocrResult == null || pageSizes == null)
            {
                return null;
            }

            var pageSize = pageSizes[ocrResult.PageNumber - 1];

            return new OcrQueryResult()
            {
                PageNumber = ocrResult.PageNumber,
                TopPixel = (int)(ocrResult.BoundingBox.Top * pageSize.Height),
                LeftPixel = (int)(ocrResult.BoundingBox.Left * pageSize.Width),
                HeightPixels = (int)((ocrResult.BoundingBox.Bottom - ocrResult.BoundingBox.Top) * pageSize.Height),
                WidthPixels = (int)((ocrResult.BoundingBox.Right - ocrResult.BoundingBox.Left) * pageSize.Width)
            };
        }
    }
}
