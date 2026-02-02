using MessageSenderAgent.Builder;
using MessageSenderAgent.Model.Address;
using MessageSenderAgent.Model.Header;
using MessageSenderAgent.Model.Observation;
using MessageSenderAgent.Model.Patient;
using MessageSenderAgent.Model.Request;
using MessageSenderAgent.Utility;
using NHapi.Base.Model;
using NHapi.Model.V25.Message;
using NHapiTools.Model.V25.Message;
using NHapiTools.Model.V25.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent
{
    public class ORU_R01MessageBuilder
    {
        private ORU_R01 _ORU_R01;
        private HeaderModel? _Header;
        private PatientModel? _PatientModel;
        private ObservationRequestModel? _ObservationRequestModel;
        private string medicalRecordNumber;
        string setId;
        public ORU_R01 Build()
        {
            medicalRecordNumber = Utilities.GetRandomAlphabeticString(8);
            setId = Utilities.GetSequenceNumber();
            _ORU_R01 = new ORU_R01();

            //MessageType type,string medicalRecordNumber, ProcessingIdType? idType = null
            MessageType type = new MessageType();
            type.Code = MessageCodeType.ORU;
            type.Sturcture = MessageStructureType.ORU_R01;
            type.Trigger = TriggerEvent.R01;
            ProcessingIdType processingIdType = ProcessingIdType.P;
           
            MSHSegmentBuilder mshBuilder = new MSHSegmentBuilder(type, medicalRecordNumber,processingIdType);
            _Header = mshBuilder.Build();
            FillMshSegment(_ORU_R01, _Header);

            PIDSegmentBuilder pidBuilder = new PIDSegmentBuilder(setId, new AddressBuilder());
            _PatientModel = pidBuilder.Build();
            FillPidSegment(_ORU_R01, _PatientModel);

            OBRSegmentBuilder obrBuilder = new OBRSegmentBuilder(ResultStatus.P);
            _ObservationRequestModel = obrBuilder.build();
            FillObrSegment(_ORU_R01, _ObservationRequestModel);
            return _ORU_R01;
        }

         
        
        void FillMshSegment(in ORU_R01 oru_R01,HeaderModel randomModel) 
        {
            var mshSegment = oru_R01.MSH;
            mshSegment.FieldSeparator.Value = randomModel.FieldSeparator;
            mshSegment.EncodingCharacters.Value = randomModel.EncodingCharacters;
            mshSegment.SendingApplication.NamespaceID.Value = randomModel.SendingApplication;
            mshSegment.SendingFacility.NamespaceID.Value = randomModel.SendingFacility;
            mshSegment.ReceivingApplication.NamespaceID.Value = randomModel.ReceivingApplication;
            mshSegment.ReceivingFacility.NamespaceID.Value = randomModel.ReceivingFacility;
            mshSegment.DateTimeOfMessage.Time.Value = randomModel.DateTimeOfMessage;

            // all three sub-fileds of MessageType are required; not mentioned on HCHB documentations
            mshSegment.MessageType.MessageStructure.Value = randomModel.MessageType.Sturcture.ToString();
            mshSegment.MessageType.MessageCode.Value = randomModel.MessageType.Code.ToString();
            mshSegment.MessageType.TriggerEvent.Value = randomModel.MessageType.Trigger.ToString();

            mshSegment.MessageControlID.Value = randomModel.MessageControlID;
            mshSegment.ProcessingID.ProcessingID.Value = randomModel.ProcessingID.ToString();
            mshSegment.VersionID.VersionID.Value = randomModel.VersionID;
        }
        void FillPidSegment(in ORU_R01 oru_R01, PatientModel patientModel)
        {
            var pidSegment = oru_R01.AddPATIENT_RESULT().PATIENT.PID;
            pidSegment.SetIDPID.Value = patientModel.SetId;
            pidSegment.PatientID.IDNumber.Value = patientModel.ExternalPatientId;

            var identifier = pidSegment.AddPatientIdentifierList();
            identifier.IDNumber.Value = patientModel.PatientId[0].Id;
            identifier.AssigningAuthority.NamespaceID.Value = patientModel.PatientId[0].AssigningAuthority;
            identifier.AssigningAuthority.UniversalID.Value = patientModel.PatientId[0].AssigningAuthority;
            identifier.AssigningAuthority.UniversalIDType.Value = "ISO";
            identifier.IdentifierTypeCode.Value = patientModel.PatientId[0].IdentifierTypeCode.ToString();

            identifier = pidSegment.AddPatientIdentifierList();
            identifier.IDNumber.Value = patientModel.PatientId[1].Id;
            identifier.AssigningAuthority.NamespaceID.Value = patientModel.PatientId[1].AssigningAuthority;
            identifier.AssigningAuthority.UniversalID.Value = patientModel.PatientId[1].AssigningAuthority;
            identifier.AssigningAuthority.UniversalIDType.Value = "ISO";
            identifier.IdentifierTypeCode.Value = patientModel.PatientId[1].IdentifierTypeCode.ToString(); ;

            //var identifiers = pidSegment.GetPatientIdentifierList();
            var alternativeIdentifiers = pidSegment.AddAlternatePatientIDPID();

            alternativeIdentifiers.IDNumber.Value = patientModel.AlternatePatientId?.Id;
            alternativeIdentifiers.AssigningAuthority.NamespaceID.Value = patientModel.AlternatePatientId?.AssigningAuthority;
            alternativeIdentifiers.AssigningAuthority.UniversalID.Value = patientModel.AlternatePatientId?.AssigningAuthority;
            alternativeIdentifiers.AssigningAuthority.UniversalIDType.Value = "ISO";
            alternativeIdentifiers.IdentifierTypeCode.Value = patientModel.AlternatePatientId?.IdentifierTypeCode.ToString();

            var patientName = pidSegment.GetPatientName(0);
            patientName.GivenName.Value = patientModel.Name.FirstName;
            patientName.FamilyName.Surname.Value = patientModel.Name.FamilyName;
            patientName.SecondAndFurtherGivenNamesOrInitialsThereof.Value = patientModel.Name.MiddleInitial;

            pidSegment.DateTimeOfBirth.Time.Value = patientModel.DateOfBirth;

            var address = pidSegment.GetPatientAddress(0);

            address.City.Value = patientModel.Address?.City;
            //address.Country.Value = patientModel.Address;
            //address.StreetAddress.StreetName.Value = patientModel.Address?.StreetAddress;
            address.StreetAddress.StreetOrMailingAddress.Value = patientModel.Address?.StreetAddress;
            address.ZipOrPostalCode.Value = patientModel.Address?.ZipCode;
            address.StateOrProvince.Value = patientModel.Address?.State;
            address.OtherGeographicDesignation.Value = patientModel.Address?.FacilityName;
            address.CountyParishCode.Value = patientModel.Address?.FacilityId;
            address.AddressType.Value = patientModel.Address?.FacilityType.ToString();

            pidSegment.SSNNumberPatient.Value = patientModel.SSN;
            pidSegment.PatientAccountNumber.IDNumber.Value = patientModel.AccountNumber;
            pidSegment.PatientDeathIndicator.Value = patientModel.DeathIndicator.ToString();
            pidSegment.PatientDeathDateAndTime.Time.Value = patientModel.DeathDateAndTime;

        }

        void FillObrSegment(in ORU_R01 oru_R01, ObservationRequestModel randomModel) 
        {
            var obrSegment = oru_R01.GetPATIENT_RESULT().GetORDER_OBSERVATION().OBR;
            obrSegment.FillerField1.Value = randomModel.FillerField1;
            obrSegment.FillerOrderNumber.EntityIdentifier.Value = randomModel.FillerOrderNumber;
            obrSegment.UniversalServiceIdentifier.Identifier.Value = randomModel.UniversalServiceIdentifier?.Identifier;
            obrSegment.UniversalServiceIdentifier.Text.Value = randomModel.UniversalServiceIdentifier?.Text;
            obrSegment.RequestedDateTime.Time.Value = randomModel.RequestedDateTime?.UptoSecondsString();
            obrSegment.ObservationDateTime.Time.Value = randomModel.ObservationDateTime?.UptoSecondsString();
            obrSegment.ResultStatus.Value = randomModel.ResultStatus?.ToString();
            obrSegment.ResultsRptStatusChngDateTime.Time.Value = randomModel.ResultStatusDate;
            obrSegment.SetIDOBR.Value = setId;

        }
    }
}
