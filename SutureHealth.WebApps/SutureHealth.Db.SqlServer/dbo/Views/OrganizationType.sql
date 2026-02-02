CREATE VIEW [dbo].[OrganizationType]
AS
SELECT
	FacilityTypeId			[OrganizationTypeId],
	[Name]
FROM [$(SutureSignWeb)].dbo.FacilityTypes WITH (NOLOCK)