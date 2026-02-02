using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace SutureHealth.Patients.Services.SqlServer
{
    partial class SqlServerPatientDbContext : PatientDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<PatientFacilityAssociation> entityBuilder)
        {
            entityBuilder.ToTable("Facilities_Patients")
                         .HasKey(fp => new { fp.PatientId, fp.FacilityId });
        }
    }
}
