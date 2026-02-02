using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Documents.Services.SqlServer
{
    public partial class SqlServerDocumentDbContext : DocumentDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<TemplateType> entityBuilder)
        {
            entityBuilder.ToTable("TemplateType")
                         .HasKey(m => m.TemplateTypeId);
            entityBuilder.Property(m => m.DateAssociation)
                         .HasColumnName("DateAssociationId")
                         .HasConversion<int>();
        }
    }
}
