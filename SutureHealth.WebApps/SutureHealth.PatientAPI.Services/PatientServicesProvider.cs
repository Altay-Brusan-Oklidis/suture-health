using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Slack.Webhooks;
using Slack.Webhooks.Blocks;
using Slack.Webhooks.Elements;
using SutureHealth.Application.Services;
using SutureHealth.Authorities;
using SutureHealth.Diagnostics;
using SutureHealth.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml;

namespace SutureHealth.Patients.Services
{
    public class PatientServicesProvider : IPatientServicesProvider
    {
        protected PatientDbContext PatientContext { get; set; }
        protected ILogger<IPatientServicesProvider> Logger { get; }
        protected ITracingService Tracer { get; }
        protected IServiceProvider ServiceProvider { get; }        
        protected ISlackClient SlackClient;
        protected IApplicationService SecurityService;

        private string SlackChannel;
        private string WebhookUrl;

        public PatientServicesProvider
        (
            PatientDbContext patientContext,
            ILogger<IPatientServicesProvider> logger,
            ITracingService tracer,
            IServiceProvider serviceProvider,
            IConfiguration configuration = null,
            IApplicationService securityService = null
        )
        {
            Logger = logger;
            PatientContext = patientContext;
            ServiceProvider = serviceProvider;
            Tracer = tracer;
            if(configuration != null) 
            {
                WebhookUrl = configuration["SutureHealth:PatientServicesNotification:WebhookUrlToAlertPatientMatch"];
                SlackChannel = configuration["SutureHealth:PatientServicesNotification:AlertPatientMatchSlackChannel"];
                SlackClient = new SlackClient(WebhookUrl);
            }     
            if(securityService != null) 
            {
                SecurityService = securityService;
            }
        }

        public async Task<Patient> CreateAsync(Patient patient, int organizationId, int memberId)
        {
            var patientId = await PatientContext.CreatePatientAsync(patient, organizationId, memberId);
            return await GetByIdAsync(patientId);
        }

        public IQueryable<Patient> GetByExternalIdentifier(int organizationId, string mrn) =>
            PatientContext.Patients.Where(p => p.OrganizationKeys.Any(ok => ok.OrganizationId == organizationId && ok.MedicalRecordNumber != null && string.Equals(ok.MedicalRecordNumber, mrn)));

        public async Task<Patient> GetByIdAsync(int patientId, int? senderOrganizationId = null) =>
            await (senderOrganizationId.HasValue ? PatientContext.QueryPatientsForSenderByOrganizationId(senderOrganizationId.Value) : PatientContext.Patients)
                .Include(p => p.Addresses)
                .Include(p => p.Identifiers)
                .Include(p => p.OrganizationKeys)
                .Include(p => p.Phones)
                .Where(p => p.PatientId == patientId)
                .FirstOrDefaultAsync();

        public IQueryable<Patient> GetByIdentifier(string type, string value) =>
            PatientContext.Patients.Where(p => p.Identifiers.Any(i => i.Type == type && i.Value == value));

        public IQueryable<MatchLog> GetMatchLogByFilter(Expression<Func<MatchLog, bool>> predicate) =>
            PatientContext.GetMatchLogByFilter(predicate);

        public IQueryable<MatchLog> GetAllMatchLogs() =>
            PatientContext.GetAllMatchLogs();

        Task<MatchLog> IPatientServicesProvider.GetMatchLogByIdAsync(int matchPatientLogId) =>
            PatientContext.GetMatchLogByIdAsync(matchPatientLogId);

