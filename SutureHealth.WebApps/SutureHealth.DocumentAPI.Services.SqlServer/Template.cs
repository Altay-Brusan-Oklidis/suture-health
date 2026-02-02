using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Documents.Services.SqlServer
{
    public partial class SqlServerDocumentDbContext : DocumentDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<Template> entityBuilder)
        {
            entityBuilder.ToTable("Template")
                         .HasKey(m => m.TemplateId);
            entityBuilder.HasOne(m => m.TemplateType)
                         .WithMany()
                         .HasForeignKey(m => m.TemplateTypeId);
            entityBuilder.HasMany(m => m.Annotations)
                         .WithOne(m => m.Template)
                         .HasForeignKey(m => m.TemplateId);
        }
    }
}
