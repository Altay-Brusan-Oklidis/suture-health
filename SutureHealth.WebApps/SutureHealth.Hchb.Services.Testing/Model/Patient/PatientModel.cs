using System.ComponentModel.DataAnnotations;
using SutureHealth.Hchb.Services.Testing.Model.Address;

namespace SutureHealth.Hchb.Services.Testing.Model.Patient
{
    [Serializable]
    public class PatientModel
    {
        [StringLength(4)]
        public string? SetId { get; set; }

        // HCHB renamed PID.2 -PatientId to external PatientId
        [StringLength(12)]
        public string? ExternalPatientId { get; set; }
        [Required]
        public List<PatientIdentifierType> PatientId { get; set; } = new List<PatientIdentifierType>();
        public PatientIdentifierType? AlternatePatientId { get; set; }

        [Required]
        [StringLength(50)]
        public PatientNameType Name { get; set; } = new();
        public string? DateOfBirth { get; set; }
        public GenderType Sex { get; set; }
        public string? Race { get; set; }
        public AddressType? Address { get; set; }
        public string? HomePhoneNumber { get; set; }
        public string? BusinessPhoneNumber { get; set; }        
        public string? PrimaryLanguage { get; set; }
        public MaritalStatus? MaritalStatus { get; set; }
        [Required]
        [StringLength(12)]
        public string AccountNumber { get; set; }= string.Empty;
        public string? SSN { get; set; }
        public string? DeathDateAndTime { get; set; }
        public PatientDeathIndicator? DeathIndicator { get; set; }
    }
}
