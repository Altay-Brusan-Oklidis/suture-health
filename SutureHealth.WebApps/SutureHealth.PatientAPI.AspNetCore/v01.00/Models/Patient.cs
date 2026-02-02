using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SutureHealth.Patients.v0100.Models
{
   /// <summary>
   /// Enum <see cref="Gender"/> List the member.
   /// </summary>
   [DataContract]
    public enum Gender
    {
        /// <summary>
        /// Undefined
        /// </summary>
        [EnumMember(Value = "0")] Undefined = 0,

        /// <summary>
        /// Male
        /// </summary>
        [EnumMember(Value = "M")] Male,

        /// <summary>
        /// Female
        /// </summary>
        [EnumMember(Value = "F")] Female,

        /// <summary>
        /// Ambiguous
        /// </summary>
        [EnumMember(Value = "A")] Ambiguous,

        /// <summary>
        /// Unknown
        /// </summary>
        [EnumMember(Value = "U")] Unknown 
    }

    /// <summary>
    /// Class <see cref="Patient"/> represent the model of patient class.
    /// </summary>
    public class Patient
    {
        /// <summary>
        /// Gets or Sets the value of Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or Sets the value of FirstName.
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or Sets the value of MiddleName.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or Sets the value of LastName.
        /// </summary>
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or Sets the value of Suffix.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or Sets the value of DateOfBirth.
        /// </summary>
        [Required]
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// Gets or Sets the value of Gender.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or Sets the value of RequestId.
        /// </summary>
        public long? RequestId { get; set; }

        /// <summary>
        /// Gets or Sets the value of SourceOrganizationUnitId.
        /// </summary>
        [Required]
        public long SourceOrganizationUnitId { get; set; }

        /// <summary>
        /// Gets or Sets the value of MasterOrganizationPatientId.
        /// </summary>
        public long? MasterOrganizationPatientId { get; set; }

        /// <summary>
        /// Gets or Sets the object of Addresses.
        /// </summary>
        public virtual ICollection<Address> Addresses { get; set; }
        /// <summary>
        /// Gets or Sets the object of Contacts.
        /// </summary>
        public virtual ICollection<ContactInfo> Contacts { get; set; }
        /// <summary>
        /// Gets or Sets the object of Identifiers.
        /// </summary>
        public virtual ICollection<Identifier> Identifiers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<OrganizationPatient> Organizations { get; set; }

        public override string ToString() => $"Patient[{Id}]: {string.Join(",", LastName, FirstName)} ";

        /// <summary>
        /// Gets or Sets whether the patient record is currently active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
