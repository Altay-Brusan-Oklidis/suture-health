CREATE PROCEDURE [dbo].[CreateOrganizationTemplate]
	@OrganizationId INT,
	@Name VARCHAR(100),
	@TemplateTypeId INT,
	@StorageKey VARCHAR(64),
	@MemberId INT
AS
BEGIN
	INSERT INTO [$(SutureSignWeb)].dbo.Templates
	(
		[FacilityDisplayName],
		[Descr],
		[FacilityId],
		[TemplatePropertyId],
		[TemplateType],
		[UseBaseFile],
		[Active],
		[DataS3Key],
		[DataS3CreatedDate],
		[DataS3CreatedByUserId]
	)
	SELECT
		@Name,
		@Name,
		@OrganizationId,
		@TemplateTypeId,
		tp.TemplateDisplayName,
		0,
		0,
		@StorageKey,
		dbo.GetSutureSignDate(),
		@MemberId
	FROM
		[$(SutureSignWeb)].dbo.TemplateProperties tp
	WHERE
		tp.TemplatePropertyId = @TemplateTypeId;

	IF (SCOPE_IDENTITY() IS NULL)
	RAISERROR('The template was not created.  Check your input values and try again.', 16, 1);

	SELECT SCOPE_IDENTITY() AS TemplateId;
END