using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SutureHealth.AspNetCore.Models;

namespace SutureHealth.AspNetCore.Areas.Identity.Models.Organization
{
    public class ProfileModel : BaseViewModel
    {
        [Required(ErrorMessage = "REQUIRED")]
        [DataType(DataType.Text)]
        public string ExternalName { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [DataType(DataType.Text)]
        public string InternalName { get; set; }

        [DataType(DataType.Text)]
        [Remote("OrganizationIsMedicareNumberAvailable", AdditionalFields = "OrganizationId", HttpMethod = "POST", ErrorMessage = "MEDICARE IS TAKEN")]
        [RegularExpression(@"^([0-9a-zA-Z]\d[0-9a-zA-Z]{2}\d[0-9a-zA-Z]|([0-9a-zA-Z]\d[0-9a-zA-Z]\d{7}))$", ErrorMessage = "Incorrect CCN format")]
        public string MedicareCertificationNumber { get; set; }

        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "INVALID")]
        [Remote("OrganizationIsNpiAvailable", AdditionalFields = "OrganizationId", HttpMethod = "POST", ErrorMessage = "NPI IS TAKEN")]
        [Required(ErrorMessage = "REQUIRED")]
        [DataType(DataType.Text)]
        public string Npi { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [RegularExpression(@"^[a-zA-Z0-9\/\s\#\.\-\', '-]*$", ErrorMessage = "INVALID")]
        [DataType(DataType.Text)]
        public string Address1 { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9\/\s\#\.\-\', '-]*$", ErrorMessage = "INVALID")]
        [DataType(DataType.Text)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Address2 { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [RegularExpression(@"^[A-z '-]+$", ErrorMessage = "INVALID")]
        [DataType(DataType.Text)]
        public string City { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        public string State { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [RegularExpression(@"^[0-9]{5}$", ErrorMessage = "INVALID")]
        [DataType(DataType.PostalCode)]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [Phone]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(\+?1[\s.-]*)?\(?[2-9]\d{2}\)?[\s.-]*[2-9]\d{2}[\s.-]*\d{4}$", ErrorMessage = "Invalid Phone Number!")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        [RegularExpression(@"^(\+?1[\s.-]*)?\(?[2-9]\d{2}\)?[\s.-]*[2-9]\d{2}[\s.-]*\d{4}$", ErrorMessage = "Invalid Fax Number!")]
        [DataType(DataType.PhoneNumber)]
        public string Fax { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CloseDate { get; set; }

        public OrganizationListItem SelectedParentOrganization { get; set; }
        public int? ParentOrganizationId { get; set; }
        public int? OrganizationTypeId { get; set; }

        public IEnumerable<SelectListItem> OrganizationTypes { get; set; }
        public int? OrganizationId { get; set; }
        public bool IsEditing { get; set; }
        public bool IsActive { get; set; }
        public AlertViewModel SaveAlert { get; set; }
        public bool HasSaveAlert => SaveAlert != null;

        public class OrganizationListItem
        {
            public int OrganizationId { get; set; }
            public string Name { get; set; }
        }
    }
}
