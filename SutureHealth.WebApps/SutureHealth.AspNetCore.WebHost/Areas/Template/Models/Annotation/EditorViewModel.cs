namespace SutureHealth.AspNetCore.Areas.Template.Models.Annotation
{
    public class EditorViewModel
    {
        public EditorViewModel() { }

        public EditorViewModel(Documents.Template template, IList<byte[]> pageImages)
        {
            Pages = pageImages.Select((img, i) => new Page()
            {
                PageNumber = i + 1,
                Base64Image = Convert.ToBase64String(img)
            });
            Annotations = template.Annotations.Select(a => new Annotation()
            {
                AnnotationId = a.TemplateAnnotationId,
                PageNumber = a.PageNumber.GetValueOrDefault(),
                Value = a.Value,
                Type = a.AnnotationType,
                Left = a.HtmlCoordinateLeft.GetValueOrDefault(),
                Top = a.HtmlCoordinateTop.GetValueOrDefault(),
                Width = a.HtmlCoordinateRight.GetValueOrDefault() - a.HtmlCoordinateLeft.GetValueOrDefault(),
                Height = a.HtmlCoordinateBottom.GetValueOrDefault() - a.HtmlCoordinateTop.GetValueOrDefault()
            });
        }

        public bool TextAreaValidationEnabled { get; set; }
        public IEnumerable<Page> Pages { get; set; }
        public IEnumerable<Annotation> Annotations { get; set; }

        public class Page
        {
            public int PageNumber { get; set; }
            public string Base64Image { get; set; }
        }

        public class Annotation
        {
            public int AnnotationId { get; set; }
            public SutureHealth.Documents.AnnotationType Type { get; set; }
            public int PageNumber { get; set; }
            public int Left { get; set; }
            public int Width { get; set; }
            public int Top { get; set; }
            public int Height { get; set; }
            public string Value { get; set; }
        }

        public class CalculatedAnnotation : Annotation
        {
            public int PageHeight { get; set; }
        }
    }
}
