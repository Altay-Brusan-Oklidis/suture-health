using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.DataScraping.Services.SqlServer
{
    partial class SqlServerDataScrapingDbContext : DataScrapingDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<Condition> entityBuilder)
        {
            entityBuilder.ToTable("Condition", "dataScraping")
                         .HasKey(x => x.Id);
        }
    }
}
