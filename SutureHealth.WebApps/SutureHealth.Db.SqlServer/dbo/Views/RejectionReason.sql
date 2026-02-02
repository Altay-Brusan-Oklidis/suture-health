CREATE VIEW [dbo].[RejectionReason]
AS
SELECT
	SelectionId									[RejectionReasonId],
	Selection									[Description],
	Active										[IsActive],
	ISNULL(SeqNumber, 0)						[SequenceNumber],
	CASE
		WHEN FacilityId > 0 THEN FacilityId
		ELSE NULL
	END											[OrganizationId],
	CASE
		WHEN UserId > 0 THEN UserId
		ELSE NULL
	END											[MemberId]
FROM [$(SutureSignWeb)].dbo.Selections
WHERE SelectionType = 'RejectionReasons';