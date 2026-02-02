using NHapi.Model.V25.Datatype;
using NHapi.Model.V25.Message;
using NHapiTools.Model.V25.Group;
using NHapiTools.Model.V25.Segment;
using SutureHealth.Hchb.Services.Testing.Builder;
using SutureHealth.Hchb.Services.Testing.Model.Address;
using SutureHealth.Hchb.Services.Testing.Model.Comment;
using SutureHealth.Hchb.Services.Testing.Model.Event;
using SutureHealth.Hchb.Services.Testing.Model.Header;
using SutureHealth.Hchb.Services.Testing.Model.Observation;
using SutureHealth.Hchb.Services.Testing.Model.Patient;
using SutureHealth.Hchb.Services.Testing.Model.Visit;
using SutureHealth.Hchb.Services.Testing.Utility;
using SutureHealth.PatientAPI.Services.Testing.Builder;
using SutureHealth.PatientAPI.Services.Testing.Model.Notification;

namespace SutureHealth.Hchb.Services.Testing.MessageBuilder
{
    public class MDMT02MessageBuilder
    {
        private MDM_T02? _MDM_T02;
        private HeaderModel? _Header;
        private PatientModel? _PatientModel;
        private EventModel? _EventModel;
        private VisitModel? _VisitModel;
        private CommentModel? _CommentModel;
        private NotificationModel? _NotificationModel;
        private ObservationModel? _ObservationModel;


        private readonly Base64Encoder _ourBase64Helper = new Base64Encoder();
        private readonly string _pdfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test Files", "sample_pathology_report.pdf");

        public MDM_T02 Build()
        {
            var currentDateTimeString = Utilities.GetRandomDateTime().UptoSecondsString();
            _MDM_T02 = new MDM_T02();

            string setId = Utilities.GetSequenceNumber();
            string medicalRecordNumber = Utilities.GetRandomString(8);

            HCHBMessageType messageType = new() { Code = MessageCodeType.MDM, Sturcture = MessageStructureType.MDM_T02, Trigger = TriggerEvent.T02 };

            MSHSegmentBuilder mshBuilder = new MSHSegmentBuilder(messageType, medicalRecordNumber, ProcessingIdType.P);
            _Header = mshBuilder.Build();
            FillMshSegment(_MDM_T02, _Header);

            EVNSegmentBuilder evnBuilder = new EVNSegmentBuilder(EventCodeType.T02);
            _EventModel = evnBuilder.Build();
            FillEvnSegment(_MDM_T02, _EventModel);

            PIDSegmentBuilder pidBuilder = new PIDSegmentBuilder(setId);
            _PatientModel = pidBuilder.Build();
            FillPidSegment(_MDM_T02, _PatientModel);


            PV1SegmentBuilder pV1Builder = new PV1SegmentBuilder(setId);
            _VisitModel = pV1Builder.Build();
            FillPV1Segment(_MDM_T02, _VisitModel);

            TXASegmentBuilder txaBuilder = new TXASegmentBuilder(ReferencedDataType.AP, DocumentType.PDF, setId);
            _NotificationModel = txaBuilder.Build();
            FillTxaSegment(_MDM_T02, _NotificationModel);

            NTESegmentBuilder nteBuilder = new NTESegmentBuilder(setId);
            _CommentModel = nteBuilder.Build();

            var observationIdentifier = new ObservationIdentifier();
            observationIdentifier.CodingSystem = NameofCodingSystem.HHC;
            observationIdentifier.OrderTypeCode = Utilities.GetRandomAlphabeticString(4);
            observationIdentifier.OrderTypeId = Utilities.GetRandomAlphabeticString(4);

            OBXBuilder obxBuilder = new OBXBuilder(setId, _pdfFilePath, ObservationValueType.ED, observationIdentifier);
            _ObservationModel = obxBuilder.Build();

            FillObxAndNteSegments(in _MDM_T02, _CommentModel, _ObservationModel);

            /*
            FillMshSegment(currentDateTimeString);
            FillEvnSegment(currentDateTimeString);
            FillPidSegment(currentDateTimeString);
            FillPV1Segment();
            FillTxaSegment(currentDateTimeString);
            FillObxAndNteSegments();
            */
            return _MDM_T02;
        }


