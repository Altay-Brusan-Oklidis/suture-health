using MessageSenderAgent.Model.Address;
using MessageSenderAgent.Model.Insurance;
using MessageSenderAgent.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Builder
{
    public class IN1SegmentBuilder
    {
        InsuranceModel insuranceModel;
        public IN1SegmentBuilder(string? setId=null) 
        {
            insuranceModel = new InsuranceModel();
            insuranceModel.SetId = setId;
        }

        public InsuranceModel Build() 
        {
            insuranceModel.InsuranceCompanyContactPerson= new ContantcPersonType() { FirstName= Utilities.GetRandomNameOrFamilyName("FirstName") };
            insuranceModel.InsuranceCompanyID= Utilities.GetRandomAlphabeticString(5);
            insuranceModel.InsuranceCompanyName= new InsuranceCompanyNameType() { PayerName= Utilities.GetRandomNameOrFamilyName("FirstName"), PayerType = Utilities.GetRandomEnumElement<OrganizationNameTypeCode>() };
            insuranceModel.InsuranceCompanyPhoneNumber = Utilities.GetRandomDecimalString(10);
            insuranceModel.InsurancePlanID = Utilities.GetRandomDecimalString(10);
            AddressBuilder addressBuilder = new AddressBuilder();
            var address = addressBuilder.Build();
            insuranceModel.InsuredsAddress = new AddressType() 
            { 
                StreetAddress = address.StreetAddress, 
                City = address.City, 
                FacilityId= Utilities.GetRandomAlphabeticString(5), 
                FacilityName= Utilities.GetRandomAlphabeticString(5), 
                FacilityType = Utilities.GetRandomEnumElement<FascilityType>(), 
                State= address.State, 
                ZipCode = address.ZipCode 
            };
            insuranceModel.InsuredsDateOfBirth = Utilities.GetRandomDateTime().UpToDateString();
            insuranceModel.NameOfInsured = new();
            insuranceModel.NameOfInsured.FirstName = Utilities.GetRandomString(5);
            insuranceModel.PolicyNumber = Utilities.GetRandomString(5);            
            return insuranceModel;
        }
    }
}
