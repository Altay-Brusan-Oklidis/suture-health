CREATE PROCEDURE [dbo].[MarkRequestArchived]
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
		547,
		r.Template,
		r.Patient,
		uf.UserId,
		'Archived (' + FORMAT(dbo.GetSutureSignDate(), 'M/d/yyyy') + ')',
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId
	FROM @LegacyRequestIds rids
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON rids.Id = r.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities submitter_uf ON r.Submitter = submitter_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON submitter_uf.FacilityId = uf.FacilityId AND uf.UserId = @MemberId
	WHERE r.SubmArchive IS NULL AND r.[Status] IS NOT NULL AND r.[Status] IN (1, 2, 3);	-- Only allow a request to be archived if it's signed, rejected, or retracted.
END
