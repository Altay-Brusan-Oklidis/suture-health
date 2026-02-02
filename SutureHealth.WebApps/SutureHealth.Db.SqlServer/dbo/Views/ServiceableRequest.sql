CREATE VIEW [dbo].[ServiceableRequest]
AS
SELECT leg.Id													AS [SutureSignRequestID],
	   leg.Patient												AS [PatientId],
	   COALESCE(CASE
			WHEN leg.st IS NULL THEN DATEDIFF(DAY, leg.[TimeStamp], GETUTCDATE())
			ELSE DATEDIFF(DAY, leg.[TimeStamp], leg.StDate)
	   END, 0)													AS [Age],
	   leg.ICD9CodeId											AS [DiagnosisCodeId],
	   ISNULL(leg.EffDate, leg.StartOfCare)						AS [EffectiveDate],
	   CASE
			WHEN MdfyUF.Id IS NULL THEN NULL	-- Don't report a modified date if we can't tie it to a user
			ELSE leg.LastModifiedDate
	   END														AS [LastModifiedDate],
	   leg.[Status]												AS [RequestStatus],
	   CAST(COALESCE(mit.SubmittedAt, leg.[Timestamp], CAST(0 AS DATETIME)) AS DATETIME)										
																AS [RequestSubmittedAt],
	   leg.StDate												AS [RequestStatusDate],
	   leg.[StartOfCare],
	   leg.Template												AS TemplateID,
	   ISNULL(leg.IsComplete, 0)								AS IsComplete,
       CASE
            WHEN leg.ST IS NOT NULL AND (ST.ActionId = 528 OR ST.ActionId = 555 OR ST.ActionId = 556 OR ST.ActionId = 557) THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
       END														AS IsSigned,
	   leg.CollaboratorId										AS [CollaboratorMemberId],
	   leg.AssistantId											AS [AssistantMemberId],
	   mdfyUF.UserId											AS [ModifierMemberId],
	   [ST].[Data]												AS [RejectionReason],
	   [ST].SubmittedBy											AS [RejectionMemberId],		--Rejecting User
	   [ST].CreateDate											AS [RejectionDate],
	   leg.SignerId												AS [SignerMemberId],
	   leg.SignerOrgId											AS [SignerOrganizationId],
	   CASE 
	   		WHEN leg.SignFile IS NOT NULL THEN CAST(1 AS BIT)
	   		ELSE CAST(0 AS BIT)
	   END														AS [SignerHasFiled],		
	   leg.SubmitterId											AS [SubmitterMemberId],
	   leg.SubmitterOrgId										AS [SubmitterOrganizationId],
	   CASE 
			WHEN leg.SubmArchive IS NULL THEN CAST(0 AS BIT)
			ELSE CAST(1 AS BIT)
	   END														AS [SubmitterHasArchived],
	   CASE 
			WHEN leg.SubmFile IS NOT NULL THEN CAST(1 AS BIT)
			ELSE CAST(0 AS BIT)
	   END														AS [SubmitterHasFiled],
	   o.DataS3Key												AS [OutcomeStorageKey],
	   mit.[ApiVersion],
	   mit.[ApiRequest],
	   mit.[TransmittedRequestId],								
	   mit.[UniqueRequestId],
	   mit.[IntegratorId],
	   mit.ExternalRequestId,
	   CASE 
			WHEN mit.[Status] IS NOT NULL THEN mit.[Status]
			ELSE CASE leg.[Status] 
					WHEN 1 THEN CAST(9 AS TINYINT)
					WHEN 2 THEN CAST(10 AS TINYINT)
					WHEN 3 THEN CAST(11 AS TINYINT)
					ELSE CAST(8 AS TINYINT)
				 END
	   END														AS [WorkflowStatus],
	   CASE WHEN COALESCE(leg.[IsResent], 0) = 1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [IsResent],
	   leg.RevenueValue,
	   leg.RelativeValueUnit,
	   ISNULL(leg.FaxStatus, -1)								AS FaxStatus,
	   leg.AutoRetry,
	   leg.ManualRetry
   FROM [$(SutureSignWeb)].[dbo].Requests leg
   LEFT JOIN dbo.TransmittedRequest	mit ON leg.Id = mit.SutureSignRequestID
   LEFT JOIN [$(SutureSignWeb)].dbo.Outcomes o ON leg.Id = o.FormId
   LEFT JOIN [$(SutureSignWeb)].dbo.Tasks [ST] ON leg.St = ST.TaskId AND ST.ActionId = 529 AND leg.[Status] = 2
   LEFT JOIN [$(SutureSignWeb)].dbo.Users_Facilities mdfyUF ON leg.LastModifiedBy = mdfyUF.Id
   where leg.Disabled = 0
