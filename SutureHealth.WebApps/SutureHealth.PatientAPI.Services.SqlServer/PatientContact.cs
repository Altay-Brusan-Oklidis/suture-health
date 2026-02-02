using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Patients.Services.SqlServer
{
    partial class SqlServerPatientDbContext : PatientDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<PatientContact> entityBuilder)
        {
            entityBuilder.ToTable("PatientContact")
                         .HasKey(x => x.Id);
            entityBuilder.Property(m => m.Id).HasColumnName("PatientContactId");
            entityBuilder.Property(m => m.ParentId).HasColumnName("PatientId");
            entityBuilder.HasOne(m => m.Parent)
                         .WithMany(m => m.Contacts)
                         .HasForeignKey(m => m.ParentId);
        }
    }
}
