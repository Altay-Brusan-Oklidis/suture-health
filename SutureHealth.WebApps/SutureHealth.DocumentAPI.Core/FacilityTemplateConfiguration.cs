using System;

namespace SutureHealth.Documents
{
    public class FacilityTemplateConfiguration
    {
        public int TemplateConfigurationId { get; set; }
        public int TemplateId { get; set; }
        public string DocumentTypeKey { get; set; }
        public int TemplateProcessingModeId { get; set; }
        public bool Enabled { get; set; }
        public int? OCRDocumentId { get; set; }
        public bool OCRAnalysisAvailable { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? DateModified { get; set; }
        public string ModifiedBy { get; set; }

        // Extended properties from SutureSignWeb
        public string TemplateName { get; set; }
        public string Category { get; set; }
        public int FacilityId { get; set; }
    }
}
