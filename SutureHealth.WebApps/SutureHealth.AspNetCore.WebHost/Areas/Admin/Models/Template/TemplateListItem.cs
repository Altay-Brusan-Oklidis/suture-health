using Microsoft.AspNetCore.Mvc;
using SutureHealth.Documents;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Template
{
    public class TemplateListItem
    {
        public TemplateListItem() { }

        public TemplateListItem(FacilityTemplateConfiguration template, IUrlHelper helper)
        {
            TemplateId = template.TemplateId;
            Name = template.TemplateName;
            IsApiEnabled = template.Enabled;
            ApiDocumentKey = template.DocumentTypeKey;
            TemplateProcessingModeId = template.TemplateProcessingModeId;
            OcrStatus = template switch
            {
                _ when template.TemplateProcessingModeId != 2 => OcrAnalysisStatus.NotApplicable,
                _ when !template.OCRDocumentId.HasValue => OcrAnalysisStatus.Pending,
                _ when template.OCRAnalysisAvailable => OcrAnalysisStatus.Available,
                _ when !template.OCRAnalysisAvailable => OcrAnalysisStatus.InProgress,
                _ => OcrAnalysisStatus.NotApplicable
            };
            ParentCoordinatesEditUrl = helper.RouteUrl("TemplateAnnotationEdit", new { templateId = template.TemplateId });
            DdpEditUrl = helper.RouteUrl("TemplateAnnotationOcrEdit", new { templateId = template.TemplateId });
            OcrScanActionUrl = helper.RouteUrl("AdminTemplateScanTemplate", new { templateId = template.TemplateId });
        }

        public int TemplateId { get; set; }
        public string Name { get; set; }
        public bool IsApiEnabled { get; set; }
        public string ApiDocumentKey { get; set; }
        public int TemplateProcessingModeId { get; set; }
        public string DdpEditUrl { get; set; }
        public string ParentCoordinatesEditUrl { get; set; }
        public string OcrScanActionUrl { get; set; }
        public string AnnotationSource => TemplateProcessingModeId switch
        {
            1 => "Parent Coordinates",
            2 => $"DDP{(OcrStatus == OcrAnalysisStatus.InProgress ? " (Scanning...)" : string.Empty)}",
            _ => "Unknown"
        };
        public string AnnotationSourceUrl => TemplateProcessingModeId switch
        {
            1 => ParentCoordinatesEditUrl,
            2 => OcrStatus == OcrAnalysisStatus.Available ? DdpEditUrl : null,
            _ => null
        };
        public bool HasAnnotationSourceUrl => !string.IsNullOrWhiteSpace(AnnotationSourceUrl);
        public OcrAnalysisStatus OcrStatus { get; set; }
        public bool CanOcrScan => OcrStatus == OcrAnalysisStatus.Pending;

        public enum OcrAnalysisStatus
        {
            NotApplicable = 0,
            Pending,
            InProgress,
            Available
        }
    }
}
