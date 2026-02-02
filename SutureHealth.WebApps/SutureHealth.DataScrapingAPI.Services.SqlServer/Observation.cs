using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace SutureHealth.DataScraping.Services.SqlServer
{
    partial class SqlServerDataScrapingDbContext : DataScrapingDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<Observation> entityBuilder)
        {
            entityBuilder.ToTable("Observation", "dataScraping")
                         .HasKey(x => x.Id);
        }
    }
}
