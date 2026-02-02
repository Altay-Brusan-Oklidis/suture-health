using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SutureHealth.Hchb.Services.SqlServer;

public partial class SqlServerHchbWebDbContext : HchbWebDbContext
{
    public SqlServerHchbWebDbContext(DbContextOptions<SqlServerHchbWebDbContext> options, IDbContextSchema schema) : base (options, schema) 
    {     
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {        
        // HchbPatientWeb
        modelBuilder.Entity<HchbPatientWeb>()
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<HchbPatientWeb>()
            .ToTable("HCHB_Patients", base.Schema)
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
            .ToTable("HCHB_Transactions", base.Schema)
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

        // Branches
        modelBuilder.Entity<Branch>()
            .ToTable("HCHB_Branches", base.Schema)
            .HasKey(x => x.Id);

        // logs
        modelBuilder.Entity<HL7MessageLog>()
            .ToTable("HL7MessageLogs", base.Schema)
            .HasKey(x => x.Id);

        modelBuilder.Entity<HL7MessageLog>()
            .ToTable("HL7MessageLogs", base.Schema)
            .Property("HchbPatientId").HasColumnName("HCHB_PatientId");

        modelBuilder.Entity<HL7MessageLog>()
            .ToTable("HL7MessageLogs", base.Schema)
            .Property("EpisodeId").HasColumnName("HCHB_EpisodeId");


        // Templates
        modelBuilder.Entity<HchbTemplate>()
            .ToTable("HCHB_Templates", base.Schema)
            .HasKey(x => x.Id);

        // Users_Facilities
        modelBuilder.Entity<UserFacility>()
            .ToTable("Users_Facilities")
            .HasKey(x => x.Id);

        // ICD codes
        modelBuilder.Entity<ICDCode>()
            .ToTable("ICD9Codes")
            .HasKey(x => x.Id);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Set the custom cache key factory
        optionsBuilder.ReplaceService<IModelCacheKeyFactory, SchemaAwareModelCacheKeyFactory>();
    }   
}

public class SchemaAwareModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        return new
        {
            Type = context.GetType(),
            Schema = context is IDbContextSchema schema ? schema.Schema : null
        };
    }
}