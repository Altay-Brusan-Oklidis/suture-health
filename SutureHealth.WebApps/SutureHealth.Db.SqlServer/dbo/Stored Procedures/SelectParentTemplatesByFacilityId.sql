CREATE PROCEDURE [dbo].[SelectParentTemplatesByFacilityId]
	@FacilityId INT
AS
BEGIN
	DECLARE @Templates TABLE
	(
		TemplateId INT,
		TemplateName VARCHAR(100),
		Category VARCHAR(50),
		FacilityId INT
	);

	INSERT INTO @Templates
	EXEC [$(SutureSignWeb)].dbo.spGetTemplatesByFacilityId @FacilityId = @FacilityId, @ContextId = @FacilityId;

	SELECT
		COALESCE(tc.TemplateConfigurationId, 0)					[TemplateConfigurationId],
		t.TemplateId,
		COALESCE(tc.DocumentTypeKey, '')						[DocumentTypeKey],
		COALESCE(tc.TemplateProcessingModeId, 1)				[TemplateProcessingModeId],
		CAST(COALESCE(tc.[Enabled], 0) AS BIT)					[Enabled],
		COALESCE(tc.DateCreated, GETUTCDATE())					[DateCreated],
		COALESCE(tc.CreatedBy, 'dbo')							[CreatedBy],
		COALESCE(tc.DateModified, GETUTCDATE())					[DateModified],
		COALESCE(tc.ModifiedBy, 'dbo')							[ModifiedBy],
		tc.OCRDocumentId,
		CAST(CASE
			WHEN od.DateCompleted IS NOT NULL THEN 1 ELSE 0
		END AS BIT)												[OCRAnalysisAvailable],
		t.TemplateName,
		t.Category,
		t.FacilityId
	FROM
		@Templates t
			LEFT JOIN dbo.TemplateConfiguration tc ON t.TemplateId = tc.TemplateId
			LEFT JOIN dbo.OCRDocument od ON tc.OCRDocumentId = od.OCRDocumentId
	WHERE
		t.FacilityId = @FacilityId;
END