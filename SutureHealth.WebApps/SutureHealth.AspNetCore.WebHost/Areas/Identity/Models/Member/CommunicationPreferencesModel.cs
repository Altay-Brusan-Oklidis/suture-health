using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using SutureHealth.AspNetCore.Models;
using Channel = SutureHealth.Notifications.Channel;

namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class CommunicationPreferencesModel
    {
        public IEnumerable<SelectListItem> Reports { get; set; }
        public IDictionary<int, IEnumerable<int>> ReportChannels { get; set; }
        public bool RequireAtLeastOneSelection { get; set; }
        public bool ShowInstructions { get; set; }
        public bool IsMobileNumberConfirmed { get; set; }
        public IEnumerable<NotificationType> Channels => new[]
        {
            new NotificationType()
            {
                Name = "Email Notifications",
                ChannelId = (int)Channel.Email,
                SelectionRequired = false
            },
            new NotificationType()
            {
                Name = "Text Notifications",
                ChannelId = (int)Channel.Sms,
                SelectionRequired = false,
                InformationalText = !IsMobileNumberConfirmed ?
                                        "A mobile number is required to enable SMS notifications. Please add and verify your mobile number." :
                                        null
            }
        };
        public IEnumerable<DayOfWeek> Days { get; set; }
        public bool HasMultipleReports => Reports != null && Reports.Count() > 1;
        public UnsubscribeModel Unsubscribe { get; set; }
        public bool HasUnsubscribe => Unsubscribe != null;

        public CommunicationPreferencesSelection[] Selections { get; set; }

        public class NotificationType
        {
            public int ChannelId { get; set; }
            public string Name { get; set; }
            public bool SelectionRequired { get; set; }
            public string InformationalText { get; set; }
            public bool HasInformationalText => !string.IsNullOrWhiteSpace(InformationalText);
        }

        public class UnsubscribeModel
        {
            public bool IsUnsubscribed { get; set; }
            public UnsubscribeReason Reason { get; set; }

            public enum UnsubscribeReason
            {
                [Description("User Request")]
                UserRequest = 0,
                [Description("Undeliverable")]
                Undeliverable
            }
        }
    }

    public class CommunicationPreferencesSelection
    {
        public int ReportTypeId { get; set; }
        public int ChannelId { get; set; }
        public DayOfWeek Day { get; set; }
    }
}
