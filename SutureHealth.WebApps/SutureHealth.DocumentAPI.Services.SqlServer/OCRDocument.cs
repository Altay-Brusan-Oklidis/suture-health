using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Documents.Services.SqlServer
{
    public partial class SqlServerDocumentDbContext : DocumentDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<OCRDocument> entityBuilder)
        {
            entityBuilder.ToTable("OCRDocument")
                         .HasKey(m => m.OCRDocumentId);
        }
    }
}
