CREATE VIEW [dbo].[RequestViewHistory]
AS
SELECT
	MAX(TaskId)				[RequestViewHistoryId],
	FormId					[SutureSignRequestId],
	UserId					[MemberId],
	ActionId				[ViewHistoryTypeId],
	MAX(CreateDate)			[LastViewedAt]
FROM [$(SutureSignWeb)].dbo.Tasks WITH (NOLOCK)
WHERE ActionId IN (524, 540) AND Active = 1
GROUP BY FormId, UserId, ActionId