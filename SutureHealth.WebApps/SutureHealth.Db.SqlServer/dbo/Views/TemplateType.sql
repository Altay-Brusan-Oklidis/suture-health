CREATE VIEW [dbo].[TemplateType]
AS
SELECT
	TemplatePropertyId							[TemplateTypeId],
	ServiceCategory								[Category],
	ServiceCategoryDisplay						[CategoryName],
	TemplateDisplayName							[Name],
	TemplateShortName							[ShortName],
	CASE LOWER(ClinicalDate)
		WHEN 'start of care' THEN 1
		WHEN 'effective date' THEN 2
		ELSE 0
	END											[DateAssociationId],
	CAST(COALESCE(ShowIcd9, 0) AS BIT)			[ShowIcd9],
	CAST(COALESCE(AssociatePatient, 0) AS BIT)	[AssociatePatient],
	CAST(COALESCE(RequireDxCode, 0) AS BIT)		[RequireDxCode],
	CAST(COALESCE(Active, 0) AS BIT)			[IsActive],
	SignerChangeAllowed							[SignerChangeAllowed]
FROM
	[$(SutureSignWeb)].dbo.TemplateProperties