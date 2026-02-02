using SutureHealth.Hchb.Services.Testing.Utility;
using NHapi.Base.Model;
using SutureHealth.PatientAPI.Services.Testing.Builder;
using SutureHealth.Hchb.Services.Testing.Model.Event;
using SutureHealth.Hchb.Services.Testing.Model.Header;
using SutureHealth.Hchb.Services.Testing.Model.Insurance;
using SutureHealth.Hchb.Services.Testing.Model.Visit;
using SutureHealth.Hchb.Services.Testing.Model.Patient;
using System.Text;
// seems there is a problem with nHAP V25.
// ADT_A04 and ADT_A11 do not exist in that version
using NHapi.Model.V25.Message;
using NHapi.Model.V25.Segment;
using NHapiTools.Model.V25.Segment;
using SutureHealth.Hchb.Services.Testing.Builder;
using SutureHealth.Patients;
using SutureHealth.Hchb.Services.Testing.Model.Address;

namespace SutureHealth.Hchb.Services.Testing.MessageBuilder
{
    public class ADTMessageBuilder
    {
        private AbstractMessage? _ADT;
        private HeaderModel? _Header;
        private PatientModel? _PatientModel;
        private EventModel? _EventModel;
        private VisitModel? _VisitModel;
        private InsuranceModel? _InsuranceModel;

        public HeaderModel? MSH { get { return _Header; } }
        public PatientModel? PID { get { return _PatientModel; } }
        public EventModel? EVN { get { return _EventModel; } }
        public VisitModel? PV1 { get { return _VisitModel; } }
        public InsuranceModel? IN1 { get { return _InsuranceModel; } }
        public string MedicalRecordNumber { get { return _medicalRecordNumber; } }
        public IMessage? Message { get { return _ADT; } }


        string setId = Utilities.GetSequenceNumber();

        string _medicalRecordNumber = string.Empty;

