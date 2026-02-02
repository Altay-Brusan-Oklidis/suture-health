

namespace SutureHealth.DataScraping
{
    public class ScrapedPatientDetailHistory
    {
        public Guid Id { get; set; }
        public string? URL { get; set; }
        public string? ExternalId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? MaritalStatus { get; set; }
        public string? SexualOrientation { get; set; }
        public string? AttendedPhysician { get; set; }

        //address
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        //billing
        public string? PatientBalance { get; set; }
        public string? InsuranceBalance { get; set; }
        public string? TotalBalance { get; set; }

        //Occupation
        public string? Occupation { get; set; }
        public string? Employer { get; set; }

        //Stats
        public string? Language { get; set; }
        public string? Ethnicity { get; set; }
        public string? Race { get; set; }
        public string? FamilySize { get; set; }
        public string? MonthlyIncome { get; set; }
        public string? Religion { get; set; }

        //Decease
        public string? DeceaseDate { get; set; }
        public string? DeceaseReason { get; set; }


        public string? SSN { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<ObservationHistory> Observations { get; set; }
        public virtual ICollection<ContactHistory> Contacts { get; set; }
        public virtual ICollection<ConditionHistory> Conditions { get; set; }
        public virtual ICollection<AllergyHistory> Allergies { get; set; }
        public virtual ICollection<MedicationHistory> Medications { get; set; }
        public virtual ICollection<ImmunizationHistory> Immunizations { get; set; }
        public virtual ICollection<PrescriptionHistory> Prescriptions { get; set; }
        public virtual ICollection<ProcedureHistory> Procedures { get; set; }


        public ScrapedPatientDetailHistory()
        {

        }

        public ScrapedPatientDetailHistory(string externalId, string firstName,
                                         string middleName, string lastName,
                                         DateTime? dateOfBirth, string gender,
                                         string maritalStatus, string sexualOrientation,
                                         string attendedPhysician, string address,
                                         string city, string country,
                                         string postalCode, string patientBalance,
                                         string insuranceBalance, string totalBalance,
                                         string occupation,
                                         string employer, string language,
                                         string ethnicity, string race,
                                         string familySize, string monthlyIncome,
                                         string religion, string deceaseDate,
                                         string deceaseReason, string ssn,
                                         ICollection<ObservationHistory> observations,
                                         ICollection<ContactHistory> contacts,
                                         ICollection<ConditionHistory> conditions,
                                         ICollection<AllergyHistory> allergies,
                                         ICollection<MedicationHistory> medications,
                                         ICollection<ImmunizationHistory> immunizations,
                                         ICollection<PrescriptionHistory> prescriptions,
                                         ICollection<ProcedureHistory> procedures)
        {
            ExternalId = externalId;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            MaritalStatus = maritalStatus;
            SexualOrientation = sexualOrientation;
            AttendedPhysician = attendedPhysician;
            Address = address;
            City = city;
            Country = country;
            PostalCode = postalCode;
            PatientBalance = patientBalance;
            TotalBalance = totalBalance;
            InsuranceBalance = insuranceBalance;
            Occupation = occupation;
            Employer = employer;
            Language = language;
            Ethnicity = ethnicity;
            Race = race;
            FamilySize = familySize;
            MonthlyIncome = monthlyIncome;
            Religion = religion;
            DeceaseDate = deceaseDate;
            DeceaseReason = deceaseReason;
            SSN = ssn;

            Observations = observations;
            Contacts = contacts;
            Conditions = conditions;
            Allergies = allergies;
            Medications = medications;
            Immunizations = immunizations;
            Prescriptions = prescriptions;
            Procedures = procedures;

            CreatedAt = DateTime.Now;
        }
    }
}
