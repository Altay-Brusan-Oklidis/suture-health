CREATE VIEW [dbo].[RequestOutcome]
AS 
SELECT nr.TransmittedRequestId																						RequestId,
	   t_uf.Id																										SignerId,
	   --t_t.CreateDate AT TIME ZONE 'Central Standard Time' AT TIME ZONE 'UTC'										TerminalStateReachedAt,
	   p_t.CreateDate AT TIME ZONE 'Central Standard Time' AT TIME ZONE 'UTC'										ProcessedAt,
	   COALESCE(bp_trn.TransitionedAt, p_t.CreateDate AT TIME ZONE 'Central Standard Time') AT TIME ZONE 'UTC'		BeganPatientReviewAt,
	   COALESCE(fp_trn.TransitionedAt, p_t.CreateDate AT TIME ZONE 'Central Standard Time') AT TIME ZONE 'UTC'		FinishedPatientReviewAt,
	   COALESCE(bd_trn.TransitionedAt, p_t.CreateDate AT TIME ZONE 'Central Standard Time') AT TIME ZONE 'UTC'		BeganDocumentProcessingAt,
	   COALESCE(fd_trn.TransitionedAt, p_t.CreateDate AT TIME ZONE 'Central Standard Time') AT TIME ZONE 'UTC'		FinishedDocumentProcessingAt,
	   CAST(CASE COALESCE(ta.AnnotationsCount, 0) WHEN 0 THEN 0 ELSE 1 END AS BIT)									HasAnnotations,
	   CAST(
			CASE
				WHEN nr.[Status] BETWEEN 9 AND 12 AND EXISTS(SELECT TOP 1 1  
															   FROM dbo.RequestSigner rs 
															  WHERE nr.TransmittedRequestId = rs.RequestId
															    AND lr.Signer <> rs.SutureSignUserFacilityId) THEN 1
				ELSE 0
		    END
	   AS BIT)																										HasSignerChanged,
	   o.DataS3Key																									SignedDocumentStorageKey
  FROM dbo.TransmittedRequest nr
  LEFT JOIN [$(SutureSignWeb)].dbo.Requests lr ON nr.SutureSignRequestID = lr.Id
  LEFT JOIN (SELECT TemplateId, COUNT(*) AnnotationsCount FROM [$(SutureSignWeb)].dbo.TemplateCoordinates WHERE Placeholder = 'Textarea' GROUP BY TemplateId) ta ON lr.Template = ta.TemplateId
  LEFT JOIN [$(SutureSignWeb)].dbo.Outcomes o ON nr.SutureSignRequestID = o.FormId
  LEFT JOIN [$(SutureSignWeb)].dbo.Tasks t_t ON nr.SutureSignRequestID = t_t.FormId AND 1 = t_t.[Active] AND t_t.ActionId IN (528, 529, 556)
  LEFT JOIN [$(SutureSignWeb)].dbo.Users_Facilities t_uf ON t_t.SubmittedBy = t_uf.UserId AND t_t.SubmittedByFacility = t_uf.FacilityId
  LEFT JOIN [$(SutureSignWeb)].dbo.Tasks p_t ON nr.SutureSignRequestID = p_t.TaskId AND 1 = p_t.[Active]
  LEFT JOIN (SELECT TransmittedRequestId, MAX(TransitionedAt) TransitionedAt FROM dbo.TransmittedRequestStatusTransition WHERE [Status] = 5 GROUP BY [TransmittedRequestId], [Status]) bp_trn ON nr.TransmittedRequestId = bp_trn.TransmittedRequestId
  LEFT JOIN (SELECT TransmittedRequestId, MAX(TransitionedAt) TransitionedAt FROM dbo.TransmittedRequestStatusTransition WHERE [PreviousStatus] = 5 GROUP BY [TransmittedRequestId], [PreviousStatus]) fp_trn ON nr.TransmittedRequestId = fp_trn.TransmittedRequestId
  LEFT JOIN (SELECT TransmittedRequestId, MAX(TransitionedAt) TransitionedAt FROM dbo.TransmittedRequestStatusTransition WHERE [Status] = 6 GROUP BY [TransmittedRequestId], [Status]) bd_trn ON nr.TransmittedRequestId = bd_trn.TransmittedRequestId
  LEFT JOIN (SELECT TransmittedRequestId, MAX(TransitionedAt) TransitionedAt FROM dbo.TransmittedRequestStatusTransition WHERE [PreviousStatus] = 6 GROUP BY [TransmittedRequestId], [PreviousStatus]) fd_trn ON nr.TransmittedRequestId = fd_trn.TransmittedRequestId