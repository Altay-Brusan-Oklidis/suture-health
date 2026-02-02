namespace SutureHealth.Documents
{
    public class AnnotationOCRMapping
    {
        public int AnnotationOCRMappingId { get; set; }
        public int TemplateConfigurationId { get; set; }
        public int AnnotationFieldTypeId { get; set; }
        public string SearchText { get; set; }
        public decimal OffsetX { get; set; }
        public decimal OffsetY { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public bool MatchAll { get; set; }

        public TemplateConfiguration TemplateConfiguration { get; set; }
    }
}
