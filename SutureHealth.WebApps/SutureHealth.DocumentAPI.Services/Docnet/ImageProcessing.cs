using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using ImageMagick;

namespace SutureHealth.Documents.Services.Docnet
{
    public static class ImageProcessing
    {
        public static byte[][] SavePdfToPng(byte[] pdfBytes, int dpi, out Size[] dimensions)
        {
            using (var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(dpi * Constants.IMAGE_DPI_SCALING_MULTIPLIER)))
            {
                var images = Enumerable.Range(0, docReader.GetPageCount()).Select(i => PdfToPng(docReader, i));

                dimensions = images.Select(img => img.Item2).ToArray();
                return images.Select(img => img.Item1).ToArray();
            }
        }

        public static byte[] SavePdfPageToPng(byte[] pdfBytes, int pageIndex, int dpi, out Size dimensions)
        {
            using (var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(dpi * Constants.IMAGE_DPI_SCALING_MULTIPLIER)))
            {
                var image = PdfToPng(docReader, pageIndex);

                dimensions = image.Item2;
                return image.Item1;
            }
        }

        public static byte[] SavePdfToTiff(byte[] pdfBytes, int dpi)
        {
            using (var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(dpi * Constants.IMAGE_DPI_SCALING_MULTIPLIER)))
            {
                return PdfToTiff(docReader);
            }
        }

        public static byte[] CombinePdfsToTiff(IEnumerable<byte[]> pdfs, int dpi)
        {
            byte[] result;

            using (var collection = new MagickImageCollection())
            {
                foreach (var pdfBytes in pdfs)
                {
                    using (var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(dpi * Constants.IMAGE_DPI_SCALING_MULTIPLIER)))
                    using (var images = new MagickImageCollection(PdfToTiff(docReader), MagickFormat.Tiff))
                    {
                        collection.AddRange(images.ToArray());
                    }
                }

                result = collection.ToByteArray(MagickFormat.Tiff);
            }

            return result;
        }

        public static byte[] RasterizePdf(byte[] pdfBytes)
        {
            int dpi = 150;
            byte[] result;

            using (var reader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(dpi * Constants.IMAGE_DPI_SCALING_MULTIPLIER)))
            using (var collection = new MagickImageCollection())
            {
                var images = Enumerable.Range(0, reader.GetPageCount()).Select(i =>
                {
                    var image = OpenPdfPage(reader, i);

                    image.Density = new Density(dpi, DensityUnit.PixelsPerInch);

                    return image;
                });

                collection.AddRange(images);
                result = collection.ToByteArray(MagickFormat.Pdfa);

                foreach (var image in images)
                {
                    image.Dispose();
                }
            }

            return result;
        }

        private static (byte[], Size) PdfToPng(IDocReader reader, int pageIndex)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (var image = OpenPdfPage(reader, pageIndex))
            {
                image.Write(memoryStream, MagickFormat.Png);

                return (memoryStream.ToArray(), new Size(image.Width, image.Height));
            }
        }

        private static byte[] PdfToTiff(IDocReader reader)
        {
            byte[] result;

            using (var collection = new MagickImageCollection())
            {
                var images = Enumerable.Range(0, reader.GetPageCount()).Select(i => OpenPdfPage(reader, i));

                collection.AddRange(images);
                result = collection.ToByteArray(MagickFormat.Tiff);

                foreach (var image in images)
                {
                    image.Dispose();
                }
            }

            return result;
        }

        #region Docnet PDF to MagickImage Conversion
        // https://stackoverflow.com/a/62585392
        private static MagickImage OpenPdfPage(IDocReader reader, int pageIndex)
        {
            MagickImage imgBackdrop;
            MagickColor backdropColor = MagickColors.White; // replace transparent pixels with this color 

            using (var pageReader = reader.GetPageReader(pageIndex))
            {
                var rawBytes = pageReader.GetImage(); // Returns image bytes as B-G-R-A ordered list.
                rawBytes = RearrangeBytesToRGBA(rawBytes);
                var width = pageReader.GetPageWidth();
                var height = pageReader.GetPageHeight();

                // specify that we are reading a byte array of colors in R-G-B-A order.
                PixelReadSettings pixelReadSettings = new PixelReadSettings(width, height, StorageType.Char, PixelMapping.RGBA);
                using (MagickImage imgPdfOverlay = new MagickImage(rawBytes, pixelReadSettings))
                {
                    // turn transparent pixels into backdrop color using composite: http://www.imagemagick.org/Usage/compose/#compose
                    imgBackdrop = new MagickImage(backdropColor, width, height);
                    imgBackdrop.Composite(imgPdfOverlay, CompositeOperator.Over);
                }
            }

            return imgBackdrop;
        }

        private static byte[] RearrangeBytesToRGBA(byte[] BGRABytes)
        {
            var max = BGRABytes.Length;
            var RGBABytes = new byte[max];
            var idx = 0;
            byte r, g, b, a;

            while (idx < max)
            {
                // get colors in original order: B G R A
                b = BGRABytes[idx];
                g = BGRABytes[idx + 1];
                r = BGRABytes[idx + 2];
                a = BGRABytes[idx + 3];

                // re-arrange to be in new order: R G B A
                RGBABytes[idx] = r;
                RGBABytes[idx + 1] = g;
                RGBABytes[idx + 2] = b;
                RGBABytes[idx + 3] = a;

                idx += 4;
            }
            return RGBABytes;
        }
        #endregion
    }
}
