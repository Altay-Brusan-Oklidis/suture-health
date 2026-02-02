CREATE VIEW [dbo].[CpoType]
AS
SELECT
	SelectionId									[CpoTypeId],
	Selection									[Description],
	Active										[IsActive],
	ISNULL(SeqNumber, 0)						[SequenceNumber],
	ISNULL(TimeValue, 0)						[Minutes],
	CASE
		WHEN FacilityId > 0 THEN FacilityId
		ELSE NULL
	END											[OrganizationId],
	CASE
		WHEN UserId > 0 THEN UserId
		ELSE NULL
	END											[MemberId]
FROM [$(SutureSignWeb)].dbo.Selections
WHERE SelectionType = 'CPOTask' AND TimeValue IS NOT NULL;