using SutureHealth.Providers;

namespace SutureHealth.AspNetCore.Areas.Network.Models.Invitation
{
    public abstract class InvitationContent : IInvitationQuestions
    {
        public CommitmentLevel UsageRequirement { get; set; }
        public TimeExpectation ReceiveDocumentTime { get; set; }
        public string FacilityName { get; set; }
        public IEnumerable<string> PhysicianFullNames { get; set; }
        public bool HasPhysicians => this.PhysicianFullNames != null && this.PhysicianFullNames.Any();
        public string SubmitterFullName { get; set; }
        public string TimeFrame => DateTime.UtcNow.AddDays((int)ReceiveDocumentTime).ToString("d");

        public string UsageRequirementText()
        {
            switch (UsageRequirement)
            {
                case CommitmentLevel.Require:
                    return "we now exclusively sign all documents electronically through SutureHealth";
                case CommitmentLevel.Recommended:
                case CommitmentLevel.NotRequired:
                    return "we highly prefer to sign all documents electronically through SutureHealth";
                default:
                    return string.Empty;
            }
        }

        public string UsageRequirementTimeText()
        {
            switch (UsageRequirement)
            {
                case CommitmentLevel.Require:
                    return $"As of {DateTime.UtcNow.AddDays((int)ReceiveDocumentTime):d} we no longer accept documents any other way.";
                case CommitmentLevel.Recommended:
                    return $"As of {DateTime.UtcNow.AddDays((int)ReceiveDocumentTime):d} we expect to receive all documents in SutureHealth.";
                case CommitmentLevel.NotRequired:
                    return $"As of {DateTime.UtcNow.AddDays((int)ReceiveDocumentTime):d} we prefer to receive all documents in SutureHealth.";
                default:
                    return string.Empty;
            }
        }
    }
}
