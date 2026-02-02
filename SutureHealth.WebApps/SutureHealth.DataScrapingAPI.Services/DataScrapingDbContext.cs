using Microsoft.EntityFrameworkCore;

namespace SutureHealth.DataScraping.Services
{
    public abstract class DataScrapingDbContext : DbContext
    {
        public DbSet<ScrapedPatient> ScrapedPatient { get; set; }
        public DbSet<ScrapedPatientHistory> ScrapedPatientHistory { get; set; }
        public DbSet<ScrapedPatientDetail> ScrapedPatientDetail { get; set; }
        public DbSet<ScrapedPatientDetailHistory> ScrapedPatientDetailHistory { get; set; }
        protected DataScrapingDbContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public async Task CreateScrapedPatientAsync(ScrapedPatient scrapedPatient)
        {
            await ScrapedPatient.AddAsync(scrapedPatient);
            await SaveChangesAsync();
        }

        public async Task CreateScrapedPatientHistoryAsync(ScrapedPatientHistory scrapedPatientHistory)
        {
            await ScrapedPatientHistory.AddAsync(scrapedPatientHistory);
            await SaveChangesAsync();
        }

        public async Task CreateScrapedPatientDetailAsync(ScrapedPatientDetail scrapedPatientDetail)
        {
            await ScrapedPatientDetail.AddAsync(scrapedPatientDetail);          
            await SaveChangesAsync(); 
        }

        public async Task CreateScrapedPatientDetailHistoryAsync(ScrapedPatientDetailHistory scrapedPatientDetailHistory)
        {            
            await ScrapedPatientDetailHistory.AddAsync(scrapedPatientDetailHistory);
            await SaveChangesAsync(); 
        }

        public async Task<ScrapedPatient?> GetScrapedPatientById(string externalId)
        {            
            var scrapedPatient = await ScrapedPatient.OrderByDescending(sp => sp.CreatedAt).FirstOrDefaultAsync(sp => sp.ExternalId == externalId);

            return scrapedPatient;
        }
        public async Task<ScrapedPatientDetail?> GetScrapedPatientDetailById(string externalId)
        {            
            var scrapedPatientDetail = await ScrapedPatientDetail.Include(sp => sp.Allergies)
                                                                 .Include(sp => sp.Conditions)
                                                                 .Include(sp => sp.Contacts)
                                                                 .Include(sp => sp.Immunizations)
                                                                 .Include(sp => sp.Medications)
                                                                 .Include(sp => sp.Observations)
                                                                 .Include(sp => sp.Prescriptions)
                                                                 .Include(sp => sp.Procedures)
                                                                 .OrderByDescending(sp => sp.CreatedAt)
                                                                 .FirstOrDefaultAsync(sp => sp.ExternalId == externalId);


            return scrapedPatientDetail;
        }

        public Task UpdateScrapedPatientDetail(ScrapedPatientDetail scrapedPatientDetail)
        {
            ScrapedPatientDetail.Update(scrapedPatientDetail);
            SaveChanges();

            return Task.CompletedTask;
        }

        public Task UpdateScrapedPatient(ScrapedPatient scrapedPatient)
        {
            ScrapedPatient.Update(scrapedPatient);
            SaveChanges();

            return Task.CompletedTask;
        }

        public async Task RemoveDuplicatePatientDetails()
        {
            var duplicatePatientDetails = ScrapedPatientDetail.Include(spd => spd.Allergies)
                                                              .Include(spd => spd.Conditions)
                                                              .Include(spd => spd.Contacts)
                                                              .Include(spd => spd.Immunizations)
                                                              .Include(spd => spd.Medications)
                                                              .Include(spd => spd.Observations)
                                                              .Include(spd => spd.Prescriptions)
                                                              .Include(spd => spd.Procedures)
                                                              .AsEnumerable()
                                                              .GroupBy(spd => spd.ExternalId)
                                                              .SelectMany(spd => spd.Select(patient => patient)
                                                              .OrderByDescending(spd => spd.CreatedAt)
                                                              .Skip(1)).ToList();                                                           


            ScrapedPatientDetail.RemoveRange(duplicatePatientDetails);
            await SaveChangesAsync();
        }

        public async Task RemoveDuplicatePatients()
        {
            var duplicatePatients = ScrapedPatient.AsEnumerable()
                                                  .GroupBy(sp => sp.ExternalId)
                                                  .SelectMany(sp => sp.Select(patient => patient)
                                                  .OrderByDescending(p => p.CreatedAt)
                                                  .Skip(1)).ToList();
                                                        
            ScrapedPatient.RemoveRange(duplicatePatients);
            await SaveChangesAsync();
        }

    }
}
