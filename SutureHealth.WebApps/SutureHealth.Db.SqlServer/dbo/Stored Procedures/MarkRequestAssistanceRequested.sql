CREATE PROCEDURE [dbo].[MarkRequestAssistanceRequested]
	@LegacyRequestIds IntegerKey READONLY,
	@MemberId INT,
	@AssistantMemberId INT
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
		@AssistantMemberId,
		uf.FacilityId,
		526,
		r.Template,
		r.Patient,
		uf.UserId,
		NULL,
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId
	FROM @LegacyRequestIds rids
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON rids.Id = r.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON signer_uf.FacilityId = uf.FacilityId AND uf.UserId = @MemberId;
END