using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.DataScraping
{
    public class ScrapPatientHtmlResponse
    {
        public IList<IIdentifier> Ids { get; set; } = new List<IIdentifier>();

        public DateTime Birthdate { get; set; }
        public Gender Gender { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public bool LogMatches { get; set; } = true;
        public int MemberId { get; set; }
        public int OrganizationId { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? StateOrProvince { get; set; }
        public string? PostalCode { get; set; }
    }
}
