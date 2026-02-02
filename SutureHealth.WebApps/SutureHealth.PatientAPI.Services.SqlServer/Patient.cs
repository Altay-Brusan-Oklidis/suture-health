using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SutureHealth.Patients.Services.SqlServer
{
    partial class SqlServerPatientDbContext : PatientDbContext
    {
        public static void OnModelCreating(EntityTypeBuilder<Patient> entityBuilder)
        {
            entityBuilder.ToTable("Patient")
                         .HasKey(x => x.PatientId);
            entityBuilder.Property(x => x.Gender)
                         .HasConversion<string>(g => g.ToString().Substring(0, 1), g => Enum.GetValues(typeof(Gender)).Cast<Gender>().Where(e => string.Equals(g, e.ToString().Substring(0, 1))).DefaultIfEmpty(Gender.Unknown).First());
            entityBuilder.HasMany(m => m.Addresses)
                         .WithOne(m => m.Parent)
                         .HasForeignKey(m => m.ParentId);
            entityBuilder.HasMany(m => m.Contacts)
                         .WithOne(m => m.Parent)
                         .HasForeignKey(m => m.ParentId);
            entityBuilder.HasMany(m => m.Identifiers)
                         .WithOne(m => m.Parent)
                         .HasForeignKey(m => m.ParentId);
            entityBuilder.HasMany(m => m.OrganizationKeys)
                         .WithOne(m => m.Patient)
                         .HasForeignKey(a => a.PatientId);
            entityBuilder.HasMany(m => m.Phones)
                         .WithOne(m => m.Parent)
                         .HasForeignKey(m => m.ParentId);
        }
    }
}
