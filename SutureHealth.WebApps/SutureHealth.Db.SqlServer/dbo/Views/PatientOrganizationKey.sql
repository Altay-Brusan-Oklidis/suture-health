CREATE VIEW [dbo].[PatientOrganizationKey] 
AS
SELECT fp.FacilityId							OrganizationId,
       fp.FacilityMRN							MedicalRecordNumber,
	   fp.PatientId,
	   CAST(COALESCE(fp.Active, 0) AS BIT)		IsActive,
	   fp.CreateDate							CreatedAt,
	   fp.ChangeDate							LastModifiedAt
  FROM [$(SutureSignWeb)].dbo.Facilities_Patients fp
