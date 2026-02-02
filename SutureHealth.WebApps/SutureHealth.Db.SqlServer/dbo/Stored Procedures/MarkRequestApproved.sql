CREATE PROCEDURE [dbo].[MarkRequestApproved]
	@LegacyRequestIds IntegerKey READONLY,
	@MemberId INT,
	@ActionId INT
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
		@ActionId,
		r.Template,
		r.Patient,
		uf.UserId,
		CAST(
			(SELECT
				Placeholder,
				coordLeft,
				coordBottom,
				coordRight,
				coordTop,
				PageNumber,
				[Required],
				[Value],
				htmlCoordinateLeft,
				htmlCoordinateBottom,
				htmlCoordinateRight,
				htmlCoordinateTop
			FROM [$(SutureSignWeb)].dbo.TemplateCoordinates
			WHERE TemplateId = r.Template
			FOR XML PATH ('Annotation')) AS VARCHAR(MAX)),
		dbo.GetSutureSignDate(),
		1,
		uf.FacilityId
	FROM @LegacyRequestIds rids
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON rids.Id = r.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf ON r.Signer = signer_uf.Id
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON signer_uf.FacilityId = uf.FacilityId AND uf.UserId = @MemberId
	WHERE r.[Status] IS NULL;		-- Only allow in-flight requests to be approved.
END