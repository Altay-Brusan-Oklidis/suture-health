using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Patients.Services.SqlServer
{
    partial class SqlServerPatientDbContext : PatientDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<PatientAddress> entityBuilder)
        {
            entityBuilder.ToTable("PatientAddress")
                         .Ignore(x => x.County)
                         .HasKey(x => x.Id);
            entityBuilder.Property(m => m.Id)
                         .HasColumnName("PatientAddressId");
            entityBuilder.Property(m => m.ParentId)
                         .HasColumnName("PatientId");
            entityBuilder.HasOne(m => m.Parent)
                         .WithMany(m => m.Addresses)
                         .HasForeignKey(m => m.ParentId);
        }
    }
}
