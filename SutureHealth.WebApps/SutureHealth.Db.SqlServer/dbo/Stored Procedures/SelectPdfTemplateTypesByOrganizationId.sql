CREATE PROCEDURE [dbo].[SelectPdfTemplateTypesByOrganizationId]
	@OrganizationId INT
AS
BEGIN
	DECLARE @OrganizationCategory TABLE (CategoryKey VARCHAR(1000) NOT NULL);

	INSERT INTO @OrganizationCategory
	SELECT TRIM(ItemString) FROM dbo.OrganizationSetting
	WHERE ParentId = @OrganizationId AND [Key] = 'Services'

	SELECT tt.*
	FROM
		(
			SELECT DISTINCT TemplateTypeId
			FROM TemplateType tt
				CROSS APPLY STRING_SPLIT(Category, ',')
				LEFT JOIN @OrganizationCategory oc ON TRIM([value]) = oc.CategoryKey
			WHERE
				ShortName != 'Face-to-Face' AND
				(Category = 'General' OR oc.CategoryKey IS NOT NULL)
		) tt_ids
		INNER JOIN TemplateType tt
			ON tt_ids.TemplateTypeId = tt.TemplateTypeId;
END