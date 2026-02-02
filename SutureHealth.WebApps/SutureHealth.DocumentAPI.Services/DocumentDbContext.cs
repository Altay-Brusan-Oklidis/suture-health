using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SutureHealth.Documents.Services
{
    public abstract class DocumentDbContext : DbContext
    {
        public DbSet<TemplateConfiguration> TemplateConfigurations { get; set; }
        public DbSet<FacilityTemplateConfiguration> FacilityTemplateConfigurations { get; set; }
        public DbSet<AnnotationOCRMapping> AnnotationOcrMappings { get; set; }
        public DbSet<OCRDocument> OcrDocuments { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<TemplateType> TemplateTypes { get; set; }

        public DocumentDbContext(DbContextOptions options) : base(options) { }

        public abstract Task CreateAnnotations(int destinationTemplateId, DataTable annotations, bool activateTemplate = false);
        public abstract Task CreateAnnotationsFromParentTemplateAsync(int destinationTemplateId);
        public abstract Task<int> CreateOrganizationTemplateAsync(int organizationId, string name, int templateTypeId, string storageKey, int memberId);
        public abstract Task<int> CreateRequestTemplateAsync(int parentTemplateId, int senderOrganizationId, int senderMemberId, string storageKey);
        public abstract Task<IEnumerable<Template>> GetActiveTemplatesByOrganizationIdAsync(int organizationId);
        public abstract Task<int> GetParentTemplateIdFromTemplateId(int templateId);
        public abstract Task<IEnumerable<FacilityTemplateConfiguration>> GetParentTemplatesByFacilityIdAsync(int facilityId);
        public abstract Task<IEnumerable<TemplateType>> GetPdfTemplateTypesByOrganizationIdAsync(int organizationId);
        public abstract Task<Template> GetTemplateByRequestIdAsync(int requestId);
    }
}