        public async Task LogPatientMatch
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
            IEnumerable<PatientPhone> phones,
            IEnumerable<IMatchingResult<Patient>> matches,
            RequestSource source = RequestSource.SutureHealth,
            string sourceDescription = ""
        ) => await PatientContext.LogPatientMatch(
                                    firstName: firstName,
                                    middleName: middleName,
                                    lastName: lastName,
                                    suffix: suffix,
                                    dateOfBirth: dateOfBirth,
                                    gender: gender,
                                    socialSecurityNumber: socialSecurityNumber,
                                    medicareMBI: medicareMBI,
                                    medicaidNumber: medicaidNumber,
                                    medicaidState: medicaidState,
                                    facilityMRN: facilityMRN,
                                    addressLine1: addressLine1,
                                    addressLine2: addressLine2,
                                    city: city,
                                    stateOrProvince: stateOrProvince,
                                    postalCode: postalCode,
                                    matchedPatient: matchedPatient,
                                    needsReview: needsReview,
                                    manuallyMatched: manuallyMatched,
                                    manuallyMatchedBy: manuallyMatchedBy,
                                    manuallyMatchedOn: manuallyMatchedOn,
                                    multiplePatientsMatched: multiplePatientsMatched,
                                    similiarPatientHigh: similiarPatientHigh,
                                    similiarPatientLow: similiarPatientLow,
                                    startProcessing: startProcessing,
                                    endProcessing: endProcessing,
                                    recordsEvaluated: recordsEvaluated,
                                    submittedBy: submittedBy,
                                    organizationId: organizationId,
                                    isSelfPay: isSelfPay,
                                    isPrivateInsurance: isPrivateInsurance,
                                    isMedicareAdvantage: isIsMedicareAdvantage,
                                    source: source,
                                    sourceDescription: sourceDescription,
                                    matches: matches,
                                    phones: phones);

        public async Task<PatientMatchingResponse> MatchAsync(PatientMatchingRequest model)
        {
            var startProcessing = DateTime.UtcNow;
            var request = model.Sanitize();
            PatientMatchingResponse matches = null;
            switch (model.Ruleset)
            {
                case MatchingRuleset.DocumentProcessing:
                    matches = await DocumentProcessingMatchAsync(request);
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(request.Ruleset)} specified: {request.Ruleset}");
            }

