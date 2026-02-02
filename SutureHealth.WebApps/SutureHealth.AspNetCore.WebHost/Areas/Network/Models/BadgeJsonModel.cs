namespace SutureHealth.AspNetCore.Areas.Network.Models
{
    public class BadgeJsonModel
    {
        public long RecentlyJoinedCount { get; set; }
        public string NetworkUrl { get; set; }
        public bool CallToActionEnabled { get; set; }
        public bool NetworkNavigationEnabled { get; set; }
    }
}
