using System;
using System.Linq;
using System.Collections.Generic;

namespace SutureHealth.Patients
{
    public class Patient
    {
        public Patient()
        {
            Addresses = new List<PatientAddress>();
            Contacts = new List<PatientContact>();
            Identifiers = new List<PatientIdentifier>();
            Phones = new List<PatientPhone>();
        }

        public int PatientId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public DateTime Birthdate { get; set; }
        public Gender Gender { get; set; } = Gender.Unknown;
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<PatientAddress> Addresses { get; set; }
        public virtual ICollection<PatientContact> Contacts { get; set; }
        public virtual ICollection<PatientIdentifier> Identifiers { get; set; }
        public virtual ICollection<PatientOrganizationKey> OrganizationKeys { get; set; }
        public virtual ICollection<PatientPhone> Phones { get; set; }

        public string FullName => Person.GetFullName(LastName, FirstName, Suffix);
        public override string ToString() => $"Patient[{PatientId}]: {string.Join(",", LastName, FirstName)} ";

        public override bool Equals(object obj)
            => (obj is Patient patient) && patient.PatientId == PatientId;

        public override int GetHashCode()
            => PatientId.GetHashCode();
    }

    public class PatientAddress : Address<int, Patient, int> { }
    public class PatientContact : ContactInfo<int, Patient, int> { }
    public class PatientIdentifier : Identifier<long, Patient, int> { }
    public class PatientPhone : ContactInfo<int, Patient, int> { public bool? IsPrimary { get; set; } = null; public bool? IsActive { get; set; } }

    public static class PatientExtensions
    {
        public static string GetSearchSummary(this Patient patient, int? organizationId = null)
        {
            var mrn = patient.OrganizationKeys?.FirstOrDefault(k => organizationId.HasValue && k.OrganizationId == organizationId.Value)?.MedicalRecordNumber;

            return $"{patient.LastName}, {patient.FirstName}" + (!string.IsNullOrWhiteSpace(patient.Suffix) ? $" {patient.Suffix}" : string.Empty) +
                        $" (DOB: {patient.Birthdate:M/d/yyyy}{(!string.IsNullOrWhiteSpace(mrn) ? $", MRN: {mrn}" : string.Empty)})";
        }

        public static bool HasFullSocial(this Patient patient)
        {
            return patient.Identifiers.HasFullSocial();
        }

        public static bool HasLast4Social(this Patient patient)
        {
            return patient.Identifiers.HasLast4Social();
        }
    }

    public static class PatientIdentifierExtensions
    {
        public static bool HasFullSocial(this IEnumerable<PatientIdentifier> identifiers)
        {
            return identifiers.Any(identifier => identifier.Type is KnownTypes.SocialSecurityNumber);
        }

        public static bool HasLast4Social(this IEnumerable<PatientIdentifier> identifiers)
        {
            return identifiers.Any(identifier => identifier.Type is KnownTypes.SocialSecuritySerial);
        }
    }
}
