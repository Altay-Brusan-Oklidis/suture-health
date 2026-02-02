namespace SutureHealth.AspNetCore.Areas.Template.Models.Annotation
{
    public class OcrQueryResult
    {
        public int PageNumber { get; set; }
        public int TopPixel { get; set; }
        public int LeftPixel { get; set; }
        public int HeightPixels { get; set; }
        public int WidthPixels { get; set; }
    }
}
