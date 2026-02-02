using System.ComponentModel.DataAnnotations;
using SutureHealth.AspNetCore.Models;
using ExpressiveAnnotations.Attributes;

namespace SutureHealth.AspNetCore.Areas.Request.Models.Send
{
    public class SendModel : BaseViewModel, ISerializedSendModel, IDuplicateRequestFields
    {
        public bool IsSurrogateSender { get; set; }

        public bool FromOfficeExpandSearch { get; set; }
        public int OrganizationId { get; set; }
        public IEnumerable<Office> Offices { get; set; }
        public bool HasMultipleOffices => Offices != null && Offices.Count() > 1;
        public bool HasRequestTemplate { get; set; }
        public bool OverrideClientModel { get; set; }

        #region ModelState Bound
        [Required(ErrorMessage = "Template is required")]
        public int? TemplateId { get; set; }

        [Required(ErrorMessage = "Signer is required")]
        public int? SignerMemberId { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public int? SignerOrganizationId { get; set; }
        public int? CollaboratorMemberId { get; set; }
        public int? AssistantMemberId { get; set; }

        [Required(ErrorMessage = "Patient is required")]
        public int? PatientId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [AssertThat("ClinicalDate >= (Today() - TimeSpan(365, 0, 0, 0))", ErrorMessage = "Date cannot be older than a year")]
        public DateTime? ClinicalDate { get; set; }
        public int? DiagnosisCodeId { get; set; }
        public bool PreviewBeforeSending { get; set; }
        public IFormFile PdfContents { get; set; }

        public bool SignerExpandSearch { get; set; }
        public string FromOfficeAutoComplete { get; set; }
        public string SignerAutoComplete { get; set; }
        public string PatientAutoComplete { get; set; }
        public string PrimaryDxCodeAutoComplete { get; set; }
        #endregion

        public class Office
        {
            public int OrganizationId { get; set; }
            public bool IsPrimary { get; set; }
            public bool IsPayingClient { get; set; }
            public string Name { get; set; }
            public string StateOrProvince { get; set; }
            public bool CanSendRequests { get; set; }
            public bool HasIncompleteProfile { get; set; }
            public int MaxUploadBytes { get; set; }
        }

        public class FromOffice : Office
        {
            public string NPI { get; set; }
            public string City { get; set; }

            public string Summary => $"{Name} ({City}, {StateOrProvince} - NPI: {NPI})";
        }
    }

    public interface ISerializedSendModel
    {
        int OrganizationId { get; }
        int? TemplateId { get; }
        int? SignerMemberId { get; }
        int? SignerOrganizationId { get; }
        int? CollaboratorMemberId { get; }
        int? AssistantMemberId { get; }
        int? PatientId { get; set; }
        DateTime? ClinicalDate { get; }
        int? DiagnosisCodeId { get; }
        bool PreviewBeforeSending { get; }

        public string FromOfficeAutoComplete { get; }

        public bool FromOfficeExpandSearch { get; }

        bool SignerExpandSearch { get; }
        string SignerAutoComplete { get; }
        string PatientAutoComplete { get; }
        string PrimaryDxCodeAutoComplete { get; }
    }

    public interface IDuplicateRequestFields
    {
        int? SignerMemberId { get; }
        int? PatientId { get; }
        int? TemplateId { get; }
        DateTime? ClinicalDate { get; }
    }

    public static class DuplicateRequestFieldsExtensions
    {
        public static bool CanCheckForDuplicateRequests(this IDuplicateRequestFields model)
            => (new bool[] { model.ClinicalDate.HasValue, model.PatientId.HasValue, model.SignerMemberId.HasValue, model.TemplateId.HasValue }).All(hv => hv);
    }
}
