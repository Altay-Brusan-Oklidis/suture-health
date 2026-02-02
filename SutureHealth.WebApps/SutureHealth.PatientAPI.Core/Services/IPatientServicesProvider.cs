using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SutureHealth.Linq;
using System.Linq.Expressions;

namespace SutureHealth.Patients.Services
{
    public interface IPatientServicesProvider
    {

        Task<Patient> CreateAsync(Patient patient, int organizationId, int memberId);
        Task<Patient> GetByIdAsync(int patientId, int? senderOrganizationId = null);
        IQueryable<Patient> GetByExternalIdentifier(int organizationId, string externalId);
        IQueryable<Patient> GetByIdentifier(string type, string value);
        IQueryable<MatchLog> GetMatchLogByFilter(Expression<Func<MatchLog, bool>> predicate);
        IQueryable<MatchLog> GetAllMatchLogs();
        IQueryable<MatchOutcome> GetMatchOutcome();
        Task<MatchLog> GetMatchLogByIdAsync(int matchPatientLogId);
        Task SetPatientMatchLogFlagsToResolved(int matchLogId, int userId);
        Task LogPatientMatch
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
        Task<PatientMatchingResponse> MatchAsync(PatientMatchingRequest request);        
        Task<bool> TryDisableNeedReviewForMatchLogInstance(int matchlogId);
        IQueryable<Patient> SearchPatientsForSigner(int memberId, string searchText, int? organizationId = null);
        IQueryable<Patient> SearchPatientsForAssistant(int memberId, string searchText, int? organizationId = null);
        Task UpdateAsync(Patient patient, int organizationId, int memberId);
        Task ResetPayerMixFlags(int patientId,int ChangeBy);
        IQueryable<Patient> QueryPatients();
        IQueryable<MatchLog> QueryMatchLogs();
        IQueryable<Patient> QueryPatientsForSenderByMemberId(int memberId);
        IQueryable<Patient> QueryPatientsForSenderByOrganizationId(int organizationId);
        Task<IEnumerable<int>> GetOrganizationIdsInPatientScopeForSenderAsync(int memberId, int? organizationId = null);
        Task UpdatePatientSocialSecurityInfo(int patientId, string ssn, string ssn4, int changeBy);

    }
}
