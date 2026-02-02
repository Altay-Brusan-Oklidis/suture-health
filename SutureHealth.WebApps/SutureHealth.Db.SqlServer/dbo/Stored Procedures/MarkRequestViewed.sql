CREATE PROCEDURE [dbo].[MarkRequestViewed]
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
		CASE
			WHEN r.[Status] IS NOT NULL THEN 540	-- Viewing a rendered PDF in a terminal state
			ELSE 524								-- Viewing an unsigned document in the editor
		END,
		r.Template,
		r.Patient,
		uf.UserId,
		'Viewed on ' + FORMAT(dbo.GetSutureSignDate(), 'M/d/yyyy hh:mm tt'),
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId
	FROM @LegacyRequestIds rids
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON rids.Id = r.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities submitter_uf ON r.Submitter = submitter_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON uf.FacilityId IN (submitter_uf.FacilityId, signer_uf.FacilityId) AND uf.UserId = @MemberId
END