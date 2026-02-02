using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Documents.Services.SqlServer
{
    public partial class SqlServerDocumentDbContext : DocumentDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<TemplateConfiguration> entityBuilder)
        {
            entityBuilder.ToTable("TemplateConfiguration")
                         .HasKey(m => m.TemplateConfigurationId);
            entityBuilder.HasMany(m => m.AnnotationOcrMappings)
                         .WithOne(m => m.TemplateConfiguration)
                         .HasForeignKey(m => m.TemplateConfigurationId);
            entityBuilder.HasOne(m => m.OCRDocument)
                         .WithMany()
                         .HasForeignKey(m => m.OCRDocumentId)
                         .IsRequired(false);
        }
    }
}
