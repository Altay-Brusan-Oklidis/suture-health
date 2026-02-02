using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Admin.Models.System
{
    public class IndexModel : BaseViewModel
    {
        public SettingsTab Settings { get; set; }
        public DoNotSendTab DoNotSend { get; set; }

        public class SettingsTab
        {
            public SettingsGridModel SettingsGrid { get; set; }
        }

        public class DoNotSendTab
        {
            public IEnumerable<NotificationTypeListItem> NotificationTypes { get; set; }

            public class NotificationTypeListItem
            {
                public int NotificationTypeId { get; set; }
                public string Description { get; set; }
            }

            public class MemberListItem
            {
                public int MemberId { get; set; }
                public string Description { get; set; }
            }
        }
    }
}
