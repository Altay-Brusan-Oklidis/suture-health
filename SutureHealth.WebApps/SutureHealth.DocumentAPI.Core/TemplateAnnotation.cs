namespace SutureHealth.Documents
{
    public class TemplateAnnotation
    {
        public int TemplateAnnotationId { get; set; }
        public int TemplateId { get; set; }
        public AnnotationType AnnotationType { get; set; }
        public int? PdfCoordinateLeft { get; set; }
        public int? PdfCoordinateBottom { get; set; }
        public int? PdfCoordinateRight { get; set; }
        public int? PdfCoordinateTop { get; set; }
        public int? HtmlCoordinateLeft { get; set; }
        public int? HtmlCoordinateBottom { get; set; }
        public int? HtmlCoordinateRight { get; set; }
        public int? HtmlCoordinateTop { get; set; }
        public int? PageNumber { get; set; }
        public string Value { get; set; }
        public int? PageHeight { get; set; }

        public Template Template { get; set; }
    }

    public enum AnnotationType
    {
        Unknown = 0,
        VisibleSignature = 1,
        DateSigned = 2,
        CheckBox = 3,
        TextArea = 4
    }
}
