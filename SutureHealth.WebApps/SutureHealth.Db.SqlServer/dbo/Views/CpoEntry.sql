CREATE VIEW [dbo].[CpoEntry]
AS
SELECT
	Id							[CpoEntryId],
	ISNULL(UserId, 0)			[MemberId],
	ISNULL(FacilityId, 0)		[OrganizationId],
	ISNULL(PatientId, 0)		[PatientId],
	RequestId					[SutureSignRequestId],
	ISNULL([Type], 1)			[Type],
	ISNULL([Minutes], 0)		[Minutes],
	ISNULL([Date], GETDATE())	[EffectiveAt],
	ISNULL(Comment, '')			[Comment],
	CreateDate					[CreatedAt]
FROM [$(SutureSignWeb)].dbo.TimeTracking