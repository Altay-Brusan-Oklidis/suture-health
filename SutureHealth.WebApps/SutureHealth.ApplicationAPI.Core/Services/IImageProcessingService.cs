using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Drawing;


namespace SutureHealth.Application.Services
{
    public interface IImageProcessingService
    {
        bool IsImageResolutionValid(IFormFile imageFile, int minWidth, int minHeight, int maxWidth, int maxHeight);
        bool IsImageResolutionValidForMobile(IFormFile imageFile, int minWidth, int minHeight); 
        bool IsImageTypeValid(IFormFile imageFile, List<ImageFormat> expectedFormats);
        IFormFile ConvertByteArrayToFormFile(byte[] byteArray, string fileName, string contentType);
        long GetImageSize(IFormFile imageFile);
        Size GetImageResolution(IFormFile imageFile);
        ImageFormat GetImageFormat(IFormFile imageFile);
        IFormFile CompressImage(IFormFile imageFile, int maxWidth = 0, int maxHeight = 0, int quality = 100);
        bool IsImageFileSizeValid(IFormFile imageFile, int maxByteSize);
        bool IsImageFileEmptyOrNull(IFormFile imageFile);
        string ToBase64(IFormFile imageFile);
    }
    public enum ImageFormat
    {
        Jpeg,
        Jpg,
        Png,
        Heif,
        Heic,
        Unknown
    }
}
