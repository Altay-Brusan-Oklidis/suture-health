using System;

namespace SutureHealth.Patients.v0100.Models
{
    /// <summary>
    /// Class <see cref="ContactInfo"/> used for define the contact information
    /// </summary>
    public class ContactInfo : IContactInfo
    {
        public long Id { get; set; }
        /// <summary>
        /// Define the type of contact like mobile, email, fax etc
        /// </summary>
        public ContactType Type { get; set; }
        /// <summary>
        /// Gets or Sets the value contact
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Gets or Sets the value of Created
        /// </summary>
        public DateTimeOffset Created { get; set; }
        /// <summary>
        /// Gets or Sets the value of Updated
        /// </summary>
        public DateTimeOffset? Updated { get; set; }
    }
}
