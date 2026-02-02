namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class RelationshipsModel
    {
        public bool HasRelationships => Relationships != null && Relationships.Any();
        public bool IsAllEditableRelationshipsActive => Relationships != null && Relationships.All(r => r.Active);
        public Relationship[] Relationships { get; set; }

        public class Relationship : IRelationshipModel
        {
            public int MemberId { get; set; }
            public bool Active { get; set; }
            public string Name { get; set; }
            public string Occupation { get; set; }
            public string Role { get; set; }
        }
    }
}
