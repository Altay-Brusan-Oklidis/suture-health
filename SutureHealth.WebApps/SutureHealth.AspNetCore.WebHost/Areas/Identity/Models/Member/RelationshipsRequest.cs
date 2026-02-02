namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class RelationshipsRequest
    {
        public IEnumerable<int> Organizations { get; set; }
        public int MemberTypeId { get; set; }
    }
}
