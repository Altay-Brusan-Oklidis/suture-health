using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SutureHealth.Patients
{
    public class MatchLog
    {
        public int MatchPatientLogID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string MaidenName { get; set; }
        public string Suffix { get; set; }
        public DateTime Birthdate { get; set; }
        public Gender Gender { get; set; }
        public string SocialSecurityNumber { get; set; }
        public string SocialSecuritySerial { get; set; }
        public string MedicareNumber { get; set; }
        //public string SubmittedMedicareMBI { get; set; }
        public string MedicaidNumber { get; set; }
        public string MedicaidState { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public int MemberId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityMRN { get; set; }
        public string Source { get; set; }
        public string HomePhone { get; set; }
        public string WorkPhone { get; set; }
        public string Mobile { get; set; }
        public string OtherPhone { get; set; }
        public string PrimaryPhone { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime ProcessingStartTime { get; set; }
        public DateTime ProcessingEndTime { get; set; }
        public int RecordsEvaluated { get; set; }
        public int MatchAlgorithmID { get; set; }
        public bool? MatchedPatient { get; set; }
        public bool? NeedsReview { get; set; }
        public bool? ManuallyMatched { get; set; }
        public int? ManuallyMatchedBy { get; set; }
        public DateTime? ManuallyMatchedOn { get; set; }
        public bool? MultiplePatientsMatched { get; set; }
        public bool? SimilarPatientHigh { get; set; }
        public bool? SimilarPatientLow { get; set; }

        public bool? IsSelfPay { get; set; }
        public bool? IsPrivateInsurance { get; set; }
        public bool? IsMedicareAdvantage { get; set; }

        public string SourceDescription { get; set; }

        public virtual IEnumerable<MatchOutcome> Outcomes { get; set; }

        public virtual Organization Organization { get; set; }
    }

    public class MatchLogEqualityComparer : IEqualityComparer<MatchLog>
    {
        public bool Equals(MatchLog x, MatchLog y)
        {
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (y == null) return false;
            return x.FirstName == y.FirstName &&
                     x.MiddleName == y.MiddleName &&
                     x.LastName == y.LastName &&
                     x.MaidenName == y.MaidenName &&
                     x.Suffix == y.Suffix &&
                     x.Birthdate == y.Birthdate &&
                     x.Gender == y.Gender &&
                     x.SocialSecurityNumber == y.SocialSecurityNumber &&
                     x.SocialSecuritySerial == y.SocialSecuritySerial &&
                     x.MedicareNumber == y.MedicareNumber &&
                     x.MedicaidNumber == y.MedicaidNumber &&
                     x.MedicaidState == y.MedicaidState &&
                     x.Address1 == y.Address1 &&
                     x.Address2 == y.Address2 &&
                     x.City == y.City &&
                     x.State == y.State &&
                     x.Zip == y.Zip &&
                     x.FacilityId == y.FacilityId &&
                     x.FacilityMRN == y.FacilityMRN &&
                     x.HomePhone == y.HomePhone &&
                     x.WorkPhone == y.WorkPhone &&
                     x.Mobile == y.Mobile &&
                     x.OtherPhone == y.OtherPhone &&
                     x.PrimaryPhone == y.PrimaryPhone &&
                     x.CreateDate.Date == y.CreateDate.Date &&
                     x.ProcessingStartTime.Date == y.ProcessingStartTime.Date &&
                     x.ProcessingEndTime.Date == y.ProcessingEndTime.Date &&
                     x.IsSelfPay == y.IsSelfPay &&
                     x.IsPrivateInsurance == y.IsPrivateInsurance &&
                     x.IsMedicareAdvantage == y.IsMedicareAdvantage;
        }

        public int GetHashCode([DisallowNull] MatchLog obj)
        {
            int hashCode = 348310844;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.FirstName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.MiddleName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.LastName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.MaidenName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.Suffix);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.Birthdate.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.Gender.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.SocialSecurityNumber);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.SocialSecuritySerial);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.MedicareNumber);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.MedicaidNumber);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.MedicaidState);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.Address1);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.Address2);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.City);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.State);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.Zip);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.FacilityId.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.FacilityMRN);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.HomePhone);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.WorkPhone);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.Mobile);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.OtherPhone);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.PrimaryPhone);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.CreateDate.Date.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.ProcessingStartTime.Date.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.ProcessingEndTime.Date.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.IsSelfPay.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.IsPrivateInsurance.ToString());
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.IsMedicareAdvantage.ToString());

            return hashCode;
        }
    }

    public static class MatchLogExtensions 
    {
        public static PatientMatchingRequest ToPatientMatchingRequest(this MatchLog matchLog) 
        {
            var request = new PatientMatchingRequest();

            request.Birthdate = matchLog.Birthdate;
            request.Gender = matchLog.Gender;
            request.FirstName = matchLog.FirstName;
            request.MiddleName = matchLog.MiddleName;
            request.LastName = matchLog.LastName;
            request.Suffix = matchLog.Suffix;
            request.MemberId = matchLog.MemberId;
            request.OrganizationId = matchLog.FacilityId;//matchLog.Organization.OrganizationId;
            request.AddressLine1 = matchLog.Address1;
            request.AddressLine2 = matchLog.Address2;
            request.City = matchLog.City;
            request.StateOrProvince = matchLog.State;
            request.PostalCode = matchLog.Zip;
            request.RequestSource = RequestSource.SutureHealth;
            request.SourceDescription = string.Empty;

            request.Phones = new List<PatientPhone>() 
            {
                new PatientPhone() { Type= ContactType.HomePhone, Value = matchLog.HomePhone, 
                                     IsPrimary = matchLog.PrimaryPhone.EqualsIgnoreCase("HomePhone") ? true : false
                                   },
                new PatientPhone() { Type= ContactType.WorkPhone, Value = matchLog.WorkPhone,
                                     IsPrimary= matchLog.PrimaryPhone.EqualsIgnoreCase("WorkPhone")?true:false
                                   },
                new PatientPhone() { Type= ContactType.OtherPhone, Value = matchLog.OtherPhone,
                                     IsPrimary= matchLog.PrimaryPhone.EqualsIgnoreCase("OtherPhone")?true:false
                                   },
                new PatientPhone() { Type= ContactType.Mobile, Value = matchLog.PrimaryPhone, 
                                     IsPrimary= matchLog.PrimaryPhone.EqualsIgnoreCase("mobile")?true:false
                                   },
                
            };

            if (!string.IsNullOrEmpty(matchLog.SocialSecurityNumber))
            {
                request.Ids.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.SocialSecurityNumber,
                    Value = matchLog.SocialSecurityNumber
                });
                request.Ids.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.SocialSecuritySerial,
                    Value = matchLog.SocialSecurityNumber.GetLast(4)
                });
            }
            else 
            if (!string.IsNullOrEmpty(matchLog.SocialSecuritySerial))
            {
                request.Ids.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.SocialSecuritySerial,
                    Value = matchLog.SocialSecuritySerial
                });
            }

            if (!string.IsNullOrEmpty(matchLog.MedicareNumber))
            {
                request.Ids.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.Medicare,
                    Value = string.Empty
                });
                request.Ids.Add(new Patients.PatientIdentifier()
                {
                    Type = KnownTypes.MedicareBeneficiaryNumber,
                    Value = matchLog.MedicareNumber
                });
            }

            if (!string.IsNullOrEmpty(matchLog.MedicaidNumber))
            { 
                request.Ids.Add(new PatientIdentifier() { Value = matchLog.MedicaidNumber, Type = KnownTypes.MedicaidNumber });
                request.Ids.Add(new PatientIdentifier() { Value = matchLog.MedicaidState, Type = KnownTypes.MedicaidState });
            }

            if (matchLog.IsSelfPay == true)
            {
                request.Ids.Add(new PatientIdentifier() 
                { 
                    Type = KnownTypes.SelfPay, 
                    Value = "true" 
                });
            }

            if (matchLog.IsMedicareAdvantage == true)
            {
                request.Ids.Add(new PatientIdentifier() 
                { 
                    Type = KnownTypes.MedicareAdvantage, 
                    Value = "true" 
                });
            }
            
            if (matchLog.IsPrivateInsurance == true)
            {
                request.Ids.Add(new PatientIdentifier() 
                { 
                    Type = KnownTypes.PrivateInsurance, 
                    Value = "true" 
                });
            }

            if (!string.IsNullOrEmpty(matchLog.FacilityMRN))
            {
                request.Ids.Add(new PatientIdentifier() { Value = matchLog.FacilityMRN, Type = KnownTypes.UniqueExternalIdentifier });
            }

            return request;
        }
    }
    public class MatchOutcome
    {
        public int MatchPatientOutcomeID { get; set; }
        public int MatchPatientLogID { get; set; }
        public int PatientId { get; set; }
        public int MatchScore { get; set; }
        public DateTime CreateDate { get; set; }
        public bool MatchRejected { get; set; }
        public bool PatientCreated { get; set; }

        public virtual MatchLog MatchLog { get; set; }
        public virtual Patient Patient { get; set; }
    }
}
