

using Microsoft.EntityFrameworkCore;

namespace SutureHealth.DataScraping.Services.SqlServer
{
    public partial class SqlServerDataScrapingDbContext : DataScrapingDbContext
    {
        public SqlServerDataScrapingDbContext(DbContextOptions<SqlServerDataScrapingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreating(modelBuilder.Entity<ScrapedPatient>());
            OnModelCreating(modelBuilder.Entity<ScrapedPatientDetail>());            
            OnModelCreating(modelBuilder.Entity<Allergy>());
            OnModelCreating(modelBuilder.Entity<Condition>());
            OnModelCreating(modelBuilder.Entity<Contact>());
            OnModelCreating(modelBuilder.Entity<Immunization>());
            OnModelCreating(modelBuilder.Entity<Medication>());
            OnModelCreating(modelBuilder.Entity<Observation>());
            OnModelCreating(modelBuilder.Entity<Prescription>());
            OnModelCreating(modelBuilder.Entity<Procedure>());

            OnModelCreating(modelBuilder.Entity<ScrapedPatientHistory>());
            OnModelCreating(modelBuilder.Entity<ScrapedPatientDetailHistory>());
            OnModelCreating(modelBuilder.Entity<AllergyHistory>());
            OnModelCreating(modelBuilder.Entity<ConditionHistory>());
            OnModelCreating(modelBuilder.Entity<ContactHistory>());
            OnModelCreating(modelBuilder.Entity<ImmunizationHistory>());
            OnModelCreating(modelBuilder.Entity<MedicationHistory>());
            OnModelCreating(modelBuilder.Entity<ObservationHistory>());
            OnModelCreating(modelBuilder.Entity<PrescriptionHistory>());
            OnModelCreating(modelBuilder.Entity<ProcedureHistory>());            

            base.OnModelCreating(modelBuilder);
        }        
    }
}