CREATE PROCEDURE [dbo].[MarkRequestUnapproved]
	@LegacyRequestIds IntegerKey READONLY,
	@MemberId INT,
	@ActionId INT
AS
BEGIN
	SET @ActionId = COALESCE(@ActionId, 0);

	IF (@ActionId NOT IN (625, 626))
	BEGIN
		RAISERROR('The ActionId specified is not valid.', 16 ,1);
		RETURN;
	END

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
		rids.Id,
		@MemberId,
		uf.FacilityId,
		@ActionId,
		r.Template,
		r.Patient,
		@MemberId,
		NULL,
		GETUTCDATE(),
		1,
		uf.FacilityId
	FROM @LegacyRequestIds rids
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON rids.Id = r.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON signer_uf.FacilityId = uf.FacilityId AND uf.UserId = @MemberId
	WHERE r.[Status] IS NULL;	-- Only allow non-terminal documents to be unapproved by signer-side users
END