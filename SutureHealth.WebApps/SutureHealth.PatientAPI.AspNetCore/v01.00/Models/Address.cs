using System;

namespace SutureHealth.Patients.v0100.Models
{
    /// <summary>
    /// Class <see cref="Address"/> used for define the Address
    /// </summary>
    public class Address : IAddress
    {
        /// <summary>
        /// Gets or Sets the value of Line1
        /// </summary>
        public string Line1 { get; set; }
        /// <summary>
        /// Gets or Sets the value of Line2
        /// </summary>
        public string Line2 { get; set; }
        /// <summary>
        /// Gets or Sets the value of City
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// Gets or Sets the value of County
        /// </summary>
        public string County { get; set; }
        /// <summary>
        /// Gets or Sets the value of StateOrProvince
        /// </summary>
        public string StateOrProvince { get; set; }
        /// <summary>
        /// Gets or Sets the value of PostalCode
        /// </summary>
        public string PostalCode { get; set; }
        /// <summary>
        /// Gets or Sets the value of CountryOrRegion
        /// </summary>
        public string CountryOrRegion { get; set; }
        /// <summary>
        /// Gets or Sets the value of Created
        /// </summary>
        public DateTimeOffset Created { get; set; }
        /// <summary>
        /// Gets or Sets the value of Updated
        /// </summary>
        public DateTimeOffset? Updated { get; set; }
        /// <summary>
        /// Gets or Sets the value of Id
        /// </summary>
        public long Id { get; set; }
    }
}
