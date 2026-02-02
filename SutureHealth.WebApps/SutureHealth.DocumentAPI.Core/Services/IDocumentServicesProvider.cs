using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SutureHealth.Imaging;

namespace SutureHealth.Documents.Services
{
    public interface IDocumentServicesProvider
    {
        Task CreateAnnotationsAsync(CreateAnnotationsRequest request);
        Task CreateAnnotationsFromOcrAsync(int destinationTemplateId, int ocrDocumentId);
        Task CreateAnnotationsFromParentTemplateAsync(int destinationTemplateId);
        Task<int> CreateOrganizationTemplateAsync(int organizationId, string name, int templateTypeId, string storageKey, int memberId);
        Task<int> CreateRequestTemplateAsync(int parentTemplateId, int senderOrganizationId, int senderMemberId, string storageKey);
        Task<CreateRequestTemplateResponse> CreateRequestTemplateAsync(int facilityId, int senderUserFacilityId, string documentTypeKey, string storageKey);
        Task CompleteOcrAnalysisAsync(string jobId, int ocrDocumentId);
        Task<IEnumerable<Template>> GetActiveTemplatesByOrganizationIdAsync(int organizationId);
        //Task<IList<byte[]>> GetEditorTemplatePngsByIdAsync(int templateId, int dpi = 100);
        OcrJobDetails GetJobDetails(string jobTag);
        Task<IOcrDocument> GetOcrDocumentResultByIdAsync(int ocrDocumentId);
        Task<IOcrDocument> GetOcrDocumentResultByTemplateIdAsync(int templateId);
        Task<IEnumerable<FacilityTemplateConfiguration>> GetParentTemplatesByFacilityIdAsync(int facilityId);
        Task<byte[]> GetRawTemplatePdfByIdAsync(int templateId);
        Task<byte[]> GetRejectedTemplatePdfByIdAsync(int templateId, RejectedPdfAttributes attributes);
        Task<byte[]> GetSignedTemplatePdfByIdAsync(int templateId, SignedPdfAttributes attributes);
        byte[] GenerateFaceToFacePdf(FaceToFaceAttributes attributes);
        //Task<IList<byte[]>> GetSignedTemplatePngsByIdAsync(int templateId, SignedPdfAttributes attributes, int dpi = 100);
        Task<IEnumerable<AnnotationOCRMapping>> GetTemplateAnnotationOcrMappingsByTemplateId(int templateId);
        Task<TemplateConfiguration> GetTemplateConfigurationByTemplateIdAsync(int templateId);
        Task<Template> GetTemplateByRequestIdAsync(int requestId);
        Task<Template> GetTemplateByIdAsync(int templateId);
        Task<(IList<byte[]>, IList<System.Drawing.Size>)> GetTemplatePngsWithDimensionsByIdAsync(int templateId, int dpi = 100);
        Task<(IList<byte[]>, IList<System.Drawing.Size>)> GetTemplatePngsWithDimensionsByRequestIdAsync(int sutureSignRequestId, int dpi = 100);
        Task<IEnumerable<TemplateType>> GetPdfTemplateTypesByOrganizationIdAsync(int organizationId);
        Task SaveTemplateConfigurationAsync(SaveTemplateConfigurationRequest request, string requestedBy);
        Task SaveTemplateAnnotationOcrMappingsAsync(int templateId, IEnumerable<AnnotationOCRMapping> mappings);
        Task StartOcrAnalyzeTemplateConfigurationAsync(int templateId, string storageKey, string requestedBy);
        Task StartOcrAnalyzeRequestDocumentAsync(long requestDocumentId, string storageKey);
        bool ValidatePdf(byte[] pdfBytes);

        IQueryable<TemplateType> GetTemplateTypes();
    }

    public class CreateRequestTemplateResponse
    {
        public int TemplateId { get; set; }
        public TemplateProcessingMode AnnotationProcessingMode { get; set; }
    }

    public enum OcrDocumentType
    {
        TemplateConfiguration,
        RequestDocument
    }

    public class OcrJobDetails
    {
        public int OcrDocumentId { get; set; }
        public long Identifier { get; set; }
        public OcrDocumentType DocumentType { get; set; }
    }

    public class SaveTemplateConfigurationRequest
    {
        public int TemplateId { get; set; }
        public string DocumentTypeKey { get; set; }
        public TemplateProcessingMode TemplateProcessingMode { get; set; }
        public bool Enabled { get; set; }
    }

    public class CreateAnnotationsRequest
    {
        public int TemplateId { get; set; }
        public bool ActivateTemplate { get; set; }
        public IEnumerable<Annotation> Annotations { get; set; }

        public class Annotation
        {
            public AnnotationType Type { get; set; }
            public int PageNumber { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
            public int Left { get; set; }
            public int PageHeight { get; set; }
            public string Value { get; set; }
        }
    }
}