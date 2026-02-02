using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace SutureHealth.Patients.Services.SqlServer
{
    partial class SqlServerPatientDbContext : PatientDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<Organization> entityBuilder)
        {
            entityBuilder.ToTable("Organization")
                         .HasKey(o=>o.OrganizationId);
            entityBuilder.HasMany(o => o.MatchLogs)
                         .WithOne(m => m.Organization)
                         .HasForeignKey( m=> m.FacilityId);            
        }
    }
}
