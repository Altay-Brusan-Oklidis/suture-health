using SutureHealth.Providers;

namespace SutureHealth.AspNetCore.Areas.Network.Models.Invitation
{
    public interface IInvitationQuestions
    {
        CommitmentLevel UsageRequirement { get; set; }
        TimeExpectation ReceiveDocumentTime { get; set; }
    }
}
