CREATE VIEW [dbo].[HistoricalRequest]
AS
SELECT leg.Id													AS [SutureSignRequestID],
	   COALESCE(CASE
			WHEN leg.st IS NULL THEN DATEDIFF(DAY, leg.[TimeStamp], GETUTCDATE())
			ELSE DATEDIFF(DAY, leg.[TimeStamp], leg.StDate)
	   END, 0)													AS [Age],
	   leg.ICD9CodeId											AS [DiagnosisCode],
	   ISNULL(leg.EffDate, leg.StartOfCare)						AS [EffectiveDate],
	   CASE
			WHEN MdfyUF.Id IS NULL THEN NULL					-- Don't report a modified date if we can't tie it to a user
			ELSE leg.LastModifiedDate
	   END														AS [LastModifiedDate],
	   leg.[Status]												AS [RequestStatus],
	   leg.StDate												AS [RequestStatusDate],
	   leg.[StartOfCare],
	   leg.Template												AS TemplateID,
	   T.FacilityDisplayName									AS [TemplateType],
	   COALESCE(TP.TemplateDisplayName, T.FacilityDisplayName)	AS [TemplateDisplayName],
	   leg.[Timestamp]											AS [SubmittedAt],
	   MdfyUF.UserId											AS [ModifierMemberId],
	   leg.Patient												AS [PatientId],
	   CT.UserId 												AS [CollaboratorMemberId],
	   ST.[Data]												AS [RejectionReason],
	   ST.SubmittedBy											AS [RejectionMemberId],		--Rejecting User
	   ST.CreateDate											AS [RejectionDate],
	   signUF.UserId											AS [SignerMemberId],
	   signUF.FacilityId										AS [SignerOrganizationId],
	   CASE 
	   		WHEN leg.SignFile IS NOT NULL THEN CAST(1 AS BIT)
	   		ELSE CAST(0 AS BIT)
	   END														AS [SignerHasFiled],		
	   subUF.UserId												AS [SubmitterMemberId],
	   subUF.FacilityId											AS [SubmitterOrganizationId],
	   leg.SurrogateSubmitterOrgId,
	   CASE 
			WHEN leg.SubmArchive IS NULL THEN CAST(0 AS BIT)
			ELSE CAST(1 AS BIT)
	   END														AS [SubmitterHasArchived],
	   CASE 
			WHEN leg.SubmFile IS NOT NULL THEN CAST(1 AS BIT)
			ELSE CAST(0 AS BIT)
	   END														AS [SubmitterHasFiled],
	   o.DataS3Key												AS [OutcomeStorageKey],

	   (
		SELECT TOP 1 FacilityMRN
		FROM [$(SutureSignWeb)].dbo.Facilities_Patients
		WHERE PatientId = leg.Patient AND FacilityId = subUF.FacilityId AND [Active] = 1
		ORDER BY ChangeDate DESC
	   )														AS [MedicalRecordNumber],
	   NULL														AS [TransmittedRequestId],								
	   NULL														AS [UniqueRequestId],
	   NULL														AS ExternalRequestId,
	   NULL														AS [ApiVersion],
	   NULL														AS [ApiRequest],
	   CAST(0 AS TINYINT)										AS [WorkflowStatus],
	   CASE
			WHEN leg.[At] IS NOT NULL THEN CAST(1 AS BIT)
			ELSE CAST(0 AS BIT)
	   END														AS [IsApproved],
	   leg.FaxStatus,
	   leg.ManualRetry
   FROM [$(SutureSignWeb)].[dbo].Requests leg
  INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities subUF ON leg.Submitter = subUF.Id AND leg.Disabled = 0
  INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signUF ON leg.Signer = signUF.Id
   LEFT JOIN [$(SutureSignWeb)].dbo.Users_Facilities MdfyUF ON leg.LastModifiedBy = MdfyUF.Id
  INNER JOIN [$(SutureSignWeb)].dbo.Templates T ON T.TemplateId = leg.Template
   LEFT JOIN [$(SutureSignWeb)].dbo.TemplateProperties TP ON TP.TemplatePropertyId = T.TemplatePropertyId
   LEFT JOIN [$(SutureSignWeb)].dbo.Tasks CT ON leg.Ct = CT.TaskId
   LEFT JOIN [$(SutureSignWeb)].dbo.Tasks ST ON leg.[Status] = 2 AND leg.St = ST.TaskId AND ST.ActionId = 529
   LEFT JOIN [$(SutureSignWeb)].dbo.Outcomes o ON leg.Id = o.FormId