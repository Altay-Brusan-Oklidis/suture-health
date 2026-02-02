using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Documents.Services.SqlServer
{
    public partial class SqlServerDocumentDbContext : DocumentDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<AnnotationOCRMapping> entityBuilder)
        {
            entityBuilder.ToTable("AnnotationOCRMapping")
                         .HasKey(m => m.AnnotationOCRMappingId);
            entityBuilder.HasOne(m => m.TemplateConfiguration)
                         .WithMany(m => m.AnnotationOcrMappings)
                         .HasForeignKey(m => m.TemplateConfigurationId);
        }
    }
}
