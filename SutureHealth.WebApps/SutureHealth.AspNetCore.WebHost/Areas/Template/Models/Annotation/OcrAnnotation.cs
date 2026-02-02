namespace SutureHealth.AspNetCore.Areas.Template.Models.Annotation
{
    public class OcrAnnotation
    {
        public int PageNumber { get; set; }
        public string SearchText { get; set; }
        public int TopPixel { get; set; }
        public int LeftPixel { get; set; }
        public int HeightPixels { get; set; }
        public int WidthPixels { get; set; }
        public AnnotationType Type { get; set; }
        public bool MatchAll { get; set; }

        public enum AnnotationType
        {
            Unknown = 0,
            VisibleSignature = 1,
            DateSigned = 2
        }
    }
}
