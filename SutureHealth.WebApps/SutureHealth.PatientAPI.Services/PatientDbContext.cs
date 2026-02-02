using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SutureHealth.Linq;
using System.Linq;
using System.Linq.Expressions;

namespace SutureHealth.Patients
{
    public abstract class PatientDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<MatchLog> MatchLogs { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<MatchOutcome> MatchLogOutcomes { get; set; }

        public DbSet<PatientFacilityAssociation> Facilities_Patients { get; set; }

        protected PatientDbContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public abstract Task<int> CreatePatientAsync(Patient patient, int organizationId, int memberId);
        public abstract Task LogPatientMatch
        (
            string firstName,
            string middleName,
            string lastName,
            string suffix,
            DateTime dateOfBirth,
            Gender gender,
            string socialSecurityNumber,
            string medicareMBI,
            string medicaidNumber,
            string medicaidState,
            string facilityMRN,
            string addressLine1,
            string addressLine2,
            string city,
            string stateOrProvince,
            string postalCode,
            bool matchedPatient,
            bool? needsReview,
            bool? manuallyMatched,
            int? manuallyMatchedBy,
            DateTime? manuallyMatchedOn,
            bool multiplePatientsMatched,
            bool similiarPatientHigh,
            bool similiarPatientLow,
            DateTime startProcessing,
            DateTime endProcessing,
            int recordsEvaluated,
            int submittedBy,
            int organizationId,
            bool? isSelfPay,
            bool? isPrivateInsurance,
            bool? isMedicareAdvantage,
            IEnumerable<PatientPhone> phones,
            IEnumerable<IMatchingResult<Patient>> matches,
            RequestSource source = RequestSource.SutureHealth,
            string sourceDescription = ""
        );
        public abstract Task UpdatePatientAsync(Patient patient, int organizationId, int memberId);
        public abstract Task UpdatePatientSocialSecurityAsync(int patientId, string ssn, string ssn4, int changeBy);
        public abstract Task ResetPatientPayerMixFlagsAsync(int patientId, int changeBy);
        public abstract IQueryable<Patient> SearchPatientsForSigner(int memberId, string searchText, int? organizationId = null);
        public abstract IQueryable<Patient> SearchPatientsForAssistant(int memberId, string searchText, int? organizationId = null);
        public abstract IQueryable<Patient> QueryPatientsForSenderByMemberId(int memberId);
        public abstract IQueryable<Patient> QueryPatientsForSenderByOrganizationId(int organizationId);
        public abstract Task<IEnumerable<int>> GetOrganizationIdsInPatientScopeForSenderAsync(int memberId, int? organizationId = null);

        public IQueryable<MatchLog> GetMatchLogByFilter(Expression<Func<MatchLog, bool>> predicate)
        {
            return MatchLogs.Where(predicate);
        }
        public IQueryable<MatchLog> GetAllMatchLogs()
        {
            return MatchLogs.AsQueryable();
        }
        public async Task<MatchLog> GetMatchLogByIdAsync(int patientMatchLogId) =>
            await MatchLogs.Where(ml => ml.MatchPatientLogID == patientMatchLogId)
                           .Include(ml => ml.Outcomes)
                                .ThenInclude(o => o.Patient)
                                    .ThenInclude(a => a.Addresses)
                           .Include(ml => ml.Outcomes)
                                .ThenInclude(o => o.Patient)
                                    .ThenInclude(i => i.Identifiers)
                           .Include(ml => ml.Outcomes)
                                .ThenInclude(o => o.Patient)
                                    .ThenInclude(a => a.OrganizationKeys)
                            .Include(ml => ml.Outcomes)
                                .ThenInclude(o => o.Patient)
                                    .ThenInclude(a => a.Phones)
                           .FirstOrDefaultAsync();


    }
}
