using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.WebHost.Areas.Visit.Models
{
    public class IndexViewModel : BaseViewModel
    {
        public IEnumerable<Visit> Visits { get; set; }

        public class Visit
        {
            public string PublicId { get; set; }
            public bool Active { get; set; }
            public string ParticipantPhoneNumberFormatted { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public string LastPatientName { get; set; }
            public string LastPatientSessionPhoneNumber { get; set; }
            public bool PatientHasJoined { get; set; }
            public bool PatientCurrentlyJoined { get; set; }
            public double JoinedMinutes { get; set; }
            public DateTimeOffset? LastPatientSessionCreatedAt { get; set; }
        }
    }
}
