CREATE PROCEDURE [dbo].[MarkRequestDataSubmission]
	@SutureSignRequestId INT,
	@MemberId INT,
	@Data VARCHAR(MAX),
	@IsComplete BIT
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
		[SubmittedByFacility],
		[IsComplete]
	)
	SELECT
		@SutureSignRequestId,
		uf.UserId,
		uf.FacilityId,
		527,
		r.Template,
		r.Patient,
		uf.UserId,
		@Data,
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId,
		@IsComplete
	FROM [$(SutureSignWeb)].dbo.Requests r
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities submitter_uf ON r.Submitter = submitter_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON uf.FacilityId IN (submitter_uf.FacilityId, signer_uf.FacilityId) AND uf.UserId = @MemberId
	WHERE r.Id = @SutureSignRequestId;
END
