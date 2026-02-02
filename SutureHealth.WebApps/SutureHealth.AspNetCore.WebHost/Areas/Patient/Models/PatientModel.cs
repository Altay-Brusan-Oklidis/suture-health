using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ExpressiveAnnotations.Attributes;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Application;
using Microsoft.AspNetCore.Mvc.Rendering;
using SutureHealth.Patients;

namespace SutureHealth.AspNetCore.Areas.Patient.Models
{
    public class PatientModel : BaseViewModel
    {
        public int? PatientId { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string SocialSecurityNumberMask => SocialSecurityNumberType switch
        {
            SocialSecurityNumberStyle.Full => "000-00-0000",
            SocialSecurityNumberStyle.Last4 => "0000",
            _ => string.Empty
        };

        public string PhoneNumberMask
        {
            get { return "(000) 000-0000"; }
        }

        public SimilarPatientModel SimilarPatientDialog { get; set; }
        public bool ShowSimilarPatientDialog => SimilarPatientDialog != null;

        #region ModelState Bound
        [RegularExpression(@"^$|^([A-Za-z,.' -]{1,50}$)", ErrorMessage = "INVALID")]
        [Required(ErrorMessage = "REQUIRED")]
        public string FirstName { get; set; }

        [RegularExpression(@"^$|^([A-Za-z,.' -]{1,50}$)", ErrorMessage = "INVALID")]
        public string MiddleName { get; set; }

        [RegularExpression(@"^$|^([A-Za-z,.' -]{1,50}$)", ErrorMessage = "INVALID")]
        [Required(ErrorMessage = "REQUIRED")]
        public string LastName { get; set; }
        public string Suffix { get; set; }

        [AssertThat("DateOfBirth <= Today() && DateOfBirth >= Date(1900, 1, 1)", ErrorMessage = "INVALID")]
        [Required(ErrorMessage = "REQUIRED")]
        [DataType(DataType.Date)]
        [DateValidation]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "REQUIRED")]
        public Gender? Gender { get; set; }
        [Required(ErrorMessage = "REQUIRED")]
        public string GenderValue { get; set; }

        [RegularExpression(@"^$|^(.{3,50}$)", ErrorMessage = "INVALID")]
        [Remote("OrganizationPatientValidateMrn", ErrorMessage = "DUPLICATE", HttpMethod = "POST", AdditionalFields = "PatientId,OrganizationId")]
        [RequiredIf("HasUnavailablePatientRecordNumber == false", ErrorMessage = "REQUIRED")]
        public string PatientRecordNumber { get; set; }
        public bool HasUnavailablePatientRecordNumber { get; set; }

