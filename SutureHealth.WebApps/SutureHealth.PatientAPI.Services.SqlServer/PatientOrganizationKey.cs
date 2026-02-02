using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Patients.Services.SqlServer
{
    partial class SqlServerPatientDbContext : PatientDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<PatientOrganizationKey> entityBuilder)
        {
            entityBuilder.ToTable("PatientOrganizationKey")
                         .HasKey(op => new { op.PatientId, op.OrganizationId } );
        }
    }
}
