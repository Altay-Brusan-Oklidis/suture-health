namespace SutureHealth.AspNetCore.Areas.Request.Models.Send
{
    public class SignerDetailsJsonModel
    {
        public int MemberId { get; set; }
        public IEnumerable<Location> Locations { get; set; }

        public class Location
        {
            public int OrganizationId { get; set; }
            public bool IsPayingClient { get; set; }
            public string Summary { get; set; }
        }
    }
}