        [RegularExpression(@"^(?!000)(?!666)(?!9)\d{3}[-](?!00)\d{2}[-](?!0000)\d{4}?$|^(?!0000)([0-9]{4})$", ErrorMessage = "INVALID")]
        [RequiredIf("SocialSecurityNumberType != PatientModel.SocialSecurityNumberStyle.Unavailable", ErrorMessage = "REQUIRED")]
        public string SocialSecurityNumber { get; set; }
        [Required(ErrorMessage = "REQUIRED")]
        public SocialSecurityNumberStyle? SocialSecurityNumberType { get; set; }

        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}$", ErrorMessage = "INVALID")]
        public string HomePhone { get; set; } = null;

        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}$", ErrorMessage = "INVALID")]
        public string WorkPhone { get; set; } = null;
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}$", ErrorMessage = "INVALID")]
        public string MobilePhone { get; set; } = null;
        [RegularExpression(@"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}$", ErrorMessage = "INVALID")]
        public string OtherPhone { get; set; } = null;

        [RegularExpression(@"^[a-zA-Z0-9\/\s\#, '-]*$", ErrorMessage = "INVALID")]
        public string Address1 { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9\/\s\#, '-]*$", ErrorMessage = "INVALID")]
        public string Address2 { get; set; }

        [RegularExpression(@"^[A-z '-]+$", ErrorMessage = "INVALID")]
        public string City { get; set; }
        [Required(ErrorMessage = "REQUIRED")]
        public string State { get; set; }

        [RegularExpression(@"^[0-9]{5}$", ErrorMessage = "INVALID")]
        [Required(ErrorMessage = "REQUIRED")]
        public string ZipCode { get; set; }

        [RequiredIf("HasMedicareAdvantage == false && HasMedicareAdvantage == false && HasMedicare == false && HasMedicaid == false && HasPrivateInsurance == false && HasSelfPay == false && HasUnavailablePayerMix == false", ErrorMessage = "* PAYER MIX IS REQUIRED")]
        public string PayerMixRequired { get; set; }

        public bool HasMedicareAdvantage { get; set; }

        public bool HasMedicare { get; set; }

        public bool HasMedicaid { get; set; }

        public bool HasPrivateInsurance { get; set; }

        public bool HasSelfPay { get; set; }
        public bool HasUnavailablePayerMix { get; set; }

        [RegularExpression(@"^[1-9]{1}[^SLOIBZsloibz|^0-9]{1}[^SLOIBZsloibz]{1}[0-9]{1}-?[^SLOIBZsloibz|^0-9]{1}[^SLOIBZsloibz]{1}[0-9]{1}-?[^SLOIBZsloibz|^0-9]{1}[^SLOIBZsloibz|^0-9]{1}[0-9]{1}[0-9]{1}$", ErrorMessage = "INVALID: See MBI Format link below")]
        [RequiredIf("HasMedicare == true", ErrorMessage = "REQUIRED")]
        public string MedicareMBI { get; set; }

        [RequiredIf("HasMedicaid == true", ErrorMessage = "REQUIRED")]
        public string MedicaidState { get; set; }

        [RequiredIf("HasMedicaid == true", ErrorMessage = "REQUIRED")]
        public string MedicaidNumber { get; set; }

        public bool ForceAddSimilarPatient { get; set; }
        #endregion

        public enum SocialSecurityNumberStyle
        {
            Unavailable = 0,
            Full,
            Last4
        }

        public string PrimaryPhone { get; set; } = null;
        public class SimilarPatientModel
        {
            public IEnumerable<SimilarField> Similarities { get; set; }
            public bool AllowAdd { get; set; }

            public enum SimilarField
            {
                [Description("None")]
                None = 0,
                [Description("Last Name")]
                LastName,
                [Description("First Name")]
                FirstName,
                [Description("DOB")]
                Birthdate,
                [Description("Gender")]
                Gender,
                [Description("Zip Code")]
                PostalCode,
                [Description("Social Security Number")]
                SocialSecurityNumber,
                [Description("Medicare MBI")]
                MedicareMBI,
                [Description("Medicaid Number")]
                MedicaidNumber
            }
        }

        public class DateValidationAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                DateTime dateValue;
                bool validdate = DateTime.TryParse(value.ToString(), out dateValue);
                // "var dateValue = (DateTime) value;" might work as well, let me know what does.
                return validdate && (dateValue < DateTime.Now);
            }
        }
        private List<PatientPhone> GetPatientPhones()
        {
            List<PatientPhone> phones = new List<PatientPhone>();
            if (!string.IsNullOrEmpty(HomePhone))
            {
                phones.Add(new Patients.PatientPhone
                {
                    Type = ContactType.HomePhone,
                    IsActive = true,
                    IsPrimary = PrimaryPhone.EqualsIgnoreCase("HomePhone"),
                    Value = HomePhone?.SanitatePhoneNumber()
                });
            }

            if (!string.IsNullOrEmpty(WorkPhone))
            {
                phones.Add(new Patients.PatientPhone
                {
                    Type = ContactType.WorkPhone,
                    IsActive = true,
                    IsPrimary = PrimaryPhone.EqualsIgnoreCase("WorkPhone"),
                    Value = WorkPhone?.SanitatePhoneNumber()
                });
            }

            if (!string.IsNullOrEmpty(MobilePhone))
            {
                phones.Add(new Patients.PatientPhone
                {
                    Type = ContactType.Mobile,
                    IsActive = true,
                    IsPrimary = PrimaryPhone.EqualsIgnoreCase("MobilePhone"),
                    Value = MobilePhone?.SanitatePhoneNumber()
                });
            }

            if (!string.IsNullOrEmpty(OtherPhone))
            {
                phones.Add(new Patients.PatientPhone
                {
                    Type = ContactType.OtherPhone,
                    IsActive = true,
                    IsPrimary = PrimaryPhone.EqualsIgnoreCase("OtherPhone"),
                    Value = OtherPhone?.SanitatePhoneNumber()
                });
            }

            return phones;
        }

        public Patients.Patient ToPatient() =>
            new Patients.Patient
            {
                Identifiers = GetPatientIdentifiers(),
                Birthdate = DateOfBirth.Value,
                FirstName = FirstName,
                MiddleName = MiddleName ?? string.Empty,
                LastName = LastName,
                Suffix = Suffix ?? string.Empty,
                Gender = Gender.GetValueOrDefault(SutureHealth.Gender.Unknown),
                Addresses = new Patients.PatientAddress[]
                {
                    new Patients.PatientAddress
                    {
                        City = City ?? string.Empty,
                        Line1 = Address1 ?? string.Empty,
                        Line2 = Address2 ?? string.Empty,
                        PostalCode = ZipCode,
                        StateOrProvince = State ?? string.Empty,
                    }
                },
                Contacts = Array.Empty<Patients.PatientContact>(),
                Phones = GetPatientPhones()
            };

        public ICollection<Patients.PatientIdentifier> GetPatientIdentifiers()
        {
            var patientIdentifiers = new List<Patients.PatientIdentifier>();

            if (!string.IsNullOrWhiteSpace(SocialSecurityNumber) && SocialSecurityNumberType != SocialSecurityNumberStyle.Unavailable)
            {
                if (SocialSecurityNumberType == SocialSecurityNumberStyle.Full)
                    patientIdentifiers.Add(KnownTypes.SocialSecurityNumber, SocialSecurityNumber);
                patientIdentifiers.Add(KnownTypes.SocialSecuritySerial, SocialSecurityNumber.GetLast(4));
            }
            if (HasMedicare && !string.IsNullOrWhiteSpace(MedicareMBI))
            {
                patientIdentifiers.Add(KnownTypes.Medicare, string.Empty);
                patientIdentifiers.Add(KnownTypes.MedicareBeneficiaryNumber, MedicareMBI.Replace("-", ""));
            }
            if (HasMedicareAdvantage && HasMedicare && !string.IsNullOrWhiteSpace(MedicareMBI)) patientIdentifiers.Add(KnownTypes.MedicareAdvantage, string.Empty);
            if (HasMedicaid && !string.IsNullOrWhiteSpace(MedicaidState) && !string.IsNullOrWhiteSpace(MedicaidNumber))
            {
                patientIdentifiers.Add(KnownTypes.Medicaid, string.Empty);
                patientIdentifiers.Add(KnownTypes.MedicaidState, MedicaidState);
                patientIdentifiers.Add(KnownTypes.MedicaidNumber, MedicaidNumber);
            }
            if (HasPrivateInsurance) patientIdentifiers.Add(KnownTypes.PrivateInsurance, string.Empty);
            if (HasSelfPay) patientIdentifiers.Add(KnownTypes.SelfPay, string.Empty);
            if (!string.IsNullOrWhiteSpace(PatientRecordNumber)) patientIdentifiers.Add(KnownTypes.UniqueExternalIdentifier, PatientRecordNumber);

            return patientIdentifiers;
        }
    }
}
