using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using SutureHealth.Authorities;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SutureHealth.Linq;
using System.Data;
using Microsoft.Extensions.FileSystemGlobbing;
using System.IO;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Runtime.Intrinsics.X86;

namespace SutureHealth.Patients.Services.SqlServer
{
    public partial class SqlServerPatientDbContext : PatientDbContext
    {
        public SqlServerPatientDbContext(DbContextOptions<SqlServerPatientDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreating(modelBuilder.Entity<Patient>());
            OnModelCreating(modelBuilder.Entity<PatientAddress>());
            OnModelCreating(modelBuilder.Entity<PatientContact>());
            OnModelCreating(modelBuilder.Entity<PatientIdentifier>());
            OnModelCreating(modelBuilder.Entity<PatientOrganizationKey>());
            OnModelCreating(modelBuilder.Entity<MatchLog>());
            OnModelCreating(modelBuilder.Entity<MatchOutcome>());
            OnModelCreating(modelBuilder.Entity<PatientFacilityAssociation>());
            OnModelCreating(modelBuilder.Entity<Organization>());
            OnModelCreating(modelBuilder.Entity<PatientPhone>());
            base.OnModelCreating(modelBuilder);
        }

        protected async Task AssociatePatientToCompany(int patientId, string companyMrn, int memberId, int organizationId)
        {
            var parameters = new[]
            {
                new SqlParameter("@PatientId", patientId),//System.Data.SqlDbType.Int),
                new SqlParameter("@AssociationExists", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output },
                new SqlParameter("@CompanyMrn", companyMrn ?? (object)DBNull.Value),
                new SqlParameter("@FacilityId", organizationId),
                new SqlParameter("@ChangeBy", memberId)
            };
            await Database.ExecuteSqlRawAsync(@"EXECUTE [dbo].[AssociatePatientToCompany]
    @CompanyMrn,
    @FacilityId,
    @PatientId,
    @ChangeBy,
    @AssociationExists OUTPUT", parameters);
        }

        protected async Task AssociatePatientToOrganization(int patientId, string facilityMrn, int memberId, int organizationId)
        {
            var parameters = new[]
            {
                new SqlParameter("@PatientId", patientId),//System.Data.SqlDbType.Int),
                new SqlParameter("@AssociationExists", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output },
                new SqlParameter("@FacilityMrn", facilityMrn ?? (object)DBNull.Value),
                new SqlParameter("@FacilityId", organizationId),
                new SqlParameter("@ChangeBy", memberId)
            };
            await Database.ExecuteSqlRawAsync(@"EXECUTE [dbo].[AssociatePatientToOrganization]
    @FacilityMrn,
    @FacilityId,
    @PatientId,
    @ChangeBy,
    @AssociationExists OUTPUT", parameters);
        }

        public override async Task<int> CreatePatientAsync
        (
            Patient patient,
            int organizationId,
            int memberId
        )
        {
            var patientId = new SqlParameter("@NewPatientId", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
            var editParameters = GetPatientEditParameters(patient, organizationId, memberId);

            await Database.ExecuteSqlRawAsync(@"EXECUTE [dbo].[CreatePatient] 
    @FirstName
  , @MiddleName
  , @MaidenName
  , @LastName
  , @Suffix
  , @Last4Ssn
  , @SSN
  , @DOB
  , @Gender
  , @Address1
  , @Address2
  , @City
  , @State
  , @Zip
  , @FacilityMrn
  , @FacilityId
  , @ChangeBy
  , @IsMedicare
  , @IsMedicareAdvantage
  , @IsMedicAid
  , @IsSelf
  , @IsPrivate
  , @IsMedicarePrimary
  , @IsMedicareAdvantagePrimary
  , @IsMedicAidPrimary
  , @IsSelfPrimary
  , @IsPrivatePrimary
  , @MedicareNum
  , @MedicareMBI
  , @MedicAidNum
  , @MedicareAdvantageNum
  , @PrivateNum
  , @MedicAidState
  , @MedicareGrpNumber
  , @MedicareAdvGrpNumber
  , @MedicaidGrpNumber
  , @PrivateGrpNumber
  , @Phones
  , @NewPatientId OUTPUT", editParameters.Union(new[] { patientId }));
            return (int)patientId.Value;
        }

        public override async Task UpdatePatientAsync(Patient patient, int organizationId, int memberId)
        {
            var patientId = new SqlParameter("@PatientId", patient.PatientId);
            var editParameters = GetPatientEditParameters(patient, organizationId, memberId);

            await Database.ExecuteSqlRawAsync(@"EXECUTE [dbo].[UpdatePatient] 
    @PatientId
  , @FirstName
  , @MiddleName
  , @MaidenName
  , @LastName
  , @Suffix
  , @Last4Ssn
  , @SSN
  , @DOB
  , @Gender
  , @Address1
  , @Address2
  , @City
  , @State
  , @Zip
  , @FacilityMrn
  , @FacilityId
  , @ChangeBy
  , @IsMedicare
  , @IsMedicareAdvantage
  , @IsMedicAid
  , @IsSelf
  , @IsPrivate
  , @IsMedicarePrimary
  , @IsMedicareAdvantagePrimary
  , @IsMedicAidPrimary
  , @IsSelfPrimary
  , @IsPrivatePrimary
  , @MedicareNum
  , @MedicareMBI
  , @MedicAidNum
  , @MedicareAdvantageNum
  , @PrivateNum
  , @MedicAidState
  , @MedicareGrpNumber
  , @MedicareAdvGrpNumber
  , @MedicaidGrpNumber
  , @PrivateGrpNumber
  , @Phones"
  , editParameters.Union(new[] { patientId }));

            await AssociatePatientToOrganization(patient.PatientId,
                                                 patient.Identifiers.SingleOrDefault(i => i.Type.EqualsIgnoreCase(KnownTypes.UniqueExternalIdentifier))?.Value,
                                                 memberId,
                                                 organizationId);
        }

        public override async Task UpdatePatientSocialSecurityAsync(int patientId,string ssn, string ssn4, int changeBy) 
        {

            var parameters = new[]
            {                
                new SqlParameter("@PatientId", patientId),
                new SqlParameter("@Last4Ssn", ssn4?? (object)DBNull.Value),
                new SqlParameter("@SSN", ssn ?? (object)DBNull.Value),
                new SqlParameter("@ChangeBy", changeBy),

            };
            await Database.ExecuteSqlRawAsync(@"EXECUTE [dbo].[UpdatePatientSocialSecurity]
                                                @PatientId,
                                                @Last4Ssn,
                                                @SSN,
                                                @ChangeBy", parameters);

        }

        public override async Task ResetPatientPayerMixFlagsAsync(int patientId, int changeBy) 
        {

            var parameters = new[]
{
                new SqlParameter("@PatientId", patientId),                
                new SqlParameter("@ChangeBy", changeBy),

            };
            await Database.ExecuteSqlRawAsync(@"EXECUTE [dbo].[ResetPatientPayerMixFlags]
                                                @PatientId,                                               
                                                @ChangeBy", parameters);
        }
        protected SqlParameter[] GetPatientEditParameters(Patient patient, int organizationId, int memberId)
        {
            var address = patient.Addresses.SingleOrDefault();
            var sanitizedSsn = SocialSecurityAdministration.SanitizeSocialSecurityNumber(patient.Identifiers.Where(i => i.Type.EqualsAnyIgnoreCase(new string[] { KnownTypes.SocialSecurityNumber, KnownTypes.SocialSecuritySerial }))
                                                                                                            .OrderBy(i => i.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial))
                                                                                                            .FirstOrDefault()?.Value ?? string.Empty);
            var medicareMbi = patient.Identifiers.GetMedicareMedicareBeneficiaryNumberOrDefault()?.Value.Replace("-", string.Empty).ToUpper();

            var phones = new DataTable();
            phones.Columns.Add("PatientId", typeof(int));
            phones.Columns.Add("Type", typeof(string));
            phones.Columns.Add("Value", typeof(string));
            phones.Columns.Add("IsActive", typeof(bool));
            phones.Columns.Add("IsPrimary", typeof(bool));


            foreach (var phone in patient.Phones)
            {
                if (!phone.Value.IsNullOrWhiteSpace())
                    phones.Rows.Add(patient.PatientId,
                                     phone.Type,
                                     phone.Value,
                                     true,
                                     phone.IsPrimary);
            }


            return new[]
            {
                new SqlParameter("@FirstName", patient.FirstName?.Trim() ?? (object)DBNull.Value),
                new SqlParameter("@MiddleName", patient.MiddleName?.Trim() ?? (object)DBNull.Value),
                new SqlParameter("@MaidenName", DBNull.Value),
                new SqlParameter("@LastName", patient.LastName?.Trim() ?? (object)DBNull.Value),
                new SqlParameter("@Suffix", patient.Suffix ?? (object)DBNull.Value),
                new SqlParameter("@Last4Ssn", sanitizedSsn.Length >= 4 ? sanitizedSsn.GetLast(4) : (object)DBNull.Value),
                new SqlParameter("@SSN", sanitizedSsn.Length == 9 ? sanitizedSsn : (object)DBNull.Value),
                new SqlParameter("@DOB", patient.Birthdate),
                new SqlParameter("@Gender", patient.Gender.ToString().Substring(0, 1)),
                new SqlParameter("@Address1", address.Line1 ?? (object)DBNull.Value),
                new SqlParameter("@Address2", address.Line2 ?? (object)DBNull.Value),
                new SqlParameter("@City", address.City ?? (object)DBNull.Value),
                new SqlParameter("@State", address.StateOrProvince ?? (object)DBNull.Value),
                new SqlParameter("@Zip", Regex.Replace(address.PostalCode, @"[^A-Za-z0-9]", "") ?? (object)DBNull.Value),
                new SqlParameter("@FacilityMrn", patient.Identifiers.SingleOrDefault(i => i.Type.EqualsIgnoreCase(KnownTypes.UniqueExternalIdentifier))?.Value ?? (object)DBNull.Value),
                new SqlParameter("@FacilityId", organizationId),
                new SqlParameter("@ChangeBy", memberId),
                new SqlParameter("@IsMedicare", !string.IsNullOrWhiteSpace(medicareMbi) || patient.Identifiers.Any(i => i.Type.EqualsIgnoreCase(KnownTypes.Medicare)) || patient.Identifiers.Any(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicareAdvantage))),
                new SqlParameter("@IsMedicareAdvantage", patient.Identifiers.Any(i => i.Type.EqualsIgnoreCase(KnownTypes.MedicareAdvantage))),
                new SqlParameter("@IsMedicAid", patient.Identifiers.Any(i => i.IsMedicaidType())),
                new SqlParameter("@IsSelf", patient.Identifiers.Any(i => i.Type.EqualsIgnoreCase(KnownTypes.SelfPay))),
                new SqlParameter("@IsPrivate", patient.Identifiers.Any(i => i.Type.EqualsIgnoreCase(KnownTypes.PrivateInsurance))),
                new SqlParameter("@IsMedicarePrimary", true),
                new SqlParameter("@IsMedicareAdvantagePrimary", true),
                new SqlParameter("@IsMedicAidPrimary", true),
                new SqlParameter("@IsSelfPrimary", true),
                new SqlParameter("@IsPrivatePrimary", true),
                new SqlParameter("@MedicareMBI", !string.IsNullOrWhiteSpace(medicareMbi) ? Regex.Replace(medicareMbi, @"[^A-Za-z0-9]", "").ToUpper() : (object)DBNull.Value),
                new SqlParameter("@MedicareGrpNumber", DBNull.Value),
                new SqlParameter("@MedicareNum", DBNull.Value),
                new SqlParameter("@MedicareAdvGrpNumber", DBNull.Value),
                new SqlParameter("@MedicareAdvantageNum", DBNull.Value),
                new SqlParameter("@MedicAidGrpNumber", DBNull.Value),
                new SqlParameter("@MedicAidNum", patient.Identifiers.GetMedicaidNumberOrDefault()?.Value.ToUpper() ?? (object)DBNull.Value),
                new SqlParameter("@MedicAidState", patient.Identifiers.GetMedicaidStateOrDefault()?.Value.ToUpper() ?? (object)DBNull.Value),
                new SqlParameter("@PrivateGrpNumber", DBNull.Value),
                new SqlParameter("@PrivateNum", DBNull.Value),
                new SqlParameter("@Phones", phones)
                {
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[PatientPhoneList]"
                }
            };
        }

        public override async Task LogPatientMatch
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
            bool? isIsMedicareAdvantage,
            IEnumerable<PatientPhone>? phones,
            IEnumerable<IMatchingResult<Patient>> matches,
            RequestSource source = RequestSource.SutureHealth,
            string sourceDescription = null
        )
        {
//#if !DEBUG
            var sanitizedSsn = SocialSecurityAdministration.SanitizeSocialSecurityNumber(socialSecurityNumber);
            var matchResults = new DataTable();
            matchResults.Columns.Add("PatientId", typeof(int));
            matchResults.Columns.Add("MatchScore", typeof(float));
            matchResults.Columns.Add("Override", typeof(bool));

            foreach (var match in matches)
                matchResults.Rows.Add(match.Match.PatientId, match.Score, match.Rules.OfType<GoldenTicketRuleResult<Patient>>().Any());

            await Database.ExecuteSqlRawAsync(@"EXECUTE [dbo].[LogPatientMatch] 
   @SubmittedFirstName
  ,@SubmittedMiddleName
  ,@SubmittedLastName
  ,@SubmittedSuffix
  ,@SubmittedDOB
  ,@SubmittedGender
  ,@SubmittedSSN
  ,@SubmittedLastSSN
  ,@SubmittedMedicaid#
  ,@SubmittedMedicaidState
  ,@SubmittedFacilityMRN
  ,@SubmittedAddress1
  ,@SubmittedAddress2
  ,@SubmittedCity
  ,@SubmittedState
  ,@SubmittedZip
  ,@SubmittedHomePhone
  ,@SubmittedWorkPhone
  ,@SubmittedMobilePhone
  ,@SubmittedOtherPhone
  ,@SubmittedPrimaryPhone
  ,@MatchAlgorithmID
  ,@MatchedPatient
  ,@NeedsReview
  ,@ManuallyMatched
  ,@ManuallyMatchedBy
  ,@ManuallyMatchedOn
  ,@MultiplePatientsMatched
  ,@SimilarPatientHigh
  ,@SimilarPatientLow
  ,@ProcessingStartTime
  ,@ProcessingEndTime
  ,@RecordsEvaluated
  ,@SubmittedBy
  ,@SubmittedByFacility
  ,@SubmittedMedicareMBI
  ,@SubmittedSource
  ,@SourceDescription
  ,@IsSelfPay 
  ,@IsPrivateInsurance
  ,@IsMedicareAdvantage
  ,@MatchingResults", new SqlParameter[] {
                new SqlParameter("@SubmittedFirstName", firstName ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedMiddleName", middleName ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedLastName", lastName ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedSuffix", suffix ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedDOB", dateOfBirth),
                new SqlParameter("@SubmittedGender", gender.ToString()),
                new SqlParameter("@SubmittedSSN", sanitizedSsn != string.Empty && sanitizedSsn.Length > 4 ? sanitizedSsn : (object)DBNull.Value),
                new SqlParameter("@SubmittedLastSsn", sanitizedSsn != string.Empty && sanitizedSsn.Length > 3 ? sanitizedSsn.GetLast(4) : (object)DBNull.Value),
                new SqlParameter("@SubmittedMedicaid#", medicaidNumber ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedMedicaidState", medicaidState ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedFacilityMrn", facilityMRN ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedAddress1", addressLine1 ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedAddress2", addressLine2 ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedCity", city ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedState", stateOrProvince ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedZip", Regex.Replace(postalCode, @"[^A-Za-z0-9]", "") ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedHomePhone", phones?.Where(p=>p.Type== ContactType.HomePhone).FirstOrDefault()?.Value?.SanitatePhoneNumber() ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedWorkPhone", phones?.Where(p=>p.Type== ContactType.WorkPhone).FirstOrDefault()?.Value?.SanitatePhoneNumber() ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedMobilePhone", phones?.Where(p=>p.Type== ContactType.Mobile).FirstOrDefault()?.Value?.SanitatePhoneNumber() ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedOtherPhone", phones?.Where(p=>p.Type== ContactType.OtherPhone).FirstOrDefault()?.Value?.SanitatePhoneNumber() ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedPrimaryPhone", phones?.Where(t => t.IsPrimary == true).FirstOrDefault() != null ? phones.Where(t => t.IsPrimary == true).FirstOrDefault().Type.ToString() : (object)DBNull.Value),
                new SqlParameter("@MatchAlgorithmID", 5),
                new SqlParameter("@MatchedPatient", matchedPatient),
                new SqlParameter("@NeedsReview", needsReview ?? false),
                new SqlParameter("@ManuallyMatched", manuallyMatched ?? false),
                new SqlParameter("@ManuallyMatchedBy", manuallyMatchedBy ?? (object)DBNull.Value),
                new SqlParameter("@ManuallyMatchedOn", manuallyMatchedOn ?? (object)DBNull.Value),
                new SqlParameter("@MultiplePatientsMatched", multiplePatientsMatched),
                new SqlParameter("@SimilarPatientHigh", similiarPatientHigh),
                new SqlParameter("@SimilarPatientLow", similiarPatientLow),
                new SqlParameter("@ProcessingStartTime", startProcessing),
                new SqlParameter("@ProcessingEndTime", endProcessing),
                new SqlParameter("@RecordsEvaluated", recordsEvaluated),
                new SqlParameter("@SubmittedBy", submittedBy),
                new SqlParameter("@SubmittedByFacility", organizationId),
                new SqlParameter("@SubmittedMedicareMBI", medicareMBI ?? (object)DBNull.Value),
                new SqlParameter("@SubmittedSource",source.ToString()),
                new SqlParameter("@SourceDescription",sourceDescription ?? (object)DBNull.Value),
                new SqlParameter("@IsSelfPay", isSelfPay ?? (object)DBNull.Value),
                new SqlParameter("@IsPrivateInsurance",isPrivateInsurance ?? (object)DBNull.Value),
                new SqlParameter("@IsMedicareAdvantage", isIsMedicareAdvantage ?? (object)DBNull.Value),
                new SqlParameter("@MatchingResults", matchResults)
                {
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[PatientMatchingScore]"
                }
            });
//#endif
        }

        public override IQueryable<Patient> SearchPatientsForSigner(int memberId, string searchText, int? organizationId = null)
            => SearchPatients(QueryPatientsForSigner(memberId, organizationId), searchText);

        public override IQueryable<Patient> SearchPatientsForAssistant(int memberId, string searchText, int? organizationId = null)
            => SearchPatients(QueryPatientsForAssistant(memberId, organizationId), searchText);

        public override IQueryable<Patient> QueryPatientsForSenderByMemberId(int memberId)
            => Patients.FromSqlInterpolated($@"SELECT p.*
FROM Patient p WITH (NOLOCK)
	INNER JOIN
		(
			SELECT DISTINCT inp.PatientId
			FROM OrganizationMember iom WITH (NOLOCK)
				INNER JOIN Organization ino WITH (NOLOCK) ON iom.OrganizationId = ino.OrganizationId
				CROSS APPLY
					(
						SELECT iom.OrganizationId
						UNION ALL
						SELECT crs_shared_o.OrganizationId
						FROM Organization crs_target_o WITH (NOLOCK)
							INNER JOIN Organization crs_company_o WITH (NOLOCK) ON crs_target_o.CompanyId = crs_company_o.OrganizationId
							INNER JOIN OrganizationSetting crs_company_os WITH (NOLOCK) ON crs_company_o.OrganizationId = crs_company_os.ParentId AND crs_company_os.[Key] = 'ShareCompanyPatients' AND crs_company_os.ItemBool = 1 AND crs_company_os.IsActive = 1
							INNER JOIN Organization crs_shared_o WITH (NOLOCK) ON crs_company_o.CompanyId = crs_shared_o.CompanyId AND crs_shared_o.OrganizationId <> iom.OrganizationId
						WHERE crs_target_o.OrganizationId = iom.OrganizationId AND crs_company_o.IsActive = 1 AND crs_shared_o.IsActive = 1
					) search_facilities
				INNER JOIN PatientOrganizationKey ipo WITH (NOLOCK) ON search_facilities.OrganizationId = ipo.OrganizationId
				INNER JOIN Patient inp WITH (NOLOCK) ON ipo.PatientId = inp.PatientId
			WHERE iom.MemberId = {memberId} AND iom.IsActive = 1 AND ino.IsActive = 1 AND ipo.IsActive = 1 AND inp.IsActive = 1
		) sp ON p.PatientId = sp.PatientId");

        public override IQueryable<Patient> QueryPatientsForSenderByOrganizationId(int organizationId)
            => Patients.FromSqlInterpolated($@"SELECT ef_p.*
FROM Patient ef_p WITH (NOLOCK)
	INNER JOIN
		(
			SELECT DISTINCT p.PatientId
			FROM
				(
					SELECT {organizationId} AS OrganizationId
					UNION ALL
					SELECT shared_o.OrganizationId
					FROM Organization target_o WITH (NOLOCK)
						INNER JOIN Organization company_o WITH (NOLOCK) ON target_o.CompanyId = company_o.OrganizationId
						INNER JOIN OrganizationSetting company_os WITH (NOLOCK) ON company_o.OrganizationId = company_os.ParentId AND company_os.[Key] = 'ShareCompanyPatients' AND company_os.ItemBool = 1 AND company_os.IsActive = 1
						INNER JOIN Organization shared_o WITH (NOLOCK) ON target_o.CompanyId = shared_o.CompanyId AND shared_o.OrganizationId <> {organizationId}
					WHERE target_o.OrganizationId = {organizationId} AND company_o.IsActive = 1 AND shared_o.IsActive = 1 AND target_o.CompanyId IS NOT NULL AND target_o.CompanyId > 0
				) organization_id INNER JOIN OrganizationPatient op WITH (NOLOCK) ON organization_id.OrganizationId = op.OrganizationId
				INNER JOIN Patient p WITH (NOLOCK) ON op.PatientId = p.PatientId
			WHERE op.Active = 1 AND p.IsActive = 1
		) patient_id ON ef_p.PatientId = patient_id.PatientId");

        public override async Task<IEnumerable<int>> GetOrganizationIdsInPatientScopeForSenderAsync(int memberId, int? organizationId = null)
        {
            using var command = Database.GetDbConnection().CreateCommand();
            var organizationIds = new List<int>();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = @"dbo.SelectOrganizationIdsInPatientScopeForSender";
            command.Parameters.Add(new SqlParameter("@MemberId", memberId));
            command.Parameters.Add(new SqlParameter("@OrganizationId", organizationId.HasValue ? organizationId.Value : DBNull.Value));

            command.Connection.Open();
            using (var reader = await command.ExecuteReaderAsync())
                while (reader.Read())
                {
                    organizationIds.Add((int)reader[0]);
                }
            command.Connection.Close();

            return organizationIds;
        }

        //public override IQueryable<PatientFacilityAssociation> GetAssociatedFacilities(int? patientId)
        //{
        //    return patientId.HasValue ?
        //                  from fp in Facilities_Patients
        //                  where fp.PatientId == patientId.Value
        //                  select fp :
        //                  from fp in Facilities_Patients
        //                  select fp;
        //     //Facilities_Patients.FromSqlInterpolated($"SELECT * FROM dbo.Facilities_Patients WHERE PatientId ={patientId.Value}"); //:
        //    //Facilities_Patients.FromSqlInterpolated($@"SELECT * FROM dbo.Facilities_Patients");
        //}

        //Facilities_Patients.FromSqlInterpolated($@"SELECT * FROM dbo.Facilities_Patients WHERE PatientId ={patientId}") :
        //Facilities_Patients.FromSqlInterpolated($@"SELECT * FROM dbo.Facilities_Patients");



        protected IQueryable<Patient> QueryPatientsForSigner(int memberId, int? organizationId = null)
            => organizationId.HasValue ?
                    Patients.FromSqlInterpolated($@"SELECT p.* FROM
(SELECT DISTINCT p.PatientId
FROM dbo.Patient p WITH (NOLOCK)
	INNER JOIN dbo.LegacyRequest lr WITH (NOLOCK) ON p.PatientId = lr.Patient
	INNER JOIN dbo.OrganizationMember om WITH (NOLOCK) ON lr.Signer = om.OrganizationMemberId
WHERE om.MemberId = {memberId} AND om.OrganizationId = {organizationId.Value} AND p.IsActive = 1 AND (lr.[Status] IS NULL OR lr.[Status] != 3) AND om.IsActive = 1) pids
	INNER JOIN dbo.Patient p WITH (NOLOCK) ON pids.PatientId = p.PatientId") :
               Patients.FromSqlInterpolated($@"SELECT p.* FROM
(SELECT DISTINCT p.PatientId
FROM dbo.Patient p WITH (NOLOCK)
	INNER JOIN dbo.LegacyRequest lr WITH (NOLOCK) ON p.PatientId = lr.Patient
	INNER JOIN dbo.OrganizationMember om WITH (NOLOCK) ON lr.Signer = om.OrganizationMemberId
WHERE om.MemberId = {memberId} AND p.IsActive = 1 AND (lr.[Status] IS NULL OR lr.[Status] != 3) AND om.IsActive = 1) pids
	INNER JOIN dbo.Patient p WITH (NOLOCK) ON pids.PatientId = p.PatientId");

        protected IQueryable<Patient> QueryPatientsForAssistant(int memberId, int? organizationId = null)
            => organizationId.HasValue ?
                    Patients.FromSqlInterpolated($@"SELECT p.* FROM
(SELECT DISTINCT p.PatientId
FROM dbo.Patient p WITH (NOLOCK)
	INNER JOIN dbo.LegacyRequest lr WITH (NOLOCK) ON p.PatientId = lr.Patient
	INNER JOIN dbo.OrganizationMember om WITH (NOLOCK) ON lr.Signer = om.OrganizationMemberId
	INNER JOIN dbo.MemberRelationship mr WITH (NOLOCK) ON om.MemberId = mr.SupervisorMemberId
	INNER JOIN dbo.OrganizationMember oma WITH (NOLOCK) ON mr.SubordinateMemberId = oma.MemberId AND om.OrganizationId = oma.OrganizationId
WHERE mr.SubordinateMemberId = {memberId} AND oma.OrganizationId = {organizationId.Value} AND p.IsActive = 1 AND (lr.[Status] IS NULL OR lr.[Status] != 3) AND om.IsActive = 1 AND oma.IsActive = 1) pids
	INNER JOIN dbo.Patient p WITH (NOLOCK) ON pids.PatientId = p.PatientId") :
               Patients.FromSqlInterpolated($@"SELECT p.* FROM
(SELECT DISTINCT p.PatientId
FROM dbo.Patient p WITH (NOLOCK)
	INNER JOIN dbo.LegacyRequest lr WITH (NOLOCK) ON p.PatientId = lr.Patient
	INNER JOIN dbo.OrganizationMember om WITH (NOLOCK) ON lr.Signer = om.OrganizationMemberId
	INNER JOIN dbo.MemberRelationship mr WITH (NOLOCK) ON om.MemberId = mr.SupervisorMemberId
	INNER JOIN dbo.OrganizationMember oma WITH (NOLOCK) ON mr.SubordinateMemberId = oma.MemberId AND om.OrganizationId = oma.OrganizationId
WHERE mr.SubordinateMemberId = {memberId} AND p.IsActive = 1 AND (lr.[Status] IS NULL OR lr.[Status] != 3) AND om.IsActive = 1 AND oma.IsActive = 1) pids
	INNER JOIN dbo.Patient p WITH (NOLOCK) ON pids.PatientId = p.PatientId");

        protected IQueryable<Patient> SearchPatients(IQueryable<Patient> query, string searchText)
        {
            searchText = searchText.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return query;
            }

            if (Regex.Match(searchText, @"^(?<LastName>[^,]+), (?<FirstName>[^(]+?)(?: *)(?:\(DOB: (?<DOB>[^,]+)(?:, MRN: (?<MRN>[^\)]+))?\))$", RegexOptions.IgnoreCase) is Match lastNameFirstNameMatch && lastNameFirstNameMatch.Success)
            {
                var lastName = lastNameFirstNameMatch.Groups["LastName"].Value;
                var firstName = lastNameFirstNameMatch.Groups["FirstName"].Value;
                var firstNameWords = firstName.Split(' ');

                if (firstNameWords.Length == 1)
                {
                    query = query.Where(p => p.FirstName == firstName);
                }
                else
                {
                    for (var i = 0; i < firstNameWords.Length; ++i)
                    {
                        var word = firstNameWords[i];

                        if (i + 1 < firstNameWords.Length)
                        {
                            query = query.Where(p => EF.Functions.Like(p.FirstName, $"%{word}%"));
                        }
                        else
                        {
                            query = query.Where(p => EF.Functions.Like(p.FirstName, $"%{word}%") || p.Suffix == word);
                        }
                    }
                }
            }
            else
            {
                var words = searchText.Split(' ').Select(w => Regex.Replace(w, @"[^A-Za-z0-9]+", string.Empty));

                foreach (var word in words)
                {
                    query = query.Where(p => EF.Functions.Like(p.FirstName, $"%{word}%") ||
                                             EF.Functions.Like(p.LastName, $"%{word}%") ||
                                             EF.Functions.Like(p.Suffix, $"%{word}%"));
                }
            }

            return query.OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName)
                        .ThenBy(p => p.Suffix)
                        .ThenBy(p => p.Birthdate);
        }
    }
}