        void FillMshSegment(in MDM_T02 mdm_T02, HeaderModel randomModel)
        {
            var mshSegment = mdm_T02.MSH;
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
        [Obsolete]
        void CreateMshSegment(string currentDateTimeString)
        {
            var mshSegment = _MDM_T02.MSH;
            mshSegment.FieldSeparator.Value = "|";
            mshSegment.EncodingCharacters.Value = "^~\\&";
            mshSegment.SendingApplication.NamespaceID.Value = "Our System";
            mshSegment.SendingFacility.NamespaceID.Value = "Our Facility";
            mshSegment.ReceivingApplication.NamespaceID.Value = "Their Remote System";
            mshSegment.ReceivingFacility.NamespaceID.Value = string.Empty;//"Their Remote Facility";
            mshSegment.DateTimeOfMessage.Time.Value = currentDateTimeString;
            mshSegment.MessageType.MessageStructure.Value = "MDM_T02";
            mshSegment.MessageControlID.Value = currentDateTimeString + "1234";//medical record number
            mshSegment.ProcessingID.ProcessingID.Value = "P";
            mshSegment.VersionID.VersionID.Value = "2.5";
            //mshSegment.EncodingCharacters.Value = "ASCII";
        }

        void FillEvnSegment(in MDM_T02 mdm_T02, EventModel eventModel)
        {
            var evnSegment = mdm_T02.EVN;
            evnSegment.EventTypeCode.Value = eventModel.EventTypeCode.ToString();
            evnSegment.RecordedDateTime.Time.Value = eventModel.RecordedDateTime;
            evnSegment.DateTimePlannedEvent.Time.Value = eventModel.DateTimePlannedEvent;
            evnSegment.EventReasonCode.Value = eventModel.EventReasonCode;
            evnSegment.EventOccurred.Time.Value = eventModel.EventOccurred;
        }
        [Obsolete]
        void CreateEvnSegment(string currentDateTimeString)
        {
            var evnSegment = _MDM_T02.EVN;
            evnSegment.EventTypeCode.Value = "T02";
            evnSegment.RecordedDateTime.Time.Value = currentDateTimeString;
            evnSegment.DateTimePlannedEvent.Time.Value = currentDateTimeString; //optional
            evnSegment.EventReasonCode.Value = "O";
            evnSegment.EventOccurred.Time.Value = currentDateTimeString;
        }


        void FillPidSegment(in MDM_T02 mdm_T02, PatientModel patientModel)
        {
            var pidSegment = mdm_T02.PID;
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
        [Obsolete]
        void CreatePIDSegment(string currentDateTimeString)
        {
            var pidSegment = _MDM_T02.PID;
            pidSegment.SetIDPID.Value = string.Empty; //HL7 numeric identifying field
            pidSegment.PatientID.IDNumber.Value = string.Empty; //MRN value: optional

            var identifier = pidSegment.AddPatientIdentifierList();
            identifier.IDNumber.Value = "HCHB Patient ID";
            identifier.AssigningAuthority.NamespaceID.Value = "HCHB";
            identifier.AssigningAuthority.UniversalID.Value = "HCHB";
            identifier.AssigningAuthority.UniversalIDType.Value = "ISO";
            identifier.IdentifierTypeCode.Value = "PN";

            identifier = pidSegment.AddPatientIdentifierList();
            identifier.IDNumber.Value = "HCHB Patient ID";
            identifier.AssigningAuthority.NamespaceID.Value = "HCHB";
            identifier.AssigningAuthority.UniversalID.Value = "HCHB";
            identifier.AssigningAuthority.UniversalIDType.Value = "ISO";
            identifier.IdentifierTypeCode.Value = "PI";

            var identifiers = pidSegment.GetPatientIdentifierList();
            var alternativeIdentifiers = pidSegment.GetAlternatePatientIDPID(0);

            alternativeIdentifiers.IDNumber.Value = "ID";
            alternativeIdentifiers.AssigningAuthority.NamespaceID.Value = "HCHB";
            alternativeIdentifiers.AssigningAuthority.UniversalID.Value = "HCHB";
            alternativeIdentifiers.AssigningAuthority.UniversalIDType.Value = "ISO";
            alternativeIdentifiers.IdentifierTypeCode.Value = "PI";

            var patientName = pidSegment.GetPatientName(0);
            patientName.GivenName.Value = "FirstName";
            patientName.FamilyName.Surname.Value = "LastName";
            patientName.SecondAndFurtherGivenNamesOrInitialsThereof.Value = "MiddleName";

            pidSegment.DateTimeOfBirth.Time.Value = currentDateTimeString;

            var address = pidSegment.GetPatientAddress(0);

            address.City.Value = "_city1";
            address.Country.Value = "USA";
            address.StreetAddress.StreetName.Value = "st. 1";
            address.StreetAddress.StreetOrMailingAddress.Value = "str.1";
            address.ZipOrPostalCode.Value = "12345";
            address.StateOrProvince.Value = "AL";
            address.OtherGeographicDesignation.Value = "Other Geo";
            address.CountyParishCode.Value = "Cty1234"; //FacilityID

            pidSegment.SSNNumberPatient.Value = "123-456-7894";
            pidSegment.PatientAccountNumber.IDNumber.Value = "0001";
            pidSegment.PatientDeathIndicator.Value = "Y";
            pidSegment.PatientDeathDateAndTime.Time.Value = currentDateTimeString;

        }


        void FillPV1Segment(in MDM_T02 mdm_T02, VisitModel visitModel)
        {
            var pv1Segment = mdm_T02.PV1;
            pv1Segment.PatientClass.Value = visitModel.PatientClass;
        }
        [Obsolete]
        void CreatePV1Segment()
        {
            var pv1Segment = _MDM_T02.PV1;
            pv1Segment.PatientClass.Value = "O";
        }


        void FillTxaSegment(in MDM_T02 mdm_T02, NotificationModel notificationModel)
        {
            var txaSegment = mdm_T02.TXA;
            txaSegment.DocumentType.Value = notificationModel.DocumentType?.ToString();
            txaSegment.DocumentContentPresentation.Value = notificationModel.DocumentContentPresentation?.ToString();
            txaSegment.ActivityDateTime.Time.Value = notificationModel.ActivityDateTime;
            txaSegment.OriginationDateTime.Time.Value = notificationModel.OriginationDateTime;
            var authenticator = txaSegment.AddAssignedDocumentAuthenticator();
            authenticator.IDNumber.Value = notificationModel.AssignedDocumentAuthenticator?.NPI;
            authenticator.GivenName.Value = notificationModel.AssignedDocumentAuthenticator?.FirstName;
            authenticator.FamilyName.Surname.Value = notificationModel.AssignedDocumentAuthenticator?.LastName;

            //txaSegment.AddTranscriptionistCodeName().AssigningAuthority.NamespaceID.Value = "HCHB Branch Code";
            //txaSegment.FillerOrderNumber.
            txaSegment.DocumentCompletionStatus.Value = notificationModel.DocumentCompletionStatus?.ToString();

            txaSegment.SetIDTXA.Value = notificationModel.SetId;
            txaSegment.DocumentContentPresentation.Value = notificationModel.DocumentContentPresentation;
            txaSegment.ActivityDateTime.Time.Value = notificationModel.ActivityDateTime;

            var pap = txaSegment.AddPrimaryActivityProviderCodeName();
            pap.IDNumber.Value = "NA";


        }
        [Obsolete]
        void CreateTxaSegment(string currentDateTimeString)
        {
            var txaSegment = _MDM_T02.TXA;
            txaSegment.DocumentType.Value = "TS";
            txaSegment.SetIDTXA.Value = "7845";
            txaSegment.DocumentContentPresentation.Value = "AP";
            txaSegment.ActivityDateTime.Time.Value = currentDateTimeString;
            //var pap = txaSegment.AddPrimaryActivityProviderCodeName();

        }

        void FillObxAndNteSegments(in MDM_T02 mdm_T02, CommentModel nteCommentModel, ObservationModel observationModel)
        {

            var observationSegment = mdm_T02.AddOBSERVATION();

            var nteSegment = observationSegment.AddNTE();

            nteSegment.SourceOfComment.Value = nteCommentModel.SourceOfComment.ToString();
            nteSegment.SetIDNTE.Value = nteCommentModel.SetId;
            var nteComment = nteSegment.AddComment();
            nteComment.Value = nteCommentModel.Comment;

            var obxSegment = observationSegment.OBX;
            //obxSegment.ValueType.Value = "ED";
            obxSegment.ValueType.Value = observationModel.ValueType.ToString();

            // TODO: check it            
            obxSegment.ObservationIdentifier.Identifier.Value = observationModel.Identifier?.OrderTypeId;
            obxSegment.ObservationIdentifier.Text.Value = observationModel.Identifier?.OrderTypeCode;
            obxSegment.ObservationIdentifier.NameOfCodingSystem.Value = observationModel.Identifier?.CodingSystem.ToString();

            obxSegment.AddObservationValue();
            /*
            int obx5MaxSize = 90000;
            var base64EncodedStringOfPdfReport = _ourBase64Helper.ConvertToBase64String(new FileInfo(_pdfFilePath));

            var substrings = Enumerable.Range(0, (int)Math.Ceiling((double)base64EncodedStringOfPdfReport.Length / obx5MaxSize))
                            .Select(i => base64EncodedStringOfPdfReport.Substring(i * obx5MaxSize, Math.Min(obx5MaxSize, base64EncodedStringOfPdfReport.Length - i * obx5MaxSize)));


            */
            //var varies = obxSegment.GetObservationValue(substrings.Count()); //obxSegment.AddObservationValue();
            foreach (var str in observationModel.FileLines)
            {
                var varies = obxSegment.AddObservationValue();
                var encapsulatedData = new ED(_MDM_T02, "PDF Report Content");

                encapsulatedData.SourceApplication.NamespaceID.Value = "SutureHealth Application";
                encapsulatedData.TypeOfData.Value = "AP"; //see HL7 table 0191: Type of referenced data
                encapsulatedData.DataSubtype.Value = "PDF";
                encapsulatedData.Encoding.Value = "Base64";
                encapsulatedData.Data.Value = str;
                varies.Data = encapsulatedData;
            }

            obxSegment.ObservationResultStatus.Value = "X";
        }


    }
}