            var stopProcessing = DateTime.UtcNow;
            if (model.LogMatches)
            {
                bool matchedPatient = matches.MatchResults.Count() == 1 && matches.TopMatch != null;
                bool multiplePatientsMatched = matches.MatchResults.Count() > 1 && matches.TopMatch != null;
                bool similiarPatientHigh = matches.MatchLevel == MatchLevel.SimilarHighRisk;
                bool similiarPatientLow = matches.MatchLevel == MatchLevel.Similar;

                bool needsReview = false;
                if (model.ManualReviewEnabled == true)
                {
                    if (matchedPatient == true) needsReview = false;
                    if (similiarPatientHigh == true || similiarPatientLow == true || multiplePatientsMatched == true) needsReview = true;

                }
                

                await PatientContext.LogPatientMatch
                (
                    firstName: model.FirstName, middleName: model.MiddleName ?? string.Empty, lastName: model.LastName, suffix: model.Suffix, dateOfBirth: model.Birthdate, gender: model.Gender,
                    socialSecurityNumber: model.Ids.GetSocialSecurityNumberOrDefault()?.Value ?? model.Ids.GetSocialSecuritySerialOrDefault()?.Value,
                    medicareMBI: model.Ids.GetMedicareMedicareBeneficiaryNumberOrDefault()?.Value.ToUpper(),
                    medicaidNumber: model.Ids.GetMedicaidNumberOrDefault()?.Value?.ToUpper(),
                    medicaidState: model.Ids.GetMedicaidStateOrDefault()?.Value?.ToUpper(),
                    facilityMRN: model.Ids.GetUniqueExternalIdentifierOrDefault()?.Value?.ToUpper(),
                    addressLine1: model.AddressLine1 ?? string.Empty,
                    addressLine2: model.AddressLine2 ?? string.Empty,
                    city: model.City ?? string.Empty,
                    stateOrProvince: model.StateOrProvince ?? string.Empty,
                    postalCode: model.PostalCode,
                    matchedPatient: matchedPatient,
                    needsReview: needsReview,
                    manuallyMatched: null,
                    manuallyMatchedBy: null,
                    manuallyMatchedOn: null,
                    multiplePatientsMatched: multiplePatientsMatched,
                    similiarPatientHigh: similiarPatientHigh,
                    similiarPatientLow: similiarPatientLow,
                    startProcessing: startProcessing,
                    endProcessing: stopProcessing,
                    recordsEvaluated: matches.MatchResults.Count(),
                    submittedBy: model.MemberId,
                    organizationId: model.OrganizationId,
                    matches: matches.MatchResults,
                    source: model.RequestSource,
                    sourceDescription: model.SourceDescription,
                    isSelfPay: model.Ids.IsThereSelfPayIdentifier(),
                    isPrivateInsurance: model.Ids.IsTherePrivateInsuranceIdentifier(),
                    isMedicareAdvantage: model.Ids.IsThereMedicareAdvantage(),
                    phones: model.Phones
                );


                if (needsReview == true && model.ManualReviewEnabled == true && SlackClient != null)
                {
                    // no need to send notifiactions for test
                    
                    /* Removing until we can make the notifications unique */
                    // var organization = await SecurityService.GetOrganizationByIdAsync(model.OrganizationId);
                    // await SendMatchedPatientNotification(organization.Name);
                }

            }
            try
            {
                var cloudwatch = ServiceProvider.GetService<IAmazonCloudWatch>();
                if (cloudwatch != null)
                {
                    await cloudwatch.PutMetricDataAsync(new PutMetricDataRequest
                    {
                        Namespace = $"SutureHealth.PatientAPI.Services-{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToUpper() ?? "NA"}",
                        MetricData = new List<MetricDatum>
                        {
                            new MetricDatum
                            {
                                MetricName = "PatientMatchingTime",
                                Value = (stopProcessing - startProcessing).TotalMilliseconds,
                                Unit = StandardUnit.Milliseconds,
                                TimestampUtc = DateTime.UtcNow,
                                Dimensions = new List<Dimension>
                                {
                                    new Dimension
                                    {
                                        Name = "Ruleset",
                                        Value = model.Ruleset.ToString()
                                    }
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Failed to send CloudWatch Metric");
            }

            return matches;
        }

        public IQueryable<Patient> SearchPatientsForSigner(int memberId, string searchText, int? organizationId = null)
            => PatientContext.SearchPatientsForSigner(memberId, searchText, organizationId);

        public IQueryable<Patient> SearchPatientsForAssistant(int memberId, string searchText, int? organizationId = null)
            => PatientContext.SearchPatientsForAssistant(memberId, searchText, organizationId);

        protected async Task<PatientMatchingResponse> DocumentProcessingMatchAsync(PatientMatchingRequest request)
        {
            const int PATIENT_MATCHING_THRESHOLD = 100;
            const int SIMILAR_HI_MIN = 57;
            const int NON_MATCH_MAX = 0;

            var rules = new List<IMatchingRule<Patient>>();
            var idRules = new List<IMatchingRule<Patient>>();
            var demographicRules = new List<IMatchingRule<Patient>>();
            var fuzzyRules = new List<IMatchingRule<Patient>>();

            foreach (var id in request.Ids.Where(i => i.IsUsedForMatching()))
            {
                var positiveScore = 81;
                var isDbRule = false;
                var negativeScore = -51;

                if (id.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial))
                {
                    positiveScore = 51;
                }

                if (id.Type.EqualsIgnoreCase(KnownTypes.MedicaidState))
                {
                    positiveScore = 0;
                    negativeScore = -81;
                }

                if (id.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber))
                {
                    idRules.CreateMatchingRule(m => m.Identifiers != null &&
                                                    m.Identifiers.Any(ident =>
                                                        ident.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber) ||
                                                        ident.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial)) &&
                                                    (!m.Identifiers.Any(ident => ident.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber)) ||
                                                     m.Identifiers.Any(ident =>
                                                         ident.Type.EqualsIgnoreCase(KnownTypes.SocialSecurityNumber) &&
                                                         ident.Value.EqualsIgnoreCase(id.Value))) &&
                                                    (!m.Identifiers.Any(ident => ident.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial)) ||
                                                     m.Identifiers.Any(ident =>
                                                         ident.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial) &&
                                                         ident.Value.EqualsIgnoreCase(
                                                             id.Value.Substring(id.Value.Length - 4)))),
                        positiveMatchScore: positiveScore,
                        negativeMatchScore: negativeScore,
                        isDbRule: false,
                        nullMatchingExpression: m =>
                            m.Identifiers.Any(ident => ident.Type.EqualsIgnoreCase(id.Type)) ||
                            m.Identifiers.Any(ident => ident.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial)),
                        description: $"m.id.Id == {id.Value} && m.id.Type == {id.Type}");
                }
                else
                {
                    //Last4 is not a rule that we need to keep track of for exceptions.
                    // Last4 is re-evaluated in the rules section. In some rules, it is removed in the others not
                    idRules.CreateMatchingRule(m => m.Identifiers != null &&
                                                    m.Identifiers.Any(ident =>
                                                        ident.Value.EqualsIgnoreCase(id.Value) &&
                                                        ident.Type.EqualsIgnoreCase(id.Type)),
                        positiveMatchScore: positiveScore,
                        negativeMatchScore: negativeScore,
                        isDbRule: false,
                        nullMatchingExpression: m => m.Identifiers.Any(ident => ident.Type.EqualsIgnoreCase(id.Type)),
                        description: $"m.id.Id == {id.Value} && m.id.Type == {id.Type}");
                }
            }

            var ids = request.Ids.Where(i => i.IsUsedForMatching())
                                 .Where(i => !i.Type.EqualsIgnoreCase(KnownTypes.SocialSecuritySerial))
                                 .Select(i => i.Value)
                                 .ToArray();
            rules.CreateMatchingRule
            (
                enumerableMatchingExpression: null,
                queryableMatchingExpression: p => p.Identifiers.Any(i => ids.Contains(i.Value)),
                positiveMatchScore: 0,
                negativeMatchScore: 0,
                isDbRule: true,
                description: $"p.Identifiers.Any(i => new [] {string.Join(", ", ids)}.Contains(i.Id))"
            );

            rules.AddRange(idRules);
            demographicRules.CreateFuzzyRule(m => m.LastName, request.LastName, 53, -33);
            demographicRules.CreateFuzzyRule(m => m.FirstName, request.FirstName, 32, -29, isDbRule: false);
            demographicRules.CreateMatchingRule(m => m.Birthdate.Date == request.Birthdate.Date, positiveMatchScore: 53, negativeMatchScore: -41, description: $"m.DateOfBirth == {request.Birthdate.ToString("O")}");
            rules.AddRange(demographicRules);
            rules.CreateMatchingRule(m => m.Gender == request.Gender.GetEnumMemberValue().ToEnum<Gender>(),
                                     positiveMatchScore: 4,
                                     negativeMatchScore: -48,
                                     isDbRule: false,
                                     description: $"m.Gender == {request.Gender.GetEnumMemberValue()}");
            if (!request.PostalCode.IsNullOrWhiteSpace())
                rules.CreateMatchingRule(m => m.Addresses != null && m.Addresses.Where(a => !a.PostalCode.IsNullOrEmpty())
                                                                                .Any(a => a.PostalCode == request.PostalCode || a.PostalCode.Substring(0, a.PostalCode.Length < 5 ? a.PostalCode.Length : 5) == request.PostalCode.Substring(0, 5)),
                                         positiveMatchScore: 8, negativeMatchScore: 0,
                                         isDbRule: false,
                                         description: $"m.PostalCode == {request.PostalCode} || PostalCode == {request.PostalCode.Substring(0, 5)}");
            fuzzyRules.AddRange(rules.OfType<FuzzyMatchingRule<Patient>>());

            var matchResult = PatientContext.Patients.AsNoTracking()
                                                     .Include(p => p.Identifiers)
                                                     .Include(p => p.Addresses)
                                                     .Where(p => p.IsActive)
                                                     .Match(primaryKeyProperty: x => x.PatientId, rules: rules.ToArray());

            var response = matchResult.AsMatchResponse(PATIENT_MATCHING_THRESHOLD, (topMatch, secondMatch, threshold) =>
            {
                if (MatchingResult.IsAboveThresholdAndNotTheSameScore(topMatch, secondMatch, threshold))
                    return MatchLevel.Match;
                if (topMatch != null && topMatch.Score >= SIMILAR_HI_MIN)
                    return MatchLevel.SimilarHighRisk;
                if (topMatch != null && topMatch.Score > NON_MATCH_MAX)
                    return MatchLevel.Similar;

                return MatchLevel.NonMatch;
            });



            if (response != null)
            {
                //var winningMatch = response.MatchResults.OrderByDescending(m => m.Score).FirstOrDefault();
                var primaryDemographicRules = demographicRules.Where(r => !(r is FuzzyMatchingRule<Patient>));
                foreach (var match in response.MatchResults.OrderByDescending(m => m.Score))
                {
                    //Rule 1
                    //One Matching Id and one non-matching Id
                    if (match.Rules.Any(mr => mr.Score > 0 && idRules.Any(idRule => idRule.Equals(mr.Rule) && (!idRule.Description.Contains("ss4")))) && match.Rules.Any(mr => mr.Score <= 0 && idRules.Any(idRule => idRule.Equals(mr.Rule))))
                    {
                        match.Rules = match.Rules.Append(new GoldenTicketRuleResult<Patient> { Rule = new MatchingRule("Rule 1: One Matching Id and one non-matching Id") });
                        response.MatchLevel = MatchLevel.SimilarHighRisk;
                    }
                    //Rule 2
                    //If more than 1 patient is > threshold, Adjust to SimilarHighRisk so it goes to support
                    if (response.MatchResults.Count(mr => mr.Score > PATIENT_MATCHING_THRESHOLD) > 1)
                    {
                        match.Rules = match.Rules.Append(new GoldenTicketRuleResult<Patient> { Rule = new MatchingRule("Rule 2: If more than 1 patient is > threshold, Adjust to SimilarHighRisk so it goes to support") });
                        response.MatchLevel = MatchLevel.SimilarHighRisk;
                    }
                    //Rule 3
                    //If score < SimilarHighRisk and any of the Ids match, Adjust to SimilarHighRisk so it goes to support
                    if (response.MatchLevel < MatchLevel.SimilarHighRisk && match.Rules.Any(mr => mr.Score > 0 && idRules.Any(idRule => idRule.Equals(mr.Rule) && (!idRule.Description.Contains("ssn4")))))
                    {
                        match.Rules = match.Rules.Append(new GoldenTicketRuleResult<Patient> { Rule = new MatchingRule("Rule 3: If score < SimilarHighRisk and any of the Ids match, Adjust to SimilarHighRisk so it goes to support") });
                        response.MatchLevel = MatchLevel.SimilarHighRisk;
                    }
                    //Rule 4
                    //2 of 3 primaryDemographicRules Do Not Match AND No Fuzzys And any of the Ids match, Adjust to SimilarHighRisk so it goes to support
                    if (match.Rules.Count(mr => mr.Score <= 0 && primaryDemographicRules.Contains(mr.Rule as IMatchingRule)) >= 2 && !match.Rules.Any(mr => mr.Score > 0 && fuzzyRules.Any(fuz => fuz.Equals(mr.Rule))) && match.Rules.Any(mr => mr.Score > 0 && idRules.Contains(mr.Rule as IMatchingRule) && (!mr.Rule.Description.Contains("ssn4"))))
                    {
                        match.Rules = match.Rules.Append(new GoldenTicketRuleResult<Patient> { Rule = new MatchingRule("Rule 4: 2 of 3 Primary Demographic Rules do NOT match AND NO fuzzys And any of the Ids match, Adjust to SimilarHighRisk so it goes to support") });
                        response.MatchLevel = MatchLevel.SimilarHighRisk;
                    }
                    //Rule 5
                    //3 of 3 primaryDemographicRules Do Not Match And any of the Ids match, Adjust to SimilarHighRisk so it goes to support
                    if (match.Rules.Where(mr => primaryDemographicRules.Contains(mr.Rule as IMatchingRule)).All(mr => mr.Score <= 0) && match.Rules.Any(mr => mr.Score > 0 && idRules.Contains(mr.Rule as IMatchingRule) && (!mr.Rule.Description.Contains("ssn4"))))
                    {
                        match.Rules = match.Rules.Append(new GoldenTicketRuleResult<Patient> { Rule = new MatchingRule("Rule 5: 3 of 3 Primary Demographic Rules Do Not Match And any of the Ids match, Adjust to SimilarHighRisk so it goes to support") });
                        response.MatchLevel = MatchLevel.SimilarHighRisk;
                    }
                    //Rule 6
                    //3 of 3 primaryDemographicRules Match And any of the Ids don't match, Adjust to SimilarHighRisk so it goes to support
                    if (match.Rules.Where(mr => primaryDemographicRules.Contains(mr.Rule as IMatchingRule)).All(mr => mr.Score >= 0) && match.Rules.Any(mr => mr.Score < 0 && idRules.Contains(mr.Rule as IMatchingRule)))
                    {
                        match.Rules = match.Rules.Append(new GoldenTicketRuleResult<Patient> { Rule = new MatchingRule("Rule 6: 3 of 3 Primary Demographic Rules Match And any of the Ids don't match, Adjust to SimilarHighRisk so it goes to support") });
                        response.MatchLevel = MatchLevel.SimilarHighRisk;
                    }
                }

                return new PatientMatchingResponse
                {
                    MatchLevel = response.MatchLevel,
                    TopMatch = response.TopMatch,
                    MatchResults = response.MatchResults,
                    Threshold = response.Threshold
                };
            }

            return null;
        }

        public async Task UpdateAsync(Patient patient, int organizationId, int memberId)
            => await PatientContext.UpdatePatientAsync(patient, organizationId, memberId);

        public async Task ResetPayerMixFlags(int patientId, int ChangeBy)  
            => await PatientContext.ResetPatientPayerMixFlagsAsync(patientId, ChangeBy);
        

        public IQueryable<Patient> QueryPatients()
            => PatientContext.Patients.AsNoTracking()
                                      .Include(p => p.Identifiers)
                                      .Include(p => p.OrganizationKeys);

        public IQueryable<MatchLog> QueryMatchLogs()
            => PatientContext.MatchLogs.AsNoTracking()
                              .Include(m => m.Organization);

        public IQueryable<Patient> QueryPatientsForSenderByMemberId(int memberId)
            => PatientContext.QueryPatientsForSenderByMemberId(memberId);

        public IQueryable<Patient> QueryPatientsForSenderByOrganizationId(int organizationId)
            => PatientContext.QueryPatientsForSenderByOrganizationId(organizationId);

        public async Task<IEnumerable<int>> GetOrganizationIdsInPatientScopeForSenderAsync(int memberId, int? organizationId = null)
            => await PatientContext.GetOrganizationIdsInPatientScopeForSenderAsync(memberId, organizationId);

        public IQueryable<MatchOutcome> GetMatchOutcome() => PatientContext.MatchLogOutcomes.AsQueryable();

        public async Task SetPatientMatchLogFlagsToResolved(int matchLogId, int userId)
        {
            var logItem = await PatientContext.MatchLogs.FirstOrDefaultAsync(i => i.MatchPatientLogID == matchLogId);
            var similarItems = PatientContext.MatchLogs.Where(m =>
                           m.FirstName == logItem.FirstName &&
                           m.MiddleName == logItem.MiddleName &&
                           m.LastName == logItem.LastName &&
                           m.MaidenName == logItem.MaidenName &&
                           m.Suffix == logItem.Suffix &&
                           m.Birthdate == logItem.Birthdate &&
                           m.Gender == logItem.Gender &&
                           m.SocialSecurityNumber == logItem.SocialSecurityNumber &&
                           m.SocialSecuritySerial == logItem.SocialSecuritySerial &&
                           m.MedicareNumber == logItem.MedicareNumber &&
                           m.MedicaidNumber == logItem.MedicaidNumber &&
                           m.MedicaidState == logItem.MedicaidState &&
                           m.Address1 == logItem.Address1 &&
                           m.Address2 == logItem.Address2 &&
                           m.City == logItem.City &&
                           m.State == logItem.State &&
                           m.Zip == logItem.Zip &&
                           m.MemberId == logItem.MemberId &&
                           m.FacilityId == logItem.FacilityId &&
                           m.FacilityMRN == logItem.FacilityMRN &&
                           m.HomePhone == logItem.HomePhone &&
                           m.WorkPhone == logItem.WorkPhone &&
                           m.Mobile == logItem.Mobile &&
                           m.OtherPhone == logItem.OtherPhone &&
                           m.PrimaryPhone == logItem.PrimaryPhone &&
                           m.IsSelfPay == logItem.IsSelfPay &&
                           m.IsPrivateInsurance == logItem.IsPrivateInsurance &&
                           m.IsMedicareAdvantage == logItem.IsMedicareAdvantage).ToList();
            
            foreach (var log in similarItems)
            {
                log.ManuallyMatched = true;
                log.ManuallyMatchedOn = DateTime.UtcNow;
                log.ManuallyMatchedBy = userId;
            }

            logItem.ManuallyMatched = true;
            logItem.ManuallyMatchedOn = DateTime.UtcNow;
            logItem.ManuallyMatchedBy = userId;
            await PatientContext.SaveChangesAsync();
        }

        public async Task<bool> TryDisableNeedReviewForMatchLogInstance(int matchlogId)
        {
            var matchLogItem = PatientContext.MatchLogs.FirstOrDefault(m => m.MatchPatientLogID == matchlogId);
            if (matchLogItem == null)
                return false;
            matchLogItem.NeedsReview = false;
            PatientContext.MatchLogs.Update(matchLogItem);
            await PatientContext.SaveChangesAsync();
            return true;
        }

        public async Task UpdatePatientSocialSecurityInfo(int patientId, string ssn, string ssn4, int changeBy) =>
            await PatientContext.UpdatePatientSocialSecurityAsync(patientId, ssn, ssn4, changeBy);


        public Task<bool> SendMatchedPatientNotification(string from)
        {
            string time = DateTime.UtcNow.TimeOfDay.ToString("hh\\:mm\\:ss");
            var head = new Section
            {
                Text = new TextObject
                {
                    Text = $"*From*: _{from}_ \n *Submitted*: {DateTime.UtcNow.Date.ToString("MM/dd/yyyy")} on {DateTime.UtcNow.TimeOfDay.ToString("hh\\:mm\\:ss")} (UTC) \n *Source*: SutureHealth" ,
                    Type = TextObject.TextType.Markdown

                }
            };
            

            SlackMessage _message = new()
            {
                Channel = SlackChannel,
                Blocks = new List<Block>() { head },
            };

            _message.Blocks.Add(new Divider());

            return SlackClient.PostAsync(_message);
        }



    }
}
