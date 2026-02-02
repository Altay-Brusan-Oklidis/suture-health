using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Documents.Services.SqlServer
{
    public partial class SqlServerDocumentDbContext : DocumentDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<FacilityTemplateConfiguration> entityBuilder)
        {
            entityBuilder.HasNoKey();
        }
    }
}
