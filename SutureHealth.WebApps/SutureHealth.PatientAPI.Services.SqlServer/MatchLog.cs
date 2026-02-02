using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace SutureHealth.Patients.Services.SqlServer
{
    partial class SqlServerPatientDbContext : PatientDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<MatchLog> entityBuilder)
        {
            entityBuilder.ToTable("PatientMatchLog")
                         .HasKey(log => log.MatchPatientLogID);
            entityBuilder.Property(log => log.MedicareNumber)
                         .HasColumnName("SubmittedMedicareMBI");
            entityBuilder.Property(log => log.Gender)
                         .HasConversion<string>(g => g.ToString().Substring(0, 1), g => Enum.GetValues(typeof(Gender)).Cast<Gender>().Where(e => string.Equals(g, e.ToString().Substring(0, 1))).DefaultIfEmpty(Gender.Unknown).First());
            entityBuilder.HasMany(log => log.Outcomes)
                         .WithOne(o => o.MatchLog)
                         .HasForeignKey(o => o.MatchPatientLogID);
            entityBuilder.HasOne(log => log.Organization)
                         .WithMany(o => o.MatchLogs);
                         
        }

        public static void OnModelCreating(EntityTypeBuilder<MatchOutcome> entityBuilder)
        {
            entityBuilder.ToTable("PatientMatchOutcome")
                         .HasKey(o => o.MatchPatientOutcomeID);
            entityBuilder.HasOne(o => o.Patient);
        }
    }
}