
namespace SutureHealth.DataScraping
{
    public class ScrapedPatient
    {
        public Guid Id { get; set; }
        public string? URL { get; set; }
        public string? ExternalId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? SSN { get; set; }
        public DateTime? DateOfBirth { get; set; }        
        public string? AttendedPhysician { get; set; }
        public DateTime CreatedAt { get; set; }

        public ScrapedPatient()
        {

        }
       

        public ScrapedPatient(string fullName, string phone, string ssn, DateTime? dateOfBirth, string externalId, string attendedPhysician, DateTime createdAt)
        {
            Id = Guid.NewGuid();
            var splittedFullName = fullName.Split(new string[] { ", ", " " }, StringSplitOptions.RemoveEmptyEntries);
            if(splittedFullName.Count() == 3)
            {
                FirstName = splittedFullName[1];
                MiddleName = splittedFullName[2];
                LastName = splittedFullName[0];
            }
            else if(splittedFullName.Count() == 2)
            {
                FirstName = splittedFullName[1];
                LastName = splittedFullName[0];
            }
            Phone = phone;
            SSN = ssn;
            DateOfBirth = dateOfBirth;
            ExternalId = externalId;
            AttendedPhysician = attendedPhysician;
            CreatedAt = createdAt;            
        }

        public ScrapedPatient Update(string firstName, string middleName, string lastName, string phone, string ssn, DateTime? dateOfBirth, string attendedPhysician, DateTime createdAt)
        {
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            Phone = phone;
            SSN = ssn;
            DateOfBirth = dateOfBirth;
            AttendedPhysician = attendedPhysician;
            CreatedAt = createdAt;

            return this;
        }
    }
}
