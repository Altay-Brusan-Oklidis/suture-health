namespace SutureHealth.AspNetCore.Areas.Request.Models.Send
{
    public class PreSubmitVerificationPostModel : IDuplicateRequestFields
    {
        public int? SignerMemberId { get; set; }
        public int? PatientId { get; set; }
        public int? TemplateId { get; set; }
        public DateTime? ClinicalDate { get; set; }
    }
}
