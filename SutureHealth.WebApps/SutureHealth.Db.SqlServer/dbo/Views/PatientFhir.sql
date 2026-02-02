CREATE VIEW [dbo].[PatientFhir]
AS 
	SELECT Id 					[Id],
		   PatientId			[PatientId],
		   FacilityId		    [OrganizationId],
		   PatientFhirId        [PatientFhirId],
		   FhirInstanceId		[FhirInstanceId],
		   CreateDate			[CreateDate]
	FROM [$(SutureSignWeb)].dbo.[PatientFhir]