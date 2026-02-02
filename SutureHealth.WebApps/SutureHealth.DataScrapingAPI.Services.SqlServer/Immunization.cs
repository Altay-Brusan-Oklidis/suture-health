using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace SutureHealth.DataScraping.Services.SqlServer
{
    partial class SqlServerDataScrapingDbContext : DataScrapingDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<Immunization> entityBuilder)
        {
            entityBuilder.ToTable("Immunization", "dataScraping")
                         .HasKey(x => x.Id);
        }
    }
}
