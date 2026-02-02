using SutureHealth.Providers;
using SutureHealth.AspNetCore.Areas.Network.Models.Invitation;

namespace SutureHealth.AspNetCore.Areas.Network.Models.Network
{
    public class InvitationModalFooterRequest : IInvitationQuestions
    {
        public TimeExpectation TimeExpectation { get; set; }
        public CommitmentLevel UsageRequirement { get; set; }

        TimeExpectation IInvitationQuestions.ReceiveDocumentTime { get => TimeExpectation; set => TimeExpectation = value; }
    }
}
