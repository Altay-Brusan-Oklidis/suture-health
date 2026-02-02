CREATE PROCEDURE [dbo].[MarkRequestRejected]
	@LegacyRequestIds IntegerKey READONLY,
	@MemberId INT,
	@TaskData VARCHAR(MAX)
AS
BEGIN
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
		signer_uf.UserId,
		signer_uf.FacilityId,
		529,
		r.Template,
		r.Patient,
		uf.UserId,
		@TaskData,
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId
	FROM @LegacyRequestIds rids
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON rids.Id = r.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON signer_uf.FacilityId = uf.FacilityId AND uf.UserId = @MemberId
	WHERE r.[Status] IS NULL;		-- Only allow in-flight requests to be rejected.
END