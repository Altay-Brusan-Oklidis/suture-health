using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace SutureHealth.DataScraping.Services.SqlServer
{
    partial class SqlServerDataScrapingDbContext : DataScrapingDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<ConditionHistory> entityBuilder)
        {
            entityBuilder.ToTable("ConditionHistory", "dataScraping")
                         .HasKey(x => x.Id);            
        }
    }
}
