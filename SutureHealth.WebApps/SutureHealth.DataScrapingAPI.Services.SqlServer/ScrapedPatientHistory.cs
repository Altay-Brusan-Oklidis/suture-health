using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace SutureHealth.DataScraping.Services.SqlServer
{
    partial class SqlServerDataScrapingDbContext : DataScrapingDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<ScrapedPatientHistory> entityBuilder)
        {
            entityBuilder.ToTable("ScrapedPatientHistory", "dataScraping")
                         .HasKey(x => x.Id);
        }
    }
}
