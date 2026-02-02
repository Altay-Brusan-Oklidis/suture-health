namespace SutureHealth.AspNetCore.Areas.Template.Models.Annotation
{
    public class OcrSaveResponse
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
