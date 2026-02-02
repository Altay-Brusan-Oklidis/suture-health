using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using SutureHealth.Authorities;
namespace SutureHealth.Patients
{
    public enum MatchingRuleset
    {
        DocumentProcessing
    }

    public class PatientMatchingRequest
    {
        [ProtectedPersonalData]
        public ICollection<PatientPhone> Phones { get; set; }
        public IList<IIdentifier> Ids { get; set; } = new List<IIdentifier>();
        public MatchingRuleset Ruleset { get; set; } = MatchingRuleset.DocumentProcessing;

        public DateTime Birthdate { get; set; }
        public Gender Gender { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public bool LogMatches { get; set; } = true;
        public bool ManualReviewEnabled { get; set; } = true;
        public int MemberId { get; set; }
        public int OrganizationId { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string StateOrProvince { get; set; }
        public string PostalCode { get; set; }

        public RequestSource RequestSource { get; set; } = RequestSource.SutureHealth;
        public string SourceDescription { get; set; } = null;

        public PatientMatchingRequest Sanitize()
        {
            var retVal = new PatientMatchingRequest();

            retVal.FirstName = FirstName?.Trim();
            retVal.LastName = LastName?.Trim();
            retVal.PostalCode = PostalCode?.Trim();
            retVal.Birthdate = Birthdate;
            retVal.Gender = Gender;
            retVal.Ids = Ids;
            retVal.Phones = Phones;

            foreach (var identifier in retVal.Ids)
            {
                identifier.Type = identifier.Type.Trim();
                identifier.Value = identifier.Value.Trim();

                if (identifier.IsSocialSecurityIdentifier()) identifier.Value = Regex.Replace(identifier.Value, @"\D+", "");
                if (identifier.IsMedicareBeneficiaryNumber()) identifier.Value = identifier.Value.Replace("-", "");
            }
            if (!retVal.Phones.IsNullOrEmpty())
            {
                foreach (var phone in retVal.Phones)
                {
                    if(phone.Value != null)
                        phone.Value = phone.Value.Trim().Replace("-", "");
                }
            }

            return retVal;
        }

        public string Validate()
        {
            if (FirstName.Length == 0 || !Regex.IsMatch(FirstName, @"^[A-Za-z]+$"))
                return "FirstName is mismatching";

            if (LastName.Length == 0 || !Regex.IsMatch(LastName, @"^[A-Za-z]+$"))
                return "LastName is mismatching";

            if (Gender != Gender.Male && Gender != Gender.Female)
                return "Invalid Gender";

            if (PostalCode.Length == 0 || !Regex.IsMatch(PostalCode, @"^[A-Za-z0-9]+$"))
                return "Invalid PostalCode";

            return "success";
        }
    }
}