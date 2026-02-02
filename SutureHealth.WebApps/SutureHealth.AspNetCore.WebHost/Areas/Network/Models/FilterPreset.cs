using System.ComponentModel;

namespace SutureHealth.AspNetCore.Areas.Network.Models
{
    public enum FilterPreset
    {
        [Description("Near Me")]
        NearMe,
        [Description("My Network")]
        MyNetwork,
        [Description("Claims with Me")]
        ClaimsWithMe,
        [Description("Invitations")]
        Invitations,
        [Description("Recently Joined")]
        RecentlyJoined,
        [Description("Invite Senders")]
        InviteSenders
    }
}
