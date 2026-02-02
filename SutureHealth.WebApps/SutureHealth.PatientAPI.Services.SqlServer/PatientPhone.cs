using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace SutureHealth.Patients.Services.SqlServer
{
    partial class SqlServerPatientDbContext : PatientDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<PatientPhone> entityBuilder)
        {

            entityBuilder.ToTable("PatientPhone")
                         .HasKey(x => x.Id);
            entityBuilder.Property(m => m.Id).HasColumnName("PatientPhoneId");
            entityBuilder.Property(m => m.ParentId).HasColumnName("PatientId");
            entityBuilder.Property(x => x.Type)
             .HasConversion(g => g.ToString(),
                            g => (ContactType)Enum.Parse(typeof(ContactType), g));
            entityBuilder.HasOne(m => m.Parent)
                         .WithMany(m => m.Phones)
                         .HasForeignKey(m => m.ParentId);
        }
    }
}
