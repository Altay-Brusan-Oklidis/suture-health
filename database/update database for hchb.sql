CREATE SCHEMA hchb

CREATE TABLE [hchb].[PatientMetaData]
(
 PatientId int IDENTITY(1000000,1) NOT NULL,
 PatientHomePhone char(15),
 PatientBusinessPhone CHAR(15),
 PatientPrimaryLanguage NCHAR(50),
 PatientMaritalStatus CHAR(15),
 PatientDead BIT,
 PatientDeathDate SMALLDATETIME,

 PatientAlergyDescription char(200) NULL,
 PatientAlergyIdentificationDate  smalldatetime null,
 
 SendingApplication nvarchar(100),
 SendingFacility nvarchar(100),
 TansmisionDate SMALLDATETIME NULL,
 EventOccurredDate SMALLDATETIME NULL,
 EventPlannedDate SMALLDATETIME NULL,
 EventMessagedDate SMALLDATETIME NULL,
)
CREATE TABLE [hchb].[PatientAsscociate]
(
PatientId int IDENTITY(1000000,1) NOT NULL,
 AssociatedPartyFirstName nvarchar(50),
 AssociatedPartyLastName nvarchar(50),
 AssociatedPartyRelation NVARCHAR(50),
 AssociatedPartyStreetAddress nvarchar(150),
 AssociatedPartyCity nvarchar(150),
 AssociatedPartyStateOrProvince varchar(2),
 AssociatedPartyPostalCode varchar(9),
 AssocaitedPartyHomePhone CHAR(15),
 AssocaitedPartyBusinessPhone CHAR(15)
)
CREATE TABLE [hchb].[PatientVisit]
(
 AgencyName nvarchar(25),
 Room char(20) NULL,
 BranchCode nvarchar(20),
 BranchName nvarchar(200),
 ServiceLine nvarchar(20) ,
 ServiceLocation nvarchar(70) NULL,
 TeamName nvarchar(200),
 AdmissionType nvarchar(20),
 AttendingDoctorId nvarchar(15) NULL,
 AttendingDoctorFirstName nvarchar(50) null,
 AttendingDoctorLastName nvarchar(50),
 AttendingDoctorDegree nvarchar(5),
 ReferringDoctorId nvarchar(15) NULL,
 ReferringDoctorFirstName nvarchar(50) null,
 ReferringDoctorLastName nvarchar(50),
 ReferringDoctorDegree nvarchar(5),
 ConsultingDoctorId nvarchar(15) NULL,
 ConsultingDoctorFirstName nvarchar(50) null,
 ConsultingDoctorLastName nvarchar(50),
 ConsultingDoctorDegree nvarchar(5),
 PatientType varchar(15),
 DischargeDisposition char(5),
 AccountStatus char(20),
 AdmitDate smalldatetime,
 DischargeDate smalldatetime
)
