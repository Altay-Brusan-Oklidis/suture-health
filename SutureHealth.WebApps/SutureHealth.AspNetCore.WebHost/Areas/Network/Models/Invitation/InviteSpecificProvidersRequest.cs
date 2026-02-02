using SutureHealth.Providers;

namespace SutureHealth.AspNetCore.Areas.Network.Models.Invitation
{
    public class InviteSpecificProvidersRequest : IInvitationQuestions
    {
        public long[] SelectedProviders { get; set; }
        public CommitmentLevel UsageRequirement { get; set; }
        public TimeExpectation ReceiveDocumentTime { get; set; }
    }
}
