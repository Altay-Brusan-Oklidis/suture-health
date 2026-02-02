CREATE VIEW [dbo].[CpoEntrySelection]
AS
SELECT
	Id								[CpoEntrySelectionId],
	ISNULL(TimeTrackingId, 0)		[CpoEntryId],
	ISNULL(SelectionId, 0)			[CpoTypeId]
FROM [$(SutureSignWeb)].dbo.TimeTracking_Selections