CREATE PROCEDURE [dbo].[CreateRequestTemplate]
	@ParentTemplateId INT,
	@SubmitterOrganizationId INT,
	@SubmitterMemberId INT,
	@DataS3Key VARCHAR(64),
	@TemplateId INT OUTPUT
AS
BEGIN
	SET @TemplateId = NULL;

	INSERT INTO [$(SutureSignWeb)].dbo.Templates
	(
		FacilityDisplayName,
		Descr,
		FacilityId,
		TemplatePropertyId,
		ParentTemplateId,
		TemplateType,
		UseBaseFile,
		Active,
		DataS3Key,
		DataS3CreatedDate,
		DataS3CreatedByUserId
	)
	SELECT pt.FacilityDisplayName,
		   'Uploaded Template for ' + pt.FacilityDisplayName,
		   @SubmitterOrganizationId,
		   pt.TemplatePropertyId,
		   @ParentTemplateId,
		   pt.TemplateType,
		   0,
		   1,
		   @DataS3Key,
		   dbo.GetSutureSignDate(),
		   @SubmitterMemberId
	  FROM [$(SutureSignWeb)].dbo.Templates pt
     INNER JOIN [$(SutureSignWeb)].dbo.TemplateProperties ptp ON pt.TemplatePropertyId = ptp.TemplatePropertyId
     WHERE pt.TemplateId = @ParentTemplateId;

	SET @TemplateId = SCOPE_IDENTITY();

	INSERT INTO [$(SutureSignWeb)].dbo.TemplateDetails
	(
		TemplateId,
		TemplateFileName,
		FormUrl,
		ImageFolderPath,
		CreateDate,
		EffectiveDate,
		SubmittedById
	)
	VALUES
	(
		@TemplateId,
		CAST(@TemplateId AS VARCHAR) + '.pdf',
		'TemplateDoNotUseBase',
		'',
		dbo.GetSutureSignDate(),
		dbo.GetSutureSignDate(),
		@SubmitterMemberId
	);
END