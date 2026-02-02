using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SutureHealth.Hchb
{
    public class SuturePatient
    {
        public int PatientId { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string MaidenName { get; set; }

        public string Suffix { get; set; }

        public DateTime DateOfBirth { get; set; }

        public bool Active { get; set; }

        public string Gender { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ChangeDate { get; set; }

        public int ChangeBy { get; set; }

        public ICollection<RequestStatus>? Requests { get; set; }
    }
}
