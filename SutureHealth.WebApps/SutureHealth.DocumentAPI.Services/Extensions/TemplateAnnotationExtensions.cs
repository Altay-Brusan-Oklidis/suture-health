using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.Model.Data;
using Telerik.Windows.Documents.Fixed.Model.Editing;

namespace SutureHealth.Documents.Services.Extensions
{
    public static class TemplateAnnotationExtensions
    {
        public static FixedContentEditor GetPdfEditor(this TemplateAnnotation annotation, RadFixedDocument pdfContent)
        {
            var position = new SimplePosition();

            if (annotation.PageHeight.GetValueOrDefault() > 0)
            {
                var page = pdfContent.Pages[annotation.PageNumber.Value - 1];

                position.Translate(annotation.HtmlCoordinateLeft.Value * Constants.HTML_WIDTH_SCALING_MULTIPLIER, annotation.HtmlCoordinateTop.Value * Constants.HTML_HEIGHT_SCALING_MULTIPLIER);

                return new FixedContentEditor(page, position);
            }
            else
            {
                int currentPageIndex = 0,
                    top = (int)(annotation.PdfCoordinateTop.Value * Constants.ABCPDF_TO_TELERIK_SCALING_MULTIPLIER),
                    bottom = (int)(annotation.PdfCoordinateBottom.Value * Constants.ABCPDF_TO_TELERIK_SCALING_MULTIPLIER);

                while (top < 0 && currentPageIndex + 1 < pdfContent.Pages.Count)
                {
                    currentPageIndex += 1;

                    top += (int)pdfContent.Pages[currentPageIndex].Size.Height;
                    bottom += (int)pdfContent.Pages[currentPageIndex].Size.Height;
                }

                if (bottom < 0)
                {
                    top += -bottom;
                }

                position.Translate(annotation.PdfCoordinateLeft.Value * Constants.ABCPDF_TO_TELERIK_SCALING_MULTIPLIER, pdfContent.Pages[currentPageIndex].Size.Height - top);

                return new FixedContentEditor(pdfContent.Pages[currentPageIndex], position);
            }
        }
    }
}
