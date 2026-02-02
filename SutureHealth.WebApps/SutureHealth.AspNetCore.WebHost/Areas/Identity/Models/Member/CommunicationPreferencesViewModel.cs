using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class CommunicationPreferencesViewModel : BaseViewModel
    {
        public string UserName { get; set; }
        public CommunicationPreferencesModel CommunicationPreferences { get; set; }
        public string ReturnUrl { get; set; }
        public AlertViewModel CompletionAlert { get; set; }
        public bool HasCompletionAlert => CompletionAlert != null;
    }
}
