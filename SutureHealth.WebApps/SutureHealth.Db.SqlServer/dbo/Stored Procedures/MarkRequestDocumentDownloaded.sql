CREATE PROCEDURE [dbo].[MarkRequestDocumentDownloaded]
	@RequestDocumentId BIGINT,
	@SubmitterUserFacilityId INT = NULL,
	@ArchiveAndFile BIT = 1
AS
BEGIN
	DECLARE @LegacyRequestId BIGINT = NULL,
			@PatientId INT = NULL,
			@TemplateId INT = NULL,
			@SubmitterUserId INT = NULL,
			@SubmitterFacilityId INT = NULL,
			@FileName NVARCHAR(256);

	SET @ArchiveAndFile = COALESCE(@ArchiveAndFile, 1);

	SELECT
		@LegacyRequestId = nr.SutureSignRequestID,
		@PatientId = lr.Patient,
		@TemplateId = lr.Template,
		@SubmitterUserFacilityId = COALESCE(@SubmitterUserFacilityId, lr.Submitter),
		@FileName = rd.[FileName]
	FROM
		dbo.RequestDocument rd
			INNER JOIN dbo.TransmittedRequest nr ON rd.RequestId = nr.TransmittedRequestId
			INNER JOIN [$(SutureSignWeb)].dbo.Requests lr ON nr.SutureSignRequestID = lr.Id
	WHERE
		rd.Id = @RequestDocumentId;

	SELECT
		@SubmitterUserId = UserId,
		@SubmitterFacilityId = FacilityId
	FROM
		[$(SutureSignWeb)].dbo.Users_Facilities
	WHERE
		Id = @SubmitterUserFacilityId;

	IF (@ArchiveAndFile = 1)
	BEGIN
		DECLARE @RequestIds IntegerKey;
		INSERT INTO @RequestIds VALUES (@LegacyRequestId);

		EXEC dbo.MarkRequestArchived
			@MemberId = @SubmitterUserId,
			@LegacyRequestIds = @RequestIds;

		EXEC dbo.MarkRequestFiled
			@MemberId = @SubmitterUserId,
			@LegacyRequestIds = @RequestIds;
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
	VALUES
	(
		@LegacyRequestId,
		@SubmitterUserId,
		@SubmitterFacilityId,
		558,
		@TemplateId,
		@PatientId,
		@SubmitterUserId,
		'PDF Name: ' + @FileName + ', Downloaded on: ' + FORMAT(dbo.GetSutureSignDate(), 'M/d/yyyy hh:mm tt'),
		dbo.GetSutureSignDate(),
		1,
		@SubmitterFacilityId
	);
END