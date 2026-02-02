
using SutureHealth.Hchb.Services.Testing.Model.Patient;
using SutureHealth.Hchb.Services.Testing.Model.Address;
using SutureHealth.Hchb.Services.Testing.Utility;

namespace SutureHealth.Hchb.Services.Testing.Builder
{
    /// <summary>
    ///  In this section Patient Identifier section is created
    /// </summary>    
    public class PIDSegmentBuilder
    {

        
        PatientModel patientModel;
        AddressBuilder addressBuilder;
        string? setId;



        public PIDSegmentBuilder(string SetId)
        {
            patientModel = new PatientModel();
            addressBuilder = new AddressBuilder();
            this.setId = SetId;            
        }

        public PatientModel Build()
        {
            patientModel.SetId = this.setId;
            patientModel.ExternalPatientId = "ER-" + Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(4);

            List<PatientIdentifierType> identifiers = new List<PatientIdentifierType>();

            identifiers.Add(new PatientIdentifierType()
            {
                Id = Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(2),
                AssigningAuthority = "HCHB",
                IdentifierTypeCode = IdentifierCodeType.PN
            });
            identifiers.Add(new PatientIdentifierType()
            {
                Id = Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(2),
                AssigningAuthority = "HCHB",
                IdentifierTypeCode = IdentifierCodeType.PI
            });

            patientModel.PatientId = identifiers;

            var alternateId = new PatientIdentifierType()
            {
                Id = Utilities.GetRandomAlphabeticString(2) + "-" + Utilities.GetRandomDecimalString(2),
                AssigningAuthority = "HCHB",
                IdentifierTypeCode = IdentifierCodeType.PI
            };
            patientModel.AlternatePatientId = alternateId; //alternateId.ToString();

            patientModel.Name.FirstName = Utilities.GetRandomNameOrFamilyName("FirstName");
            patientModel.Name.FamilyName = Utilities.GetRandomNameOrFamilyName("LastName");
            patientModel.Name.MiddleInitial = Utilities.GetRandomAlphabeticString(2);

            //patientModel.FullName = Utilities.GetRandomalphabeticString(22) + "^" + Utilities.GetRandomalphabeticString(22) + "^" + Utilities.GetRandomAlphabeticString(2);

            patientModel.DateOfBirth = Utilities.GetRandomDateTime().UpToDateString();

            patientModel.Sex = Utilities.GetRandomEnumElement<GenderType>();
            patientModel.Race = Utilities.GetRandomProperyValue<RaceType>();//Utilities.GetRandomRace();

            addressBuilder.Build();
            patientModel.Address = new();
            patientModel.Address.City = addressBuilder.GetAddress().City;
            patientModel.Address.State = addressBuilder.GetAddress().State;
            patientModel.Address.StreetAddress = addressBuilder.GetAddress().StreetAddress;
            patientModel.Address.ZipCode = addressBuilder.GetAddress().ZipCode;

            patientModel.Address.FacilityId = addressBuilder.GetAddress().FacilityId;
            patientModel.Address.FacilityName = addressBuilder.GetAddress().FacilityName;
            patientModel.Address.FacilityType = addressBuilder.GetAddress().FacilityType;

            patientModel.HomePhoneNumber = Utilities.GetRandomFormattedPhoneNumber();
            patientModel.BusinessPhoneNumber = Utilities.GetRandomFormattedPhoneNumber();

            patientModel.MaritalStatus = Utilities.GetRandomEnumElement<MaritalStatus>();

            patientModel.AccountNumber = Utilities.GetRandomString(12);
            patientModel.SSN = Utilities.GetRandomSSN();

            string isDead = Utilities.GetRandomDeathIndicator();
            if (isDead == "Y")
            {
                patientModel.DeathIndicator = PatientDeathIndicator.Y;
                patientModel.DeathDateAndTime = Utilities.GetRandomDateTime().UptoSecondsString();
            }
            else
            {
                patientModel.DeathIndicator = PatientDeathIndicator.N;
            }
            return patientModel;
        }

        public Model.Patient.PatientModel PatientModel { get { return patientModel; } }
    }
}
