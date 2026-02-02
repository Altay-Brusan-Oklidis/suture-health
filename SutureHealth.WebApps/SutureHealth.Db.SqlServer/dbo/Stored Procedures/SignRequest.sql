CREATE PROCEDURE [dbo].[SignRequest]
	@SutureSignRequestId INT,
	@MemberId INT,
	@ActionId INT,
	@StorageKey VARCHAR(64),
	@SignatureToken NVARCHAR(MAX),
	@Pid VARCHAR(64)
AS
BEGIN
	-- TODO: Add logic/support for PDF regeneration (ActionId: 565)
	-- NOTE: @MemberId must belong to the signer's facility or no rows will be inserted.  This will need to be refactored if/when support for admin PDF regeneration is needed.
	IF (@ActionId NOT IN (528, 555, 556, 557))
	BEGIN
		RAISERROR('The @ActionId specified is not supported.', 16, 1);
		RETURN;
	END

	-- Sign task
	INSERT INTO [$(SutureSignWeb)].dbo.Tasks
	(
		[FormId],
		[UserId],
		[FacilityId],
		[ActionId],
		[TemplateId],
		[PatientId],
		[SubmittedBy],
		[Data],
		[CreateDate],
		[Active],
		[SubmittedByFacility]
	)
	SELECT
		@SutureSignRequestId,
		signer_uf.UserId,
		signer_uf.FacilityId,
		@ActionId,
		r.Template,
		r.Patient,
		@MemberId,
		@SignatureToken,
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId
	FROM [$(SutureSignWeb)].dbo.Requests r
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON signer_uf.FacilityId = uf.FacilityId AND uf.UserId = @MemberId
	WHERE r.Id = @SutureSignRequestId;

	-- PDF generation task
	INSERT INTO [$(SutureSignWeb)].dbo.Tasks
	(
		[FormId],
		[UserId],
		[FacilityId],
		[ActionId],
		[TemplateId],
		[PatientId],
		[SubmittedBy],
		[Data],
		[CreateDate],
		[Active],
		[SubmittedByFacility]
	)
	SELECT
		@SutureSignRequestId,
		signer_uf.UserId,
		signer_uf.FacilityId,
		541,
		r.Template,
		r.Patient,
		@MemberId,
		NULL,
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId
	FROM [$(SutureSignWeb)].dbo.Requests r
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON signer_uf.FacilityId = uf.FacilityId AND uf.UserId = @MemberId
	WHERE r.Id = @SutureSignRequestId;

	IF EXISTS (SELECT 1 FROM [$(SutureSignWeb)].dbo.Outcomes WITH (NOLOCK) WHERE FormId = @SutureSignRequestId)
	BEGIN
		UPDATE o
		SET o.ActionId = @ActionId,
			o.Id = @Pid,
			o.DataS3Key = @StorageKey,
			o.DataS3CreatedDate = dbo.GetSutureSignDate(),
			o.DataS3CreatedByUserId = @MemberId
		FROM [$(SutureSignWeb)].dbo.Outcomes o
		WHERE o.FormId = @SutureSignRequestId;
	END
	ELSE
	BEGIN
		INSERT INTO [$(SutureSignWeb)].dbo.Outcomes
		(
			[PatientId],
			[PrimarySigner],
			[UserId],
			[FacilityId],
			[TemplateId],
			[ActionId],
			[FormId],
			[Active],
			[Id],
			[CreateDate],
			[SubmittingFacilityId],
			[TemplatePropertyId],
			[SigningActionId],
			[DataS3Key],
			[DataS3CreatedDate],
			[DataS3CreatedByUserId]
		)
		SELECT
			r.Patient,
			signer_uf.UserId,
			@MemberId,
			signer_uf.FacilityId,
			r.Template,
			541,
			r.Id,
			1,
			@Pid,
			dbo.GetSutureSignDate(),
			signer_uf.FacilityId,
			t.TemplatePropertyId,
			@ActionId,
			@StorageKey,
			dbo.GetSutureSignDate(),
			@MemberId
		FROM [$(SutureSignWeb)].dbo.Requests r
			INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
			INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON signer_uf.FacilityId = uf.FacilityId AND uf.UserId = @MemberId
			INNER JOIN [$(SutureSignWeb)].dbo.Templates t ON r.Template = t.TemplateId
		WHERE r.Id = @SutureSignRequestId;
	END
END