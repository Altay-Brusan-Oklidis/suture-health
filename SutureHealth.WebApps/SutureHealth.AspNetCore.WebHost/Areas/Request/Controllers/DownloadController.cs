using Microsoft.AspNetCore.Mvc;
using SutureHealth.Documents.Services.Docnet;
using SutureHealth.Requests;
using SutureHealth.Requests.Services;
using SutureHealth.Application.Services;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Controller = SutureHealth.AspNetCore.Mvc.Controller;

namespace SutureHealth.AspNetCore.Areas.Request.Controllers
{
    [Area("Request")]
    [Route("request")]
    public class DownloadController : Controller
    {
        private const int TIFF_DPI = 100;

        protected IIdentityService IdentityServices { get; }
        protected IRequestServicesProvider RequestServices { get; }

        public DownloadController
        (
            IIdentityService identityServices,
            IRequestServicesProvider requestServices
        )
        {
            IdentityServices = identityServices;
            RequestServices = requestServices;
        }

        [HttpGet("{requestId:int}/download", Name = "DownloadRequest")]
        public async Task<IActionResult> DownloadDocument(int requestId)
            => await DownloadDocuments(new[] { requestId });

        [HttpPost("download", Name = "DownloadRequests")]
        public async Task<IActionResult> DownloadDocuments(int[] requestIds)
        {
            var userOrganizations = IdentityServices.GetOrganizationMembersByMemberId(CurrentUser.Id);
            var settings = await IdentityServices.GetOrganizationSettings().Where(os => userOrganizations.Any(o => o.OrganizationId == os.ParentId) && os.IsActive == true && os.ItemBool == true &&
                                                                                            (os.Key == "DownloadDocumentAsTiff" || os.Key == "DownloadMultipleDocumentsAsTiff"))
                                                                             .ToArrayAsync();
            var tiffInsteadOfPdf = settings.Any(s => s.Key == "DownloadDocumentAsTiff");
            var tiffInsteadOfZip = settings.Any(s => s.Key == "DownloadMultipleDocumentsAsTiff");

            var requests = await RequestServices.GetServiceableRequests().Where(r => requestIds.Any(id => r.SutureSignRequestId == id))
                                                                         .Include(r => r.Signer)
                                                                         .Include(r => r.Patient)
                                                                         .Include(r => r.Template).ThenInclude(t => t.TemplateType)
                                                                         .ToArrayAsync();
            var pdfs = await RequestServices.GetServiceableRequestPdfByIdAsync(requestIds);
            
            string GetRequestContainerFileName(IEnumerable<ServiceableRequest> requests)
            {
                var illegalChars = @"[\\\/:\*\?""'<>&|]";

                if (requests.Select(r => r.SignerMemberId).Distinct().Count() == 1)
                {
                    return $"Forms Signed By {Regex.Replace(requests.First().Signer.SigningName, illegalChars, string.Empty)}";
                }
                else
                {
                    return "Signed Forms";
                }
            }

            async Task<byte[]> GetZipContainerForRequestsAsync(IEnumerable<ServiceableRequest> requests, IReadOnlyDictionary<int, byte[]> pdfs, bool tiffInsteadOfPdf = false)
            {
                using (var zipFileStream = new MemoryStream())
                {
                    using (var zipFile = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                    {
                        foreach (var request in requests)
                        {
                            var file = zipFile.CreateEntry($"{request.GetRequestFileName(CurrentUser)}.{(tiffInsteadOfPdf ? "tiff" : "pdf")}", CompressionLevel.Fastest);

                            using (var fileStream = file.Open())
                            using (var pdfStream = new MemoryStream(tiffInsteadOfPdf ? ImageProcessing.SavePdfToTiff(pdfs[request.SutureSignRequestId], TIFF_DPI) : pdfs[request.SutureSignRequestId]))
                            {
                                await pdfStream.CopyToAsync(fileStream);
                            }
                        }
                    }

                    return zipFileStream.ToArray();
                }
            }

            // Make sure each request asked for belongs to an org the requesting user has access to.
            // This attempt at authorization should be replaced with a more formal implementation used by all endpoints in the future.
            var organizations = await userOrganizations.ToArrayAsync();
            if (!requests.Select(req => new int[] { req.SubmitterOrganizationId, req.SignerOrganizationId })
                         .All(reqIds => reqIds.Join(organizations, id => id, om => om.OrganizationId, (id, om) => id).Any()))
            {
                return StatusCode(401);
            }

            try
            {
                if (requests.Count() == 1)
                {
                    var request = requests.First();

                    return File(tiffInsteadOfPdf ? ImageProcessing.SavePdfToTiff(pdfs[request.SutureSignRequestId], TIFF_DPI) : pdfs[request.SutureSignRequestId],
                        @"application/octet-stream",
                        $"{request.GetRequestFileName(CurrentUser)}.{(tiffInsteadOfPdf ? "tiff" : "pdf")}");
                }
                else
                {
                    return File(tiffInsteadOfZip ? ImageProcessing.CombinePdfsToTiff(pdfs.Values, TIFF_DPI) : await GetZipContainerForRequestsAsync(requests, pdfs, tiffInsteadOfPdf),
                        @"application/octet-stream",
                        $"{GetRequestContainerFileName(requests)}.{(tiffInsteadOfZip ? "tiff" : "zip")}");
                }
            }
            finally
            {
                await RequestServices.MarkRequestDownloadedAsync(CurrentUser.Id, requests.Select(r => r.SutureSignRequestId).ToArray());
            }
        }
    }
}
