CREATE PROCEDURE [dbo].[SelectRequestTasks]
	@UserId int,
	@FormId int
AS
BEGIN
	SET NOCOUNT ON
	SELECT  T.FormId,
			T.CreateDate as [TaskDate],
			T.ActionId,
			U.FirstName,
			U.LastName,
			U.Suffix,
			U.PrimCredential,
			F.[Name] AS OrganizationName,
			CASE 
				WHEN T.ActionId = 524 THEN 'View Document'
				WHEN T.ActionId = 525 THEN 'Approve Document'
				WHEN T.ActionId = 526 THEN 'Request Assistance'
				WHEN T.ActionId = 527 THEN 'Edit Document'
				WHEN T.ActionId = 528 THEN 'Sign Document'
				WHEN T.ActionId BETWEEN 529 AND 539 THEN 'Reject Document'
				WHEN T.ActionId = 540 THEN 'Open PDF'
				WHEN T.ActionId BETWEEN 541 AND 546 THEN 'Generate PDF'
				WHEN T.ActionID IN (547,548) THEN 'Archive Document'
				WHEN T.ActionId IN (549,550) THEN 'Retract Document'
				WHEN T.ActionId IN (551,552) THEN 'Create Document'
				WHEN T.ActionId IN (553,554) THEN 'Sent for Signature'
				WHEN T.ActionId BETWEEN 555 AND 557 THEN 'Sign Document'
				WHEN T.ActionId BETWEEN 558 AND 565 THEN 'Download Document'
				WHEN T.ActionId = 566 THEN 'Mark Document as Filed'
				WHEN T.ActionId = 575 THEN 'Add NP/PA to Document'
				WHEN T.ActionId = 627 THEN (SELECT 'Document type changed from ' + (SELECT TemplateDisplayName FROM [$(SutureSignWeb)].[dbo].TemplateProperties WHERE TemplatePropertyId = T.OldTemplatePropertyId) + ' to ' + (SELECT TemplateDisplayName FROM [$(SutureSignWeb)].[dbo].TemplateProperties WHERE TemplatePropertyId = T.NewTemplatePropertyId))
				WHEN T.ActionId = 653 THEN 'Change Signer/Office'
				WHEN T.ActionId = 753 THEN 'Resend Document'
				ELSE 'Unknown'
			END as ActionText
	  FROM [$(SutureSignWeb)].[dbo].Tasks T
	 INNER JOIN [$(SutureSignWeb)].[dbo].Users U ON (U.UserId = T.SubmittedBy and T.ActionId not in (541, 555, 557)) or (U.UserId = T.UserId and T.ActionId in (555, 557))
	 INNER JOIN [$(SutureSignWeb)].[dbo].Facilities F ON F.FacilityId = T.SubmittedByFacility
	 INNER JOIN [$(SutureSignWeb)].[dbo].Actions as A ON T.ActionId = A.ActionId
	 WHERE T.FormId = @FormId
	   AND ((T.ActionId in (540, 547, 558, 566) AND T.SubmittedByFacility in (SELECT FacilityId FROM [$(SutureSignWeb)].[dbo].Users_Facilities UF WHERE UF.UserId = @UserId)) OR 
			(T.ActionId not in (540, 547, 558, 566)))
	   AND A.Type = 'Tasks'
	 ORDER BY T.TaskId
END