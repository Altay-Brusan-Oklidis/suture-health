using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Patients.Services.SqlServer
{
    partial class SqlServerPatientDbContext : PatientDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<PatientIdentifier> entityBuilder)
        {
            entityBuilder.ToTable("PatientIdentifier")
                         .HasKey(m => m.Id);
            entityBuilder.Property(m => m.Id)
                         .HasColumnName("PatientIdentifierId")
                         .ValueGeneratedOnAdd();
            entityBuilder.Property(m => m.ParentId)
                         .HasColumnName("PatientId");
            entityBuilder.HasOne(m => m.Parent)
                         .WithMany(m => m.Identifiers)
                         .HasForeignKey(m => m.ParentId);
        }
    }
}
