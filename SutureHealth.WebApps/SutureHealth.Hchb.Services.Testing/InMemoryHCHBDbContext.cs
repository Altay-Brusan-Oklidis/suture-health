using Amazon.Lambda.Core;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SutureHealth.Patients;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb.Services.Testing
{
    public class InMemoryHCHBDbContext : HchbWebDbContext
    {
        string dbName;
        public InMemoryHCHBDbContext(string databaseName) : base(new DbContextOptions<HchbWebDbContext>(),
            new DbContextSchema("HCHB_CommonSpirit"))
        {
            this.dbName = databaseName;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // HchbPatientWeb
            modelBuilder.Entity<HchbPatientWeb>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<HchbPatientWeb>()
                .ToTable("HCHB_Patients", "HCHB_CommonSpirit")
                .HasKey(x => x.Id);

            modelBuilder.Entity<HchbPatientWeb>()
                .Property(x => x.HchbPatientId)
                .HasColumnName("HCHB_PatientId");

            modelBuilder.Entity<HchbPatientWeb>()
                .Property(x => x.EpisodeId)
                .HasColumnName("HCHB_EpisodeId");

            modelBuilder.Entity<HchbPatientWeb>()
                .HasIndex(x => new { x.PatientId, x.EpisodeId })
                .IsUnique();

            // HchbTransaction
            modelBuilder.Entity<HchbTransaction>()
                .ToTable("HCHB_Transactions", "HCHB_CommonSpirit")
                .HasKey(x => x.Id);

            modelBuilder.Entity<HchbTransaction>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<HchbTransaction>()
                .HasIndex(x => new { x.RequestId })
                .IsUnique();

            modelBuilder.Entity<HchbTransaction>()
                .Property(x => x.HchbPatientId)
                .HasColumnName("HCHB_PatientId");

            modelBuilder.Entity<HchbTransaction>()
                .Property(x => x.EpisodeId)
                .HasColumnName("HCHB_EpisodeId");

            //modelBuilder.Entity<HchbTransaction>()
            //    .Property(x=>x.AdmitDate)
            //    .HasColumnType("DateTime");


            // Branches
            modelBuilder.Entity<Branch>()
                .ToTable("HCHB_Branches", "HCHB_CommonSpirit")
                .HasKey(x => x.Id);

            modelBuilder.Entity<Branch>()
                .Property(x => x.BranchCode)
                .HasColumnName("BranchCode");

            modelBuilder.Entity<Branch>()
                .Property(x => x.BranchName)
                .HasColumnName("BranchName");


            // MessageLog
            modelBuilder.Entity<HL7MessageLog>()
                .ToTable("HL7MessageLogs", "HCHB_CommonSpirit")
                .HasKey(x => x.Id);

            // Template
            modelBuilder.Entity<HchbTemplate>()
                .ToTable("HCHB_Templates", "HCHB_CommonSpirit")
                .HasKey(x => x.Id);


            // RequestStatus
            modelBuilder.Entity<RequestStatus>()
                .ToTable("Requests")
                .HasKey(x => x.Id);

            modelBuilder.Entity<RequestStatus>()
                .HasOne(x => x.RequestPatient)
                .WithMany(x => x.Requests)
                .HasForeignKey(x => x.Patient);

            modelBuilder.Entity<SuturePatient>()
                .ToTable("Patients")
                .HasKey(x => x.PatientId);

            modelBuilder.Entity<SuturePatient>()
                .Property(x => x.DateOfBirth)
                .HasColumnName("DOB");

            // SutureTask
            modelBuilder.Entity<SutureTask>()
                .ToTable("Tasks")
                .HasKey(x => x.TaskId);

            modelBuilder.Entity<ICDCode>()
                .ToTable("ICD9Codes")
                .HasKey(x => x.Id);

            modelBuilder.Entity<ICDCode>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ICDCode>()
                .Property(x => x.IcdCode)
                .HasColumnName("ICD9Code");

            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(s=>Trace.WriteLine(s), new[] { DbLoggerCategory.Database.Command.Name });
            optionsBuilder.UseInMemoryDatabase("HCHBDbContext-" + dbName);
            base.OnConfiguring(optionsBuilder);
        }

    }
}