        public IMessage Build(TriggerEvent triggerEvent = TriggerEvent.A01, string medicalRecordNumber = "")
        {
            if (_medicalRecordNumber.IsNullOrEmpty())
                _medicalRecordNumber = Utilities.GetRandomString(8);
            else
                _medicalRecordNumber = medicalRecordNumber;

            HCHBMessageType messageType;
            EventCodeType eventCode;
            switch (triggerEvent)
            {
                case TriggerEvent.A01:
                    _ADT = new ADT_A01();
                    eventCode = EventCodeType.A01;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A01,
                        Trigger = TriggerEvent.A01
                    };
                    break;
                case TriggerEvent.A03:
                    _ADT = new ADT_A03();
                    eventCode = EventCodeType.A03;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A03,
                        Trigger = TriggerEvent.A03
                    };
                    break;
                case TriggerEvent.A04:
                    _ADT = new ADT_A01();
                    eventCode = EventCodeType.A04;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A01,
                        Trigger = TriggerEvent.A04
                    };
                    break;
                case TriggerEvent.A08:
                    _ADT = new ADT_A01();
                    eventCode = EventCodeType.A08;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A01,
                        Trigger = TriggerEvent.A08
                    };
                    break;
                case TriggerEvent.A11:
                    _ADT = new ADT_A09();
                    eventCode = EventCodeType.A11;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A09,
                        Trigger = TriggerEvent.A09
                    };
                    break;
                default:
                    _ADT = new ADT_A01();
                    eventCode = EventCodeType.A01;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A01,
                        Trigger = TriggerEvent.A01
                    };
                    break;
            }

            MSHSegmentBuilder mshBuilder = new MSHSegmentBuilder(messageType, _medicalRecordNumber, ProcessingIdType.P);
            _Header = mshBuilder.Build();
            FillMshSegment(_ADT, _Header);

            EVNSegmentBuilder evnBuilder = new EVNSegmentBuilder(eventCode, reason: EventReasonType.PatientRequest);
            _EventModel = evnBuilder.Build();
            FillEvnSegment(_ADT, _EventModel);

            PIDSegmentBuilder pidBuilder = new PIDSegmentBuilder(setId);
            _PatientModel = pidBuilder.Build();
            FillPidSegment(_ADT, _PatientModel);

            PV1SegmentBuilder pv1Builder = new PV1SegmentBuilder(setId);
            _VisitModel = pv1Builder.Build();
            FillPV1Segment(_ADT, _VisitModel);

            if (_ADT is ADT_A01 || _ADT is ADT_A03)
            {
                IN1SegmentBuilder in1Builder = new IN1SegmentBuilder(setId);
                _InsuranceModel = in1Builder.Build();
                FillIN1Segment(_ADT, _InsuranceModel);
            }

            return _ADT;
        }

        public void UpdateTrigger(TriggerEvent newTriggerEvent)
        {

            HCHBMessageType messageType;
            EventCodeType eventCode;
            switch (newTriggerEvent)
            {
                case TriggerEvent.A01:
                    eventCode = EventCodeType.A01;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A01,
                        Trigger = TriggerEvent.A01
                    };
                    break;
                case TriggerEvent.A03:
                    eventCode = EventCodeType.A03;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A03,
                        Trigger = TriggerEvent.A03
                    };
                    break;
                case TriggerEvent.A04:
                    eventCode = EventCodeType.A04;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A01,
                        Trigger = TriggerEvent.A04
                    };
                    break;
                case TriggerEvent.A08:
                    eventCode = EventCodeType.A08;
                    messageType = new HCHBMessageType()
                    {
                        Sturcture = MessageStructureType.ADT_A01,
                        Trigger = TriggerEvent.A08
                    };
                    break;
                case TriggerEvent.A11:
                    eventCode = EventCodeType.A11;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A09,
                        Trigger = TriggerEvent.A09
                    };
                    break;
                default:
                    eventCode = EventCodeType.A01;
                    messageType = new HCHBMessageType()
                    {
                        Code = MessageCodeType.ADT,
                        Sturcture = MessageStructureType.ADT_A01,
                        Trigger = TriggerEvent.A01
                    };
                    break;
            }

            MSHSegmentBuilder mshBuilder = new MSHSegmentBuilder(messageType, _medicalRecordNumber, ProcessingIdType.P);
            _Header = mshBuilder.Build();
            FillMshSegment(_ADT, _Header);

            EVNSegmentBuilder evnBuilder = new EVNSegmentBuilder(eventCode, reason: EventReasonType.PatientRequest);
            _EventModel = evnBuilder.Build();
            FillEvnSegment(_ADT, _EventModel);
        }

        public void UpdateADTMessageWithPatientInfo(Patient patient)
        {
            PatientModel patientModel = new PatientModel();
            patientModel.Name.FirstName = patient.FirstName;
            patientModel.Name.FamilyName = patient.LastName;
            patientModel.Name.MiddleInitial = patient.MiddleName;
            if (patient.Gender == Gender.Male)
                patientModel.Sex = GenderType.M;
            else if (patient.Gender == Gender.Female)
                patientModel.Sex = GenderType.F;
            else patientModel.Sex = GenderType.U;
            foreach (var identifier in patient.Identifiers)
            {
                //IdentifierCodeType identifierType = new IdentifierCodeType();
                if (string.Compare(identifier.Type, "ssn", true) == 0)
                {
                    patientModel.SSN = identifier.Value;
                    continue;
                }
                if (string.Compare(identifier.Type, "mbi", true) == 0)
                {
                    patientModel.PatientId.Add(new PatientIdentifierType()
                    {
                        AssigningAuthority = "MBI Center",
                        Id = identifier.Value,
                        IdentifierTypeCode = IdentifierCodeType.MC
                    });
                }
                if (string.Compare(identifier.Type, "medicaid-state", true) == 0)
                {
                    patientModel.PatientId.Add(new PatientIdentifierType()
                    {
                        AssigningAuthority = "MA Center",
                        Id = identifier.Value,
                        IdentifierTypeCode = IdentifierCodeType.MA
                    });

                }
                if (string.Compare(identifier.Type, "ssn4", true) == 0)
                {
                    patientModel.PatientId.Add(new PatientIdentifierType()
                    {
                        AssigningAuthority = " SutureHealth",
                        Id = identifier.Value,
                        IdentifierTypeCode = IdentifierCodeType.SS4
                    });
                }
            }

            patientModel.DateOfBirth = patient.Birthdate.ToString("yyy-MM-dd");
            var patientAddress = patient.Addresses.FirstOrDefault();
            patientModel.Address = new AddressType()
            {
                City = patientAddress?.City,
                ZipCode = patientAddress?.PostalCode,
                StreetAddress = patientAddress?.Line1,
                State = patientAddress?.StateOrProvince
            };


            FillPidSegment(_ADT, patientModel);
        }

        public void UpdateADTMessageDemographics(string? name = null, string? familyName = null)
        {
            if (name != null)
            {
                _PatientModel.Name.FirstName = name;
            }
            if (familyName != null)
            {
                _PatientModel.Name.FamilyName = familyName;
            }

            FillPidSegment(_ADT, _PatientModel);
        }

        void FillMshSegment(AbstractMessage? message, HeaderModel randomModel)
        {
            MSH mshSegment;
            if (message is ADT_A01 adt_a01)
            {
                mshSegment = adt_a01.MSH;
            }
            else if (message is ADT_A03 adt_a03)
            {
                mshSegment = adt_a03.MSH;
            }
            else if (message is ADT_A09 adt_a09)
            {
                mshSegment = adt_a09.MSH;
            }
            else
            {
                throw new ArgumentException("Message Type is not supported");
            }

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
        void FillEvnSegment(in AbstractMessage message, EventModel eventModel)
        {
            EVN evnSegment;
            if (message is ADT_A01 adt_a01)
            {
                evnSegment = adt_a01.EVN;
            }
            else if (message is ADT_A03 adt_a03)
            {
                evnSegment = adt_a03.EVN;
            }
            else if (message is ADT_A09 adt_a09)
            {
                evnSegment = adt_a09.EVN;
            }
            else
            {
                throw new ArgumentException("Message Type is not supported");
            }
            evnSegment.EventTypeCode.Value = eventModel.EventTypeCode.ToString();
            evnSegment.RecordedDateTime.Time.Value = eventModel.RecordedDateTime;
            evnSegment.DateTimePlannedEvent.Time.Value = eventModel.DateTimePlannedEvent;
            evnSegment.EventReasonCode.Value = eventModel.EventReasonCode;
            evnSegment.EventOccurred.Time.Value = eventModel.EventOccurred;
        }
        void FillPidSegment(in AbstractMessage message, PatientModel patientModel)
        {
            PID pidSegment;
            if (message is ADT_A01 adt_a01)
            {
                pidSegment = adt_a01.PID;
            }
            else if (message is ADT_A03 adt_a03)
            {
                pidSegment = adt_a03.PID;
            }
            else if (message is ADT_A09 adt_a09)
            {
                pidSegment = adt_a09.PID;
            }
            else
            {
                throw new ArgumentException("Message Type is not supported");
            }

            pidSegment.SetIDPID.Value = patientModel.SetId;
            pidSegment.PatientID.IDNumber.Value = patientModel.ExternalPatientId;
            pidSegment.AdministrativeSex.Value = Enum.GetName(patientModel.Sex);

            var identifier = pidSegment.AddPatientIdentifierList();
            identifier.IDNumber.Value = patientModel.PatientId[0].Id;
            identifier.AssigningAuthority.NamespaceID.Value = patientModel.PatientId[0].AssigningAuthority;
            identifier.AssigningAuthority.UniversalID.Value = patientModel.PatientId[0].AssigningAuthority;
            identifier.AssigningAuthority.UniversalIDType.Value = "ISO";
            identifier.IdentifierTypeCode.Value = patientModel.PatientId[0].IdentifierTypeCode.ToString();

            identifier = pidSegment.AddPatientIdentifierList();
            if (patientModel.PatientId.Count > 1)
            {
                identifier.IDNumber.Value = patientModel.PatientId[1].Id;
                identifier.AssigningAuthority.NamespaceID.Value = patientModel.PatientId[1].AssigningAuthority;
                identifier.AssigningAuthority.UniversalID.Value = patientModel.PatientId[1].AssigningAuthority;
                identifier.AssigningAuthority.UniversalIDType.Value = "ISO";
                identifier.IdentifierTypeCode.Value = patientModel.PatientId[1].IdentifierTypeCode.ToString();
            }

            //var identifiers = pidSegment.GetPatientIdentifierList();
            var alternativeIdentifiers = pidSegment.GetAlternatePatientIDPID(0);

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
        void FillPV1Segment(in AbstractMessage message, VisitModel visitModel)
        {
            PV1 pv1Segment;
            if (message is ADT_A01 adt_a01)
            {
                pv1Segment = adt_a01.PV1;
            }
            else if (message is ADT_A03 adt_a03)
            {
                pv1Segment = adt_a03.PV1;
            }
            else if (message is ADT_A09 adt_a09)
            {
                pv1Segment = adt_a09.PV1;
            }
            else
            {
                throw new ArgumentException("Message Type is not supported");
            }
            pv1Segment.AssignedPatientLocation.PointOfCare.Value = visitModel.AssignedPatientLocation?.AgencyName;
            pv1Segment.AssignedPatientLocation.Room.Value = visitModel.AssignedPatientLocation?.Room;
            pv1Segment.AssignedPatientLocation.Facility.NamespaceID.Value = visitModel.AssignedPatientLocation?.BranchCode;
            pv1Segment.AssignedPatientLocation.Facility.UniversalID.Value = visitModel.AssignedPatientLocation?.BranchName;
            pv1Segment.AssignedPatientLocation.LocationStatus.Value = visitModel.AssignedPatientLocation?.ServiceLine;
            pv1Segment.AssignedPatientLocation.LocationDescription.Value = visitModel.AssignedPatientLocation?.TeamName;
            pv1Segment.AdmissionType.Value = visitModel.AdmissionType;

            pv1Segment.GetAttendingDoctor(0).GivenName.Value = visitModel.AttendingDoctor?.FirstName;
            pv1Segment.GetAttendingDoctor(0).FamilyName.Surname.Value = visitModel.AttendingDoctor?.LastName;
            pv1Segment.GetAttendingDoctor(0).IDNumber.Value = visitModel.AttendingDoctor?.NPI;

            pv1Segment.GetReferringDoctor(0).GivenName.Value = visitModel.ReferringDoctor?.FirstName;
            pv1Segment.GetReferringDoctor(0).FamilyName.Surname.Value = visitModel.ReferringDoctor?.LastName;
            pv1Segment.GetReferringDoctor(0).IDNumber.Value = visitModel.ReferringDoctor?.NPI;

            pv1Segment.GetConsultingDoctor(0).GivenName.Value = visitModel.ConsultingDoctor?.FirstName;
            pv1Segment.GetConsultingDoctor(0).FamilyName.Surname.Value = visitModel.ConsultingDoctor?.LastName;
            pv1Segment.GetConsultingDoctor(0).IDNumber.Value = visitModel.ConsultingDoctor?.NPI;

            pv1Segment.PatientType.Value = visitModel.PatientType;

            if (visitModel.AccountStatus is not null)
            {
                pv1Segment.PatientType.Value = visitModel.PatientType;
                pv1Segment.AccountStatus.Value = Enum.GetName(visitModel.AccountStatus.Value);
            }

            //pv1Segment.AdmitDateTime = visitModel.AdmitDateTime;           

            pv1Segment.PatientClass.Value = visitModel.PatientClass;
        }
        void FillIN1Segment(in AbstractMessage message, InsuranceModel insuranceModel)
        {

            IN1 in1Segment;
            if (message is ADT_A01 adt_a01)
            {
                in1Segment = adt_a01.AddINSURANCE().IN1;
            }
            else if (message is ADT_A03 adt_a03)
            {
                in1Segment = adt_a03.AddINSURANCE().IN1;
            }
            else
            {
                throw new ArgumentException("Message Type does not support insurance");
            }

            in1Segment.SetIDIN1.Value = insuranceModel.SetId;
            in1Segment.InsurancePlanID.Identifier.Value = insuranceModel.InsurancePlanID;
            var companyID = in1Segment.AddInsuranceCompanyID();
            companyID.IDNumber.Value = insuranceModel.InsuranceCompanyID;

            var companyName = in1Segment.AddInsuranceCompanyName();
            companyName.OrganizationName.Value = insuranceModel.InsuranceCompanyName?.PayerName;
            companyName.OrganizationNameTypeCode.Value = insuranceModel.InsuranceCompanyName?.PayerType.ToString();

            var companyAddress = in1Segment.AddInsuranceCompanyAddress();
            companyAddress.City.Value = insuranceModel.InsuredsAddress.City;
            companyAddress.Country.Value = "USA";
            companyAddress.ZipOrPostalCode.Value = insuranceModel.InsuredsAddress.ZipCode;

            var insured = in1Segment.AddNameOfInsured();
            insured.GivenName.Value = insuranceModel.NameOfInsured?.FirstName;
            insured.FamilyName.Surname.Value = insuranceModel.NameOfInsured?.LastName;
            insured.SecondAndFurtherGivenNamesOrInitialsThereof.Value = insuranceModel.NameOfInsured?.Initials;

            in1Segment.InsuredSDateOfBirth.Time.Value = insuranceModel.InsuredsDateOfBirth;
            var address = in1Segment.AddInsuredSAddress();
            address.StreetAddress.StreetOrMailingAddress.Value = insuranceModel.InsuredsAddress.StreetAddress;
            address.ZipOrPostalCode.Value = insuranceModel.InsuredsAddress?.ZipCode;
            address.City.Value = insuranceModel.InsuredsAddress?.City;
            address.StateOrProvince.Value = insuranceModel.InsuredsAddress?.State;

            in1Segment.PolicyNumber.Value = insuranceModel.PolicyNumber;





        }

    }
}
