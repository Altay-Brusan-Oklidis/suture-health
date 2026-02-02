CREATE PROCEDURE [dbo].[MarkRequestResent]
	@LegacyRequestIds IntegerKey READONLY,
	@MemberId INT
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
		uf.UserId,
		uf.FacilityId,
		753,
		r.Template,
		r.Patient,
		uf.UserId,
		NULL,	-- NOTE: May need to pull XML from the 553 or 527
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId
	FROM @LegacyRequestIds rids
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON rids.Id = r.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities submitter_uf ON r.Submitter = submitter_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON uf.FacilityId IN (submitter_uf.FacilityId, signer_uf.FacilityId) AND uf.UserId = @MemberId
		INNER JOIN [$(SutureSignWeb)].dbo.Tasks terminal_t ON r.St = terminal_t.TaskId
	WHERE terminal_t.ActionId IN (529, 549);	-- Only allow a request to be resent if it's currently rejected or retracted.
END