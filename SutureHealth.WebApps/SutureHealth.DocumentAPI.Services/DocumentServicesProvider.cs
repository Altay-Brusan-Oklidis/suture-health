using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Telerik.Windows.Documents.Fixed.Model.Editing;
using SutureHealth.Imaging;
using SutureHealth.Storage;
using SutureHealth.Documents.Services.Extensions;
using SutureHealth.Documents.Services.Docnet;
using PdfFormatProvider = Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.PdfFormatProvider;

namespace SutureHealth.Documents.Services
{
    public class TelerikDocumentServicesProvider : IDocumentServicesProvider
    {
        protected ILogger<IDocumentServicesProvider> Logger { get; }
        protected DocumentDbContext DbContext { get; }
        protected TextractService Textract { get; }
        protected IBinaryStorageService StorageService { get; }
        protected IAmazonS3 S3Client { get; }

        protected string TextractTopicArn { get; }
        protected string TextractRoleArn { get; }
        protected string DocumentS3Bucket { get; }

        public TelerikDocumentServicesProvider
        (
            ILogger<IDocumentServicesProvider> logger,
            DocumentDbContext context,
            TextractService textract,
            IConfiguration configuration,
            IBinaryStorageService storageService,
            IAmazonS3 s3Client
        )
        {
            Logger = logger;
            DbContext = context;
            Textract = textract;
            StorageService = storageService;
            S3Client = s3Client;

            DocumentS3Bucket = configuration["SutureHealth:S3BinaryStorageBucket"];
            TextractTopicArn = configuration["SutureHealth:DocumentServicesProvider:TextractTopicArn"];
            TextractRoleArn = configuration["SutureHealth:DocumentServicesProvider:TextractRoleArn"];
        }

        public async Task CreateAnnotationsFromOcrAsync(int destinationTemplateId, int ocrDocumentId)
        {
            var parentTemplateId = await DbContext.GetParentTemplateIdFromTemplateId(destinationTemplateId);
            var templateConfiguration = await this.DbContext.TemplateConfigurations.Include(tc => tc.AnnotationOcrMappings).SingleAsync(tc => tc.TemplateId == parentTemplateId && tc.Enabled == true);
            var ocrRecord = await this.DbContext.OcrDocuments.FindAsync(ocrDocumentId);
            var requestDocument = new TextractOcrDocument(ocrRecord.OCRResult);
            var pdfPages = null as IReadOnlyDictionary<int, System.Drawing.Size>;

            using (var annotations = GetNewTemplateAnnotationDataTable())
            {
                using (var stream = await this.S3Client.GetObjectStreamAsync(ocrRecord.StorageContainer, ocrRecord.StorageKey, null))
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);

                    using (var pdfDocument = global::Docnet.Core.DocLib.Instance.GetDocReader(memoryStream.ToArray(), new global::Docnet.Core.Models.PageDimensions(1)))
                    {
                        pdfPages = Enumerable.Range(1, pdfDocument.GetPageCount()).ToDictionary(i => i, i =>
                        {
                            using (var pdfPage = pdfDocument.GetPageReader(i - 1))
                            {
                                return new System.Drawing.Size(pdfPage.GetPageWidth(), pdfPage.GetPageHeight());
                            }
                        });
                    }
                }

                foreach (var mapping in templateConfiguration.AnnotationOcrMappings)
                {
                    var searchResults = requestDocument.TextWhere(t => t.IndexOf(mapping.SearchText, StringComparison.InvariantCultureIgnoreCase) >= 0);

                    if (!searchResults.Any())
                    {
                        Logger.LogWarning($"Failed to find any matches for AnnotationOcrMappingId {mapping.AnnotationOCRMappingId} (OcrDocumentId: {ocrDocumentId}) when creating annotations for TemplateId {destinationTemplateId}");
                    }

                    if (!mapping.MatchAll)
                    {
                        searchResults = searchResults.Take(1);
                    }

                    foreach (var searchResult in searchResults)
                    {
                        var resultPage = pdfPages[searchResult.PageNumber];
                        int pageHeight = (int)(resultPage.Height * 1.395),
                            heightMultiplier = (int)(resultPage.Height * 1.391),
                            widthMultiplier = (int)(resultPage.Width * 1.391),
                            coordLeft = (int)((searchResult.BoundingBox.Left + mapping.OffsetX) * widthMultiplier),
                            coordTop = (int)((searchResult.BoundingBox.Top + mapping.OffsetY) * heightMultiplier),
                            coordRight = (int)((searchResult.BoundingBox.Left + mapping.OffsetX + mapping.Width) * widthMultiplier),
                            coordBottom = (int)((searchResult.BoundingBox.Top + mapping.OffsetY + mapping.Height) * heightMultiplier);

                        annotations.Rows.Add(mapping.AnnotationFieldTypeId, searchResult.PageNumber, coordTop, coordRight, coordBottom, coordLeft, pageHeight, DBNull.Value);
                    }
                }

