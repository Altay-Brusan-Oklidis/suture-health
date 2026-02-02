CREATE PROCEDURE [dbo].[CreateAnnotations]
	@DestinationTemplateId INT,
	@DeleteExistingAnnotations BIT = 1,
	@ActivateTemplate BIT = 0,
	@Annotations TemplateAnnotation READONLY
AS
BEGIN
	SET @DeleteExistingAnnotations = COALESCE(@DeleteExistingAnnotations, 1);
	SET @ActivateTemplate = COALESCE(@ActivateTemplate, 0);

	IF (@DeleteExistingAnnotations = 1)
	BEGIN
		DELETE FROM [$(SutureSignWeb)].dbo.TemplateCoordinates
		WHERE TemplateId = @DestinationTemplateId;
	END

	INSERT INTO [$(SutureSignWeb)].dbo.TemplateCoordinates
	(
		TemplateId,
		Placeholder,
		EffectiveDate,
		DateMod,
		PageNumber,
		htmlCoordinateLeft,
		htmlCoordinateBottom,
		htmlCoordinateRight,
		htmlCoordinateTop,
		htmlPageHeight,
		[Value],
		[Required]
	)
	SELECT
		@DestinationTemplateId,
		CASE [AnnotationFieldTypeId]
			WHEN 1 THEN 'VisibleSignature'
			WHEN 2 THEN 'DateSigned'
			WHEN 3 THEN 'CheckBox'
			WHEN 4 THEN 'TextArea'
			ELSE 'Unknown'
		END,
		dbo.GetSutureSignDate(),
		dbo.GetSutureSignDate(),
		[PageNumber],
		[Left],
		[Bottom],
		[Right],
		[Top],
		[PageHeight],
		[Value],
		CASE [AnnotationFieldTypeId] WHEN 4 THEN 1 ELSE NULL END
	FROM	
		@Annotations;

	IF (@ActivateTemplate = 1)
	BEGIN
		UPDATE [$(SutureSignWeb)].dbo.Templates
		SET [Active] = 1
		WHERE TemplateId = @DestinationTemplateId;
	END
END
