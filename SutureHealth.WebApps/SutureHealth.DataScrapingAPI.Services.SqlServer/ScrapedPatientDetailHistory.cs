using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.DataScraping.Services.SqlServer
{
    partial class SqlServerDataScrapingDbContext : DataScrapingDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<ScrapedPatientDetailHistory> entityBuilder)
        {
            entityBuilder.ToTable("ScrapedPatientDetailHistory", "dataScraping")
                         .HasKey(x => x.Id);
            entityBuilder.HasMany(x => x.Contacts)
                         .WithOne()
                         .HasForeignKey(x => x.PatientId);
            entityBuilder.HasMany(x => x.Conditions)
                         .WithOne()
                         .HasForeignKey(x => x.PatientId);
            entityBuilder.HasMany(x => x.Allergies)
                         .WithOne()
                         .HasForeignKey(x => x.PatientId);
            entityBuilder.HasMany(x => x.Medications)
                         .WithOne()
                         .HasForeignKey(x => x.PatientId);
            entityBuilder.HasMany(x => x.Immunizations)
                         .WithOne()
                         .HasForeignKey(x => x.PatientId);
            entityBuilder.HasMany(x => x.Prescriptions)
                         .WithOne()
                         .HasForeignKey(x => x.PatientId);
            entityBuilder.HasMany(x => x.Procedures)
                         .WithOne()
                         .HasForeignKey(x => x.PatientId);
            entityBuilder.HasMany(x => x.Observations)
                         .WithOne()
                         .HasForeignKey(x => x.PatientId);

        }
    }
}
