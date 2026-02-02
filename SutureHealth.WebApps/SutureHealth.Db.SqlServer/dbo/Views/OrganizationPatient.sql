CREATE VIEW [dbo].[OrganizationPatient]
AS
	SELECT
		fp.FacilityId	OrganizationId,
		fp.PatientId,
		fp.Active,
		fp.CreateDate,
		fp.ChangeDate,
		fp.ChangeBy
	FROM [$(SutureSignWeb)].dbo.Facilities_Patients fp