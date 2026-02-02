using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Documents.Services.SqlServer
{
    public partial class SqlServerDocumentDbContext : DocumentDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<TemplateAnnotation> entityBuilder)
        {
            entityBuilder.ToTable("TemplateAnnotation")
                         .HasKey(m => m.TemplateAnnotationId);
            entityBuilder.Property(m => m.AnnotationType)
                         .HasConversion<string>();
            entityBuilder.HasOne(m => m.Template)
                         .WithMany(m => m.Annotations)
                         .HasForeignKey(m => m.TemplateId);
        }
    }
}