                await DbContext.CreateAnnotations(destinationTemplateId, annotations);
            }
        }

        public async Task<(IList<byte[]>, IList<Size>)> GetTemplatePngsWithDimensionsByIdAsync(int templateId, int dpi = 100)
            => GetTemplatePngsWithDimensions(await GetRawTemplatePdfByIdAsync(templateId), dpi);

        public async Task<(IList<byte[]>, IList<System.Drawing.Size>)> GetTemplatePngsWithDimensionsByRequestIdAsync(int sutureSignRequestId, int dpi = 100)
        {
            var template = await GetTemplateByRequestIdAsync(sutureSignRequestId);
            var pdfBytes = !string.IsNullOrWhiteSpace(template?.StorageKey) ? await StorageService.RetrieveFromBinaryStorageAsync(template.StorageKey) : null;

            return GetTemplatePngsWithDimensions(pdfBytes, dpi);
        }

        protected (IList<byte[]>, IList<System.Drawing.Size>) GetTemplatePngsWithDimensions(byte[] pdfBytes, int dpi)
        {
            if (pdfBytes != null)
            {
                var images = ImageProcessing.SavePdfToPng(pdfBytes, dpi, out var sizes);

                return (images, sizes);
            }
            else
            {
                return (Array.Empty<byte[]>(), Array.Empty<System.Drawing.Size>());
            }
        }

        /*
        private async Task<IList<byte[]>> GetAnnotatedTemplatePngs(int templateId, Action<FixedContentEditor, TemplateAnnotation> annotationDrawAction, int imageWidth)
        {
            var template = await GetTemplateByIdAsync(templateId);
            var rawData = await GetRawTemplatePdfByIdAsync(templateId);
            var pdf = RadFixedDocumentExtensions.OpenBinary(rawData);

            foreach (var annotation in template.Annotations.Where(a => a.PageNumber <= pdf.Pages.Count))
            {
                var editor = annotation.GetPdfEditor(pdf);

                annotationDrawAction.Invoke(editor, annotation);
            }

            return Enumerable.Range(0, pdf.Pages.Count).Select(i => pdf.SavePageToPng(i, imageWidth)).ToArray();
        }

        public async Task<IList<byte[]>> GetEditorTemplatePngsByIdAsync(int templateId, int dpi = 100)
            => await GetAnnotatedTemplatePngs(templateId, (editor, annotation) => editor.DrawTemplateAnnotationAsEditing(annotation), dpi);

        public async Task<IList<byte[]>> GetSignedTemplatePngsByIdAsync(int templateId, SignedPdfAttributes attributes, int dpi = 100)
            => await GetAnnotatedTemplatePngs(templateId, (editor, annotation) => editor.DrawTemplateAnnotationAsSigning(annotation, attributes.Signature, attributes.SignatureId, attributes.DateSigned), dpi);
        */

        public async Task<byte[]> GetSignedTemplatePdfByIdAsync(int templateId, SignedPdfAttributes attributes)
        {
            var template = await GetTemplateByIdAsync(templateId);
            var rawData = await GetRawTemplatePdfByIdAsync(templateId);
            var pdf = RadFixedDocumentExtensions.OpenBinary(rawData);            

            foreach (var annotation in template.Annotations.Where(a => a.PageNumber <= pdf.Pages.Count))
            {
                var editor = annotation.GetPdfEditor(pdf);

                editor.DrawTemplateAnnotationAsSigning(annotation, attributes.Signature, attributes.Pid, attributes.DateSigned);
            }
            foreach(var page in pdf.Pages)
            {
                var pageEditor = new FixedContentEditor(page);
                pageEditor.DrawRequestFooter(attributes.RequestId);
            }

            pdf.FlattenFormFields();

            return pdf.SaveBinary();
        }

        public async Task<byte[]> GetRejectedTemplatePdfByIdAsync(int templateId, RejectedPdfAttributes attributes)
        {
            var rawData = await GetRawTemplatePdfByIdAsync(templateId);
            var pdf = RadFixedDocumentExtensions.OpenBinary(rawData);

            foreach (var page in pdf.Pages.Select(p => new FixedContentEditor(p)))
            {
                if (page.Root == pdf.Pages[0])
                {
                    page.DrawRequestFooter(attributes.RequestId);
                }
                page.DrawRejectionWatermark();
            }
            pdf.AppendRejectionTemplate(attributes);

            pdf.FlattenFormFields();

            return pdf.SaveBinary();
        }

        public byte[] GenerateFaceToFacePdf(FaceToFaceAttributes attributes)
        {
            var pdf = RadFixedDocumentExtensions.OpenFaceToFaceTemplate(string.IsNullOrWhiteSpace(attributes.TreatmentPlan) ?
                                                                            RadFixedDocumentExtensions.FaceToFaceTemplateType.General :
                                                                            RadFixedDocumentExtensions.FaceToFaceTemplateType.WithTreatmentPlan);

            new FixedContentEditor(pdf.Pages[0]).DrawFaceToFaceAttributes(attributes);

            return pdf.SaveBinary();
        }

        public bool ValidatePdf(byte[] pdfBytes)
        {
            var provider = new PdfFormatProvider();

            try
            {
                provider.Import(pdfBytes);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<FacilityTemplateConfiguration>> GetParentTemplatesByFacilityIdAsync(int facilityId) =>
            await DbContext.GetParentTemplatesByFacilityIdAsync(facilityId);

        public async Task<IEnumerable<AnnotationOCRMapping>> GetTemplateAnnotationOcrMappingsByTemplateId(int templateId) =>
            await this.DbContext.TemplateConfigurations.AsNoTracking()
                                                       .Where(tc => tc.TemplateId == templateId)
                                                       .SelectMany(tc => tc.AnnotationOcrMappings)
                                                       .ToArrayAsync();

        public async Task SaveTemplateAnnotationOcrMappingsAsync(int templateId, IEnumerable<AnnotationOCRMapping> mappings)
        {
            if (mappings == null)
            {
                throw new ArgumentException("An enumerable of mappings must be provided.", "mappings");
            }

            var templateConfiguration = await this.DbContext.TemplateConfigurations.Include(tc => tc.AnnotationOcrMappings).FirstOrDefaultAsync(tc => tc.TemplateId == templateId);
            if (templateConfiguration == null)
            {
                throw new ArgumentException("No TemplateConfiguration could be found corresponding to the specified TemplateId.");
            }

            if (templateConfiguration.AnnotationOcrMappings.Any())
            {
                this.DbContext.AnnotationOcrMappings.RemoveRange(this.DbContext.AnnotationOcrMappings.Where(m => m.TemplateConfigurationId == templateConfiguration.TemplateConfigurationId));
                await this.DbContext.SaveChangesAsync();
            }

            if (mappings.Any())
            {
                foreach (var mapping in mappings)
                {
                    mapping.TemplateConfigurationId = templateConfiguration.TemplateConfigurationId;
                    mapping.AnnotationOCRMappingId = 0;

                    templateConfiguration.AnnotationOcrMappings.Add(mapping);
                }

                await this.DbContext.SaveChangesAsync();
            }
        }

        public async Task StartOcrAnalyzeTemplateConfigurationAsync(int templateId, string storageKey, string requestedBy)
        {
            var templateConfiguration = await this.DbContext.TemplateConfigurations.SingleOrDefaultAsync(tc => tc.TemplateId == templateId);
            var ocrDocument = new OCRDocument()
            {
                StorageContainer = this.DocumentS3Bucket,
                StorageKey = storageKey,
                DateCreated = DateTime.UtcNow,
                DateCompleted = null,
                OCRResult = null
            };

            if (templateConfiguration == null)
            {
                throw new ArgumentException("No TemplateConfiguration could be found corresponding to the provided Id", "templateConfigurationId");
            }

            if (templateConfiguration.OCRDocumentId.HasValue)
            {
                ocrDocument = await this.DbContext.OcrDocuments.FindAsync(templateConfiguration.OCRDocumentId.Value);
            }
            else
            {
                await this.DbContext.OcrDocuments.AddAsync(ocrDocument);
                await this.DbContext.SaveChangesAsync();

                templateConfiguration.OCRDocumentId = ocrDocument.OCRDocumentId;
                templateConfiguration.DateModified = DateTime.UtcNow;
                templateConfiguration.ModifiedBy = requestedBy;

                await this.DbContext.SaveChangesAsync();
            }

            await SendToTextractAsync(ocrDocument.OCRDocumentId, OcrDocumentType.TemplateConfiguration, templateConfiguration.TemplateConfigurationId, storageKey);
        }

        public async Task StartOcrAnalyzeRequestDocumentAsync(long requestDocumentId, string storageKey)
        {
            var ocrDocument = new OCRDocument()
            {
                StorageContainer = this.DocumentS3Bucket,
                StorageKey = storageKey,
                DateCreated = DateTime.UtcNow,
                DateCompleted = null,
                OCRResult = null
            };

            this.DbContext.OcrDocuments.Add(ocrDocument);
            await this.DbContext.SaveChangesAsync();

            await this.SendToTextractAsync(ocrDocument.OCRDocumentId, OcrDocumentType.RequestDocument, requestDocumentId, storageKey);
        }

        public async Task CompleteOcrAnalysisAsync(string jobId, int ocrDocumentId)
        {
            var ocrDocument = await this.DbContext.OcrDocuments.FindAsync(ocrDocumentId);

            ocrDocument.DateCompleted = DateTime.UtcNow;
            ocrDocument.OCRResult = (await this.Textract.GetDocumentAnalysisAsync(jobId)).AsSerializedJson();

            await this.DbContext.SaveChangesAsync();
        }

        public OcrJobDetails GetJobDetails(string jobTag)
        {
            return TryParseJobTag(jobTag, out var ocrDocumentId, out var documentType, out var identifier) ? new OcrJobDetails() { OcrDocumentId = ocrDocumentId, DocumentType = documentType, Identifier = identifier } : null;
        }

        public async Task<IOcrDocument> GetOcrDocumentResultByIdAsync(int ocrDocumentId)
        {
            var ocrDocument = await this.DbContext.OcrDocuments.FindAsync(ocrDocumentId);

            if (ocrDocument == null || ocrDocument.OCRResult == null)
            {
                return null;
            }

            return new TextractOcrDocument(ocrDocument.OCRResult);
        }

        public async Task<IOcrDocument> GetOcrDocumentResultByTemplateIdAsync(int templateId)
        {
            var ocrDocument = (await this.DbContext.TemplateConfigurations.Include(tc => tc.OCRDocument).SingleOrDefaultAsync(tc => tc.TemplateId == templateId))?.OCRDocument;

            if (ocrDocument == null || ocrDocument.OCRResult == null)
            {
                return null;
            }

            return new TextractOcrDocument(ocrDocument.OCRResult);
        }

        public async Task CreateAnnotationsAsync(CreateAnnotationsRequest request)
        {
            using (var annotations = GetNewTemplateAnnotationDataTable())
            {
                foreach (var annotation in request.Annotations)
                {
                    annotations.Rows.Add((int)annotation.Type,
                                         annotation.PageNumber,
                                         annotation.Top,
                                         annotation.Right,
                                         annotation.Bottom,
                                         annotation.Left,
                                         annotation.PageHeight,
                                         !string.IsNullOrWhiteSpace(annotation.Value) ? annotation.Value : (object)DBNull.Value);
                }

                await DbContext.CreateAnnotations(request.TemplateId, annotations, request.ActivateTemplate);
            }
        }

        public async Task CreateAnnotationsFromParentTemplateAsync(int destinationTemplateId) =>
            await DbContext.CreateAnnotationsFromParentTemplateAsync(destinationTemplateId);

        public async Task<int> CreateOrganizationTemplateAsync(int organizationId, string name, int templateTypeId, string storageKey, int memberId)
            => await DbContext.CreateOrganizationTemplateAsync(organizationId, name, templateTypeId, storageKey, memberId);

        public async Task<int> CreateRequestTemplateAsync(int parentTemplateId, int senderOrganizationId, int senderMemberId, string storageKey)
        {
            var templateId = await DbContext.CreateRequestTemplateAsync(parentTemplateId, senderOrganizationId, senderMemberId, storageKey);
            await CreateAnnotationsFromParentTemplateAsync(templateId);
            return templateId;
        }

        public async Task<CreateRequestTemplateResponse> CreateRequestTemplateAsync(int facilityId, int userId, string documentTypeKey, string storageKey)
        {
            var templateConfiguration = (await this.GetParentTemplatesByFacilityIdAsync(facilityId)).FirstOrDefault(t => t.Enabled && string.Equals(t.DocumentTypeKey, documentTypeKey, StringComparison.OrdinalIgnoreCase));
            if (templateConfiguration == null)
            {
                throw new ArgumentException("No active template was found corresponding to the provided facilityId and documentTypeKey.");
            }

            return new CreateRequestTemplateResponse()
            {
                TemplateId = await this.DbContext.CreateRequestTemplateAsync(templateConfiguration.TemplateId, facilityId, userId, storageKey),
                AnnotationProcessingMode = (TemplateProcessingMode)templateConfiguration.TemplateProcessingModeId
            };
        }

        public async Task<TemplateConfiguration> GetTemplateConfigurationByTemplateIdAsync(int templateId) =>
            await this.DbContext.TemplateConfigurations.AsNoTracking().SingleOrDefaultAsync(tc => tc.TemplateId == templateId);

        public async Task SaveTemplateConfigurationAsync(SaveTemplateConfigurationRequest request, string requestedBy)
        {
            var templateConfiguration = await this.DbContext.TemplateConfigurations.SingleOrDefaultAsync(tc => tc.TemplateId == request.TemplateId);
            if (templateConfiguration == null)
            {
                templateConfiguration = new TemplateConfiguration()
                {
                    TemplateId = request.TemplateId,
                    DateCreated = DateTime.UtcNow,
                    CreatedBy = requestedBy
                };

                this.DbContext.TemplateConfigurations.Add(templateConfiguration);
            }

            if (request.TemplateProcessingMode != TemplateProcessingMode.DDP)
            {
                // This logic isn't strictly necessary, but I'm adding it to allow a means for the UI to put the object in a state where we can re-run OCR analysis.
                templateConfiguration.OCRDocumentId = null;
            }

            templateConfiguration.DocumentTypeKey = request.DocumentTypeKey;
            templateConfiguration.Enabled = request.Enabled;
            templateConfiguration.TemplateProcessingModeId = (int)request.TemplateProcessingMode;
            templateConfiguration.DateModified = DateTime.UtcNow;
            templateConfiguration.ModifiedBy = requestedBy;

            await this.DbContext.SaveChangesAsync();
        }

        public async Task<Template> GetTemplateByIdAsync(int templateId)
            => await DbContext.Templates.AsNoTracking()
                                        .Include(t => t.TemplateType)
                                        .Include(t => t.Annotations)
                                        .FirstOrDefaultAsync(t => t.TemplateId == templateId);

        public async Task<Template> GetTemplateByRequestIdAsync(int requestId)
            => await DbContext.GetTemplateByRequestIdAsync(requestId);

        public async Task<IEnumerable<Template>> GetActiveTemplatesByOrganizationIdAsync(int organizationId)
            => await DbContext.GetActiveTemplatesByOrganizationIdAsync(organizationId);

        public async Task<byte[]> GetRawTemplatePdfByIdAsync(int templateId)
        {
            var template = await DbContext.Templates.FindAsync(templateId);

            if (string.IsNullOrWhiteSpace(template?.StorageKey))
            {
                return null;
            }

            return await StorageService.RetrieveFromBinaryStorageAsync(template.StorageKey);
        }

        public async Task<IEnumerable<TemplateType>> GetPdfTemplateTypesByOrganizationIdAsync(int organizationId)
            => await DbContext.GetPdfTemplateTypesByOrganizationIdAsync(organizationId);

        public IQueryable<TemplateType> GetTemplateTypes()
            => DbContext.TemplateTypes.AsNoTracking();


        protected async Task<string> SendToTextractAsync(int ocrDocumentId, OcrDocumentType documentType, long identifier, string s3Key)
        {
            var jobId = await this.Textract.StartDocumentAnalysisAsync(new TextractAnalyzeDocumentRequest()
            {
                JobIdentifier = $"OcrDocumentId-{ocrDocumentId}",
                JobTag = CreateJobTag(ocrDocumentId, documentType, identifier),
                S3Bucket = this.DocumentS3Bucket,
                S3Key = s3Key,
                SnsRoleArn = this.TextractRoleArn,
                SnsTopicArn = this.TextractTopicArn
            });

            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new InvalidOperationException("Failed to receive JobId from OCR service.  This usually indicates a permissions issue.");
            }

            return jobId;
        }

        protected static DataTable GetNewTemplateAnnotationDataTable()
        {
            var table = new DataTable();

            table.Columns.Add("AnnotationFieldTypeId", typeof(int));
            table.Columns.Add("PageNumber", typeof(int));
            table.Columns.Add("Top", typeof(int));
            table.Columns.Add("Right", typeof(int));
            table.Columns.Add("Bottom", typeof(int));
            table.Columns.Add("Left", typeof(int));
            table.Columns.Add("PageHeight", typeof(int));
            table.Columns.Add("Value", typeof(string));

            return table;
        }

        protected static bool TryParseJobTag(string jobTag, out int ocrDocumentId, out OcrDocumentType documentType, out long identifier)
        {
            string[] jobTagSegments;

            ocrDocumentId = default(int);
            documentType = default(OcrDocumentType);
            identifier = default(long);

            if (string.IsNullOrWhiteSpace(jobTag))
            {
                return false;
            }

            jobTagSegments = jobTag.Split('-');

            if (jobTagSegments.Count() != 3)
            {
                return false;
            }

            if (!int.TryParse(jobTagSegments.First(), out ocrDocumentId))
            {
                return false;
            }

            if (!Enum.TryParse(jobTagSegments[1], out documentType))
            {
                return false;
            }

            if (!long.TryParse(jobTagSegments.Last(), out identifier))
            {
                return false;
            }

            return true;
        }

        protected static string CreateJobTag(int ocrDocumentId, OcrDocumentType documentType, long identifier)
        {
            return $"{ocrDocumentId}-{documentType}-{identifier}";
        }
    }
}
