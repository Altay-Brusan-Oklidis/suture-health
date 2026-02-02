namespace SutureHealth.AspNetCore.Areas.Request.Models.Send
{
    public class PreSubmitVerificationJsonModel
    {
        public DuplicateRiskLevel DuplicateRequestRiskLevel { get; set; }
        public bool PatientUpdateRequested { get; set; }

        public enum DuplicateRiskLevel
        {
            Indeterminate = -1,
            None = 0,
            Warning = 1,
            Error = 2
        }
    }
}
