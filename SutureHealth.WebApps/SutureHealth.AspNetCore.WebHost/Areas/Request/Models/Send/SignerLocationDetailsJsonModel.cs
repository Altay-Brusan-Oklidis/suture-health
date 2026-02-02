namespace SutureHealth.AspNetCore.Areas.Request.Models.Send
{
    public class SignerLocationDetailsJsonModel
    {
        public int MemberId { get; set; }
        public int OrganizationId { get; set; }
        public IEnumerable<Subordinate> Collaborators { get; set; }
        public IEnumerable<Subordinate> Assistants { get; set; }
        public bool HasCollaborators => Collaborators != null && Collaborators.Any();
        public bool HasAssistants => Assistants != null && Assistants.Any();

        public class Subordinate
        {
            public int MemberId { get; set; }
            public string Summary { get; set; }
        }
    }
}
