using System;
using System.Collections.Generic;

namespace SutureHealth.Documents
{
    public class TemplateConfiguration
    {
        public TemplateConfiguration()
        {
            AnnotationOcrMappings = new List<AnnotationOCRMapping>();
        }

        public int TemplateConfigurationId { get; set; }
        public int TemplateId { get; set; }
        public string DocumentTypeKey { get; set; }
        public int TemplateProcessingModeId { get; set; }
        public bool Enabled { get; set; }
        public OCRDocument OCRDocument { get; set; }
        public int? OCRDocumentId { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? DateModified { get; set; }
        public string ModifiedBy { get; set; }

        public TemplateProcessingMode TemplateProcessingMode
        {
            get
            {
                return Enum.IsDefined(typeof(TemplateProcessingMode), this.TemplateProcessingModeId) ? (TemplateProcessingMode)this.TemplateProcessingModeId : TemplateProcessingMode.Unknown;
            }
        }

        public IList<AnnotationOCRMapping> AnnotationOcrMappings { get; set; }
    }
}