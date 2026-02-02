namespace SutureHealth.AspNetCore.Areas.Search.Models.Signer
{
    public class SearchJsonModel<SignerType>
    {
        public IEnumerable<SignerType> Signers { get; set; }
    }

    public class SigningMember
    {
        public int MemberId { get; set; }
        public string Summary { get; set; }
    }

    public class SigningOrganizationMember
    {
        public int OrganizationMemberId { get; set; }
        public int OrganizationId { get; set; }
        public int MemberId { get; set; }
        public string Summary { get; set; }
    }
}
