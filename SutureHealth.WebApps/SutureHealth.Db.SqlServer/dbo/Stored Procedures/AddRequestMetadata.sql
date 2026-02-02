CREATE PROCEDURE [dbo].[AddRequestMetadata]
	@SutureSignRequestId INT,
	@Metadata VARCHAR(MAX),
	@MemberId INT,
	@IsComplete BIT
AS
BEGIN
	UPDATE t
	SET t.[Active] = 0
	FROM [$(SutureSignWeb)].dbo.Tasks t
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON t.FormId = r.Id
	WHERE t.FormId = @SutureSignRequestId AND t.ActionId = 527 AND r.[Status] IS NULL;

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
		[SubmittedByFacility],
		[IsComplete]
	)
	SELECT
		@SutureSignRequestId,
		signer_uf.UserId,
		signer_uf.FacilityId,
		527,
		r.Template,
		r.Patient,
		uf.UserId,
		@Metadata,
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId,
		@IsComplete
	FROM [$(SutureSignWeb)].dbo.Requests r
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON signer_uf.FacilityId = uf.FacilityId AND uf.UserId = @MemberId
	WHERE r.Id = @SutureSignRequestId AND r.[Status] IS NULL;		-- Only allow in-flight requests to have metadata added.
END