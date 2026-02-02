CREATE PROCEDURE [dbo].[MarkRequestAuditEvent]
	@LegacyRequestIds IntegerKey READONLY,
	@MemberId INT,
	@ActionId INT,
	@Data VARCHAR(MAX)
AS
BEGIN
	IF NOT EXISTS (SELECT TOP 1 0 FROM [$(SutureSignWeb)].dbo.Actions WHERE [ActionId] = @ActionId AND [Active] = 1)
	BEGIN
		RAISERROR('The ActionId specified is not valid.', 16, 1);
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
		uf.UserId,
		uf.FacilityId,
		@ActionId,
		r.Template,
		r.Patient,
		uf.UserId,
		@Data,
		GETUTCDATE(),
		1,
		uf.FacilityId
	FROM @LegacyRequestIds rids
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON rids.Id = r.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities submitter_uf ON r.Submitter = submitter_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON uf.FacilityId IN (submitter_uf.FacilityId, signer_uf.FacilityId) AND uf.UserId = @MemberId;
END