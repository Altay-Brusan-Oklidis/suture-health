using Microsoft.EntityFrameworkCore;

namespace SutureHealth.Hchb.Services;

public abstract class HchbWebDbContext : DbContext, IDbContextSchema
{
    public string Schema {  get; }
    protected HchbWebDbContext(DbContextOptions options, IDbContextSchema schema) : base(options) 
    {
        Schema = schema.Schema;
    }
    
    public DbSet<HchbPatientWeb> HchbPatients { get; set; }
        
    public DbSet<HchbTransaction> HchbTransactions { get; set; }
    
    public DbSet<RequestStatus> RequestStatuses { get; set; }
    
    public DbSet<SutureTask> Tasks { get; set; }
    
    public DbSet<ICDCode> ICDCodes { get; set; }

    public DbSet<HchbTemplate> Templates { get; set; }
    
    public DbSet<Branch> Branches { get; set; }

    public DbSet<HL7MessageLog> Logs { get; set; }

    public DbSet<UserFacility> UserFacilities { get; set; }
}
