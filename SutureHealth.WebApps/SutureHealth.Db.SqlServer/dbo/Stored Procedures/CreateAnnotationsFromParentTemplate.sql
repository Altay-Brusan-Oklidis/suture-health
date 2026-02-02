CREATE PROCEDURE [dbo].[CreateAnnotationsFromParentTemplate]
	@DestinationTemplateId INT,
	@DeleteExistingAnnotations BIT = 1,
	@ParentTemplateId INT = NULL
AS
BEGIN
	SET @DeleteExistingAnnotations = COALESCE(@DeleteExistingAnnotations, 1);

	IF (@DeleteExistingAnnotations = 1)
	BEGIN
		DELETE FROM [$(SutureSignWeb)].dbo.TemplateCoordinates
		WHERE TemplateId = @DestinationTemplateId;
	END

	IF @ParentTemplateId IS NULL
	BEGIN
		SELECT
			@ParentTemplateId = ParentTemplateId
		FROM
			[$(SutureSignWeb)].dbo.Templates
		WHERE
			TemplateId = @DestinationTemplateId;
	END

	INSERT INTO [$(SutureSignWeb)].dbo.TemplateCoordinates
		(
			TemplateId,
			Placeholder,
			coordLeft,
			coordBottom,
			coordRight,
			coordTop,
			EffectiveDate,
			EndDate,
			DateMod,
			PageNumber,
			FieldId,
			ToolTips,
			FieldLabel,
			[Required],
			[Value],
			htmlCoordinateLeft,
			htmlCoordinateBottom,
			htmlCoordinateRight,
			htmlCoordinateTop,
			htmlPageHeight
		)
		SELECT
			@DestinationTemplateId,
			Placeholder,
			coordLeft,
			coordBottom,
			coordRight,
			coordTop,
			dbo.GetSutureSignDate(),
			NULL,
			dbo.GetSutureSignDate(),
			PageNumber,
			FieldId,
			ToolTips,
			FieldLabel,
			[Required],
			[Value],
			htmlCoordinateLeft,
			htmlCoordinateBottom,
			htmlCoordinateRight,
			htmlCoordinateTop,
			htmlPageHeight
		FROM
			[$(SutureSignWeb)].dbo.TemplateCoordinates
		WHERE
			TemplateId = @ParentTemplateId
			and (Placeholder='VisibleSignature' or Placeholder='DateSigned' or Placeholder='TextArea' or Placeholder='CheckBox' )
END