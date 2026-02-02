using MessageSenderAgent.Model.Address;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Insurance
{
    public class InsuranceModel
    {
        public string SetId { get; set; }
        /// <summary>
        /// Medicare or Medicaid Policy (Plan) Number
        /// </summary>
        [Required]
        public string? InsurancePlanID { get; set; }
        /// <summary>
        /// Payer name
        /// </summary>
        public string? InsuranceCompanyID { get; set; }
        public InsuranceCompanyNameType? InsuranceCompanyName { get; set; }
        /// <summary>
        /// This field contains the name of the person who should be
        /// contacted at the insurance company.
        /// 
        /// Second and Further Given Names or Initials Thereof – Contact Name
        /// </summary>
        public ContantcPersonType? InsuranceCompanyContactPerson { get; set; }
        /// <summary>
        /// Insurance Company Primary Number, formatted as nnnnnnnnnn
        /// </summary>
        public string? InsuranceCompanyPhoneNumber { get; set; }
        /// <summary>
        /// This field contains the name of the insured person.
        /// Last Name ^ First Name
        /// FamilyName(Surname) – Last Name
        /// Given Name – First Name
        /// </summary>
        public InsuredNameType? NameOfInsured { get; set; }
        /// <summary>
        /// Description of insured’s relationship to the patient.
        /// </summary>
        public string? InsuredsRelationshipToPatient { get; set; }
        public string? InsuredsDateOfBirth { get; set; }
        public AddressType? InsuredsAddress { get; set; }
        public string? PolicyNumber { get; set; }
      
    }
}
