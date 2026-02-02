using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Template.Models.Annotation
{
    public class OcrEditViewModel : BaseViewModel
    {
        public int TemplateId { get; set; }
        public IEnumerable<TemplatePage> TemplatePages { get; set; }
        public IEnumerable<OcrAnnotation> Annotations { get; set; }
        public string ReturnUrl { get; set; }

        public class TemplatePage
        {
            public int PageNumber { get; set; }
            public string ImageBase64 { get; set; }
            public int ImageWidthPixels { get; set; }
            public int ImageHeightPixels { get; set; }
        }
    }
}
