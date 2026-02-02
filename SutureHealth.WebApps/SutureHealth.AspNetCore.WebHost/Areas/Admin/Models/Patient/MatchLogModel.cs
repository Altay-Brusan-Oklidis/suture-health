using SutureHealth.AspNetCore.Models;
using SutureHealth.Patients;
using SutureHealth.Linq;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.Patient
{
    public enum SocialSecurityNumberStyle
    {
        Unavailable = 0,
        Full,
        Last4
    }

    public class MatchLogModel : BaseViewModel
    {
        public MatchLog MatchPatientLog { get; internal set; } = new MatchLog();
        public IEnumerable<MatchingResult<Patients.Patient>> Matches { get; internal set; }
        public string SocialSecurityNumber { get; set; }
        public SocialSecurityNumberStyle? SocialSecurityNumberType { get; set; }
        public string SocialSecurityNumberMask => SocialSecurityNumberType switch
        {
            SocialSecurityNumberStyle.Full => "000-00-0000",
            SocialSecurityNumberStyle.Last4 => "0000",
            _ => string.Empty
        };
    }
}
