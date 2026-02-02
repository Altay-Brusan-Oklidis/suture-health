using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SutureHealth.Documents.Testing
{
    [TestClass]
    public class ImageProcessing
    {
        [TestCategory("Integration")]
        [TestMethod]
        public async Task ConvertPdfToTiff()
        {
            var pdfBytes = File.ReadAllBytes(@"C:\Users\mculotta\Downloads\cms484.pdf");

            File.WriteAllBytes(@"C:\Users\mculotta\Desktop\cms484.tiff", SutureHealth.Documents.Services.Docnet.ImageProcessing.SavePdfToTiff(pdfBytes, 1));
        }

        [TestCategory("Integration")]
        [TestMethod]
        public async Task CombinePdfsToTiff()
        {
            var pdfBytes = File.ReadAllBytes(@"C:\Users\mculotta\Downloads\cms484.pdf");
            File.WriteAllBytes(@"C:\Users\mculotta\Desktop\combined.tiff", SutureHealth.Documents.Services.Docnet.ImageProcessing.CombinePdfsToTiff(new byte[][] { pdfBytes, pdfBytes }, 1));
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void CreatePngFromPdfPage()
        {
            var pdfBytes = File.ReadAllBytes(@"C:\Users\mculotta\Downloads\cms484.pdf");

            File.WriteAllBytes(@"C:\Users\mculotta\Desktop\scaling.png", SutureHealth.Documents.Services.Docnet.ImageProcessing.SavePdfPageToPng(pdfBytes, 0, 100, out _));
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void RasterizePdf()
        {
            var pdfBytes = File.ReadAllBytes(@"C:\Users\mculotta\Downloads\cms484.pdf");

            File.WriteAllBytes(@"C:\Users\mculotta\Desktop\rasterized.pdf", SutureHealth.Documents.Services.Docnet.ImageProcessing.RasterizePdf(pdfBytes));
        }
    }
}
