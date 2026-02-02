CREATE VIEW [dbo].[Template]
AS
SELECT
	TemplateId,
	FacilityDisplayName				[Name],
	FacilityId						[OrganizationId],
	TemplatePropertyId				[TemplateTypeId],
	ParentTemplateId,
	Active							[IsActive],
	DataS3Key						[StorageKey],
	DataS3CreatedByUserId			[StoredByMemberId],
	DataS3CreatedDate				[DateStored]
FROM
	[$(SutureSignWeb)].dbo.Templates
