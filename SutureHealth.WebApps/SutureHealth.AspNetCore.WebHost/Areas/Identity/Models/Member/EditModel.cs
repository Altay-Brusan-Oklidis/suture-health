using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using ExpressiveAnnotations.Attributes;
using SutureHealth.AspNetCore.Models;
using SutureHealth.Application;

namespace SutureHealth.AspNetCore.Areas.Identity.Models.Member
{
    public class EditModel : BaseViewModel, IEditModel
    {
        public bool CanEditBillingAdministrator { get; set; }
        public bool CanEditDefault { get; set; }
        public int? MemberId { get; set; }
        public bool IsCreating => !MemberId.HasValue;
        public bool IsAllEditableOfficesActive => Offices != null && Offices.Where(o => o.CanEdit).All(o => o.Active);
        public bool HasRelationships => Relationships != null && Relationships.Any();
        public bool IsMobileNumberConfirmed { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string CancelUrl { get; set; }
        public IEnumerable<SelectListItem> Occupations { get; set; }
        public bool HasPendingDocuments => Offices != null && Offices.Any(o => o.HasPendingDocuments);

        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        [RegularExpression(RegexValidation.NameValidation, ErrorMessage = ValidationMessages.InvalidFieldMessage)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        [RegularExpression(RegexValidation.NameValidation, ErrorMessage = ValidationMessages.InvalidFieldMessage)]
        public string LastName { get; set; }
        public string Suffix { get; set; }

        [RequiredIf("MemberTypeId == 2000 || MemberTypeId == 2001 || MemberTypeId == 2008", ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        [RegularExpression(RegexValidation.NameValidation, ErrorMessage = ValidationMessages.InvalidFieldMessage)]
        public string ProfessionalSuffix { get; set; }

        [RequiredIf("MemberTypeId == 2000 || MemberTypeId == 2001 || MemberTypeId == 2008", ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        [RegularExpression(RegexValidation.NPIValidate, ErrorMessage = ValidationMessages.InvalidFieldMessage)]
        [Remote("MemberIsNpiAvailable", AdditionalFields = "MemberId", HttpMethod = "POST", ErrorMessage = ValidationMessages.NPITakenMessage)]
        // NOTE (#2441): Server-side validation for this property is removed by ValidateEditModel() so the above attributes will only affect client-side validation.
        public string Npi { get; set; }

        [RequiredIf("MemberTypeId == 2000", ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        public string SigningName { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        [RegularExpression(RegexValidation.UserNameValidate, ErrorMessage = ValidationMessages.UserNameMessageHelp)]
        [Remote("MemberIsUserNameAvailable", AdditionalFields = "MemberId", HttpMethod = "POST", ErrorMessage = ValidationMessages.UserNameTakenMessage)]
        public string UserName { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        [RegularExpression(RegexValidation.EmailValidate, ErrorMessage = ValidationMessages.InvalidFieldMessage)]
        public string Email { get; set; }

        [RegularExpression(RegexValidation.PhoneValidate, ErrorMessage = ValidationMessages.InvalidFieldMessage)]
        public string MobileNumber { get; set; }

        [RegularExpression(RegexValidation.PhoneValidate, ErrorMessage = ValidationMessages.InvalidFieldMessage)]
        public string OfficePhone { get; set; }

        public string OfficePhoneExtension { get; set; }

        [Required(ErrorMessage = ValidationMessages.RequiredFieldMessage)]
        public int? MemberTypeId { get; set; }

        public int? DefaultOrganizationId { get; set; }

        public bool CanSignDocuments { get; set; }

        public bool ShowPatientsInfo { get; set; }
        public bool IsUserLimitReached { get; set; }
        public int MaxAllowedCoummunityUsers { get; set; }

        public Office[] Offices { get; set; }
        IOfficeModel[] IEditModel.Offices => Offices;
        public Relationship[] Relationships { get; set; }
        IRelationshipModel[] IEditModel.Relationships => Relationships;

        public class Office : IOfficeModel
        {
            public int OrganizationId { get; set; }
            public string Name { get; set; }

            public bool Active { get; set; }
            public bool BillingAdministrator { get; set; }
            public bool CanEdit { get; set; }
            public bool HasPendingDocuments { get; set; }
            public bool UserManagement { get; set; }
        }

        public class Relationship : IRelationshipModel
        {
            public int MemberId { get; set; }
            public bool Active { get; set; }
        }

        public class OfficeComparer : IEqualityComparer<Office>
        {
            public bool Equals(Office x, Office y)
                => x.OrganizationId.Equals(y.OrganizationId);

            public int GetHashCode([DisallowNull] Office obj)
                => obj.OrganizationId.GetHashCode();
        }
    }

    public interface IAccountModel
    {
        string FirstName { get; }
        string LastName { get; }
        string Suffix { get; }
        string ProfessionalSuffix { get; }
        string Npi { get; }
        string SigningName { get; }
        string UserName { get; }
        string Email { get; }
        string MobileNumber { get; }
        string OfficePhone { get; }
        string OfficePhoneExtension { get; }
    }

    public interface IEditModel : IAccountModel
    {
        int? MemberTypeId { get; }
        int? DefaultOrganizationId { get; set; }
        IOfficeModel[] Offices { get; }
        IRelationshipModel[] Relationships { get; }
    }

    public interface IOfficeModel
    {
        int OrganizationId { get; }
        bool Active { get; }
        bool UserManagement { get; }
        bool BillingAdministrator { get; }
    }

    public interface IRelationshipModel
    {
        int MemberId { get; }
        bool Active { get; }
    }
}
