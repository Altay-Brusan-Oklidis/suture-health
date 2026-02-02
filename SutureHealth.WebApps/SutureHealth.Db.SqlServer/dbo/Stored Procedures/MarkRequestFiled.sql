CREATE PROCEDURE [dbo].[MarkRequestFiled]
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
		566,
		r.Template,
		r.Patient,
		uf.UserId,
		'Marked as filed on ' + FORMAT(dbo.GetSutureSignDate(), 'M/d/yyyy') + ' at ' + FORMAT(dbo.GetSutureSignDate(), 'hh:mm tt'),
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId
	FROM @LegacyRequestIds rids
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON rids.Id = r.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities submitter_uf ON r.Submitter = submitter_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON uf.FacilityId IN (submitter_uf.FacilityId, signer_uf.FacilityId) AND uf.UserId = @MemberId
	WHERE ((r.SignFile IS NULL AND signer_uf.FacilityId = uf.FacilityId) OR (r.SubmFile IS NULL AND submitter_uf.FacilityId = uf.FacilityId))
		AND r.[Status] IS NOT NULL AND r.[Status] IN (1, 2);	-- Only allow a request to be filed if it's signed or rejected.
END