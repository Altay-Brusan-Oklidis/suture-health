CREATE VIEW [dbo].[PatientFhir_MatchErrorLog]
	AS 
	SELECT Id 						[Id],
		   PatientId				[PatientId],
		   FhirPatientId		    [FhirPatientId],
		   FhirInstanceId			[FhirInstanceId],
		   PatientData				[PatientData],
		   Status					[Status],
		   ErrorType				[ErrorType],
		   ErrorCode				[ErrorCode],
		   Severity					[Severity],
		   Description				[Description],
		   CreateDate				[CreateDate],
		   UpdateDate				[UpdateDate]
	FROM [$(SutureSignWeb)].dbo.[PatientFhir_MatchErrorLog]
