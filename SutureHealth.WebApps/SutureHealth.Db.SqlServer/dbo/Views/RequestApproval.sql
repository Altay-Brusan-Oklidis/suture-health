CREATE VIEW [dbo].[RequestApproval]
AS
SELECT
	RequestApprovalId	[RequestApprovalId],
	FormId				[SutureSignRequestId],
	UserId				[ApproverMemberId],
	ApprovalDate		[ApprovedAt]
FROM [$(SutureSignWeb)].dbo.RequestApproval
WHERE DeleteDate IS NULL