CREATE PROCEDURE [dbo].[MarkRequestIncomplete]
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
		549,
		r.Template,
		r.Patient,
		uf.UserId,
		'Incomplete (' + FORMAT(dbo.GetSutureSignDate(), 'M/d/yyyy') + ')',
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId
	FROM @LegacyRequestIds rids
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON rids.Id = r.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities submitter_uf ON r.Submitter = submitter_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON submitter_uf.FacilityId = uf.FacilityId AND uf.UserId = @MemberId
	WHERE r.[Status] IS NULL;	-- Only allow a request to be retracted if it's pending (not in a terminal state).
END