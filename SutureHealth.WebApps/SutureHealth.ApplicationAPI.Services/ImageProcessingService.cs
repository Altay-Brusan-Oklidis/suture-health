using SutureHealth.Application.Services;
using System;
using System.Collections.Generic;
using System.IO;
using ImageFormat = SutureHealth.Application.Services.ImageFormat;
using Microsoft.AspNetCore.Http;
using SutureHealth.Application;
using ImageMagick;

namespace SutureHealth.Security
{
    public class ImageProcessingService : IImageProcessingService
    {
        public IFormFile CompressImage(IFormFile imageFile, int maxWidth, int maxHeight, int quality)
        {
            var format = GetImageFormat(imageFile);
            using var inputMemoryStream = imageFile.OpenReadStream();
            using var image = new MagickImage(inputMemoryStream);

            if (maxWidth != 0 && maxHeight != 0)
            {
                image.Resize(maxWidth, maxHeight);
            }

            using var outputMemoryStream = new MemoryStream();
            image.Quality = quality;

            if (format == ImageFormat.Jpeg)
                image.Write(outputMemoryStream, MagickFormat.Jpeg);
            else if (format == ImageFormat.Jpg)
                image.Write(outputMemoryStream, MagickFormat.Jpg);
            else if (format == ImageFormat.Png)
                image.Write(outputMemoryStream, MagickFormat.Png);
            else if (format == ImageFormat.Heif || format == ImageFormat.Heic)
                return imageFile;
            else
                throw new ArgumentException("Unsupported image format");

            var formFile = ConvertByteArrayToFormFile(outputMemoryStream.ToArray(), imageFile.FileName, imageFile.ContentType); ;
            return formFile;
        }

        public string ToBase64(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null; 
            }

            using var memoryStream = new MemoryStream();
            file.CopyTo(memoryStream);

            var bytes = memoryStream.ToArray();

            string base64String = Convert.ToBase64String(bytes);

            return base64String;
        }

        public System.Drawing.Size GetImageResolution(IFormFile imageFile)
        {
            using var stream = imageFile.OpenReadStream();
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            ReadOnlySpan<byte> imageFileByteSpan = memoryStream.ToArray().AsSpan();
            using (var magickImage = new MagickImage(imageFileByteSpan))
            {
                var result = new System.Drawing.Size(magickImage.Width, magickImage.Height);
                return result;
            }
        }        

        public bool IsImageResolutionValid(IFormFile imageFile, int minWidth, int minHeight, int maxWidth, int maxHeight)
        {
            var imageResolution = GetImageResolution(imageFile);

            if (imageResolution.Width < minWidth || imageResolution.Height < minHeight)
                return false;
            if ((imageResolution.Width > maxWidth || imageResolution.Height > maxHeight))
                return false;

            return true;
        }

        public bool IsImageResolutionValidForMobile(IFormFile imageFile, int minWidth, int minHeight)
        {
            var imageResolution = GetImageResolution(imageFile);

            if (imageResolution.Width < minWidth || imageResolution.Height < minHeight)
                return false;            

            return true;
        }

        public bool IsImageTypeValid(IFormFile imageFile, List<ImageFormat> expectedFormats)
        {
            var imgType = GetImageFormat(imageFile);

            return expectedFormats.Contains(imgType);
        }

        public IFormFile ConvertByteArrayToFormFile(byte[] byteArray, string fileName, string contentType)
        {
            var stream = new MemoryStream(byteArray);
            return new FormFile(stream, 0, byteArray.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        public long GetImageSize(IFormFile imageFile)
        {
            if (imageFile == null)
                return 0;

            return imageFile.Length;
        }

        public ImageFormat GetImageFormat(IFormFile imageFile)
        {
            try
            {
                using var stream = imageFile.OpenReadStream();
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);

                ReadOnlySpan<byte> imageFileByteSpan = memoryStream.ToArray().AsSpan();

                using (var magickImage = new MagickImage(imageFileByteSpan))
                {
                    var format = magickImage.Format.ToString().ToLower();

                    if (format == "jpeg")
                        return ImageFormat.Jpeg;
                    if (format == "jpg")
                        return ImageFormat.Jpg;
                    if (format == "png")
                        return ImageFormat.Png;
                    if (format == "heif")
                        return ImageFormat.Heif;
                    if (format == "heic")
                        return ImageFormat.Heic;

                    return ImageFormat.Unknown;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting image format: {ex.Message}");
                return ImageFormat.Unknown;
            }
        }       

        public bool IsImageFileSizeValid(IFormFile imageFile, int maxByteSize)
        {
            return imageFile.Length <= maxByteSize;
        }

        public bool IsImageFileEmptyOrNull(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return true;

            return false;
        }

        //public bool IsImageTypeValid(IFormFile imageFile, string[] allowedFileTypes)
        //{
        //    return allowedFileTypes.Contains(imageFile.ContentType.ToLower());
        //}

    }


    public static class FormFileExtensions
    {
        public static byte[] ToByteArray(this ImageStream imageStream)
        {
            if (imageStream == null)
                return null;

            if (imageStream.Stream == null)
                return null;

            var memoryStream = new MemoryStream();
            imageStream.Stream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }
    }



}
