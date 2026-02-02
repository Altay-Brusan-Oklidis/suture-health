CREATE PROCEDURE [dbo].[CreateRequest]
	@PatientId INT,
	@SignerUserFacilityId INT,
	@SubmitterUserFacilityId INT,
	@TemplateId INT,
	@EffectiveDate DATE,
	@StartOfCare DATE,
	@Icd9CodeId INT,
	@RequestId INT OUTPUT
AS
BEGIN
	DECLARE @SignerUserId INT,
			@SignerFacilityId INT,
			@SubmitterUserId INT,
			@SubmitterFacilityId INT;

	SET @RequestId = NULL;
	SELECT TOP 1 @RequestId = Id FROM [$(SutureSignWeb)].dbo.Requests WHERE Signer = @SignerUserFacilityId AND Template = @TemplateId ORDER BY [TimeStamp] DESC

	-- Instead of erroring, return the request that was already created using the provided TemplateId.
	IF (@RequestId IS NOT NULL)
	BEGIN
		RETURN;
	END

	SELECT
		@SignerUserId = UserId,
		@SignerFacilityId = FacilityId
	FROM
		[$(SutureSignWeb)].dbo.Users_Facilities
	WHERE
		Id = @SignerUserFacilityId;

	SELECT
		@SubmitterUserId = UserId,
		@SubmitterFacilityId = FacilityId
	FROM
		[$(SutureSignWeb)].dbo.Users_Facilities
	WHERE
		Id = @SubmitterUserFacilityId;

	-- CreateRequest (Action: 551)
	INSERT INTO [$(SutureSignWeb)].dbo.Tasks
	(
		FormId,
		UserId,
		FacilityId,
		ActionId,
		TemplateId,
		PatientId,
		RuleId,
		SubmittedBy,
		CreateDate,
		Active,
		SubmittedByFacility,
		EffectiveDate,
		StartOfCare,
		ICD9CodeId
	)
	VALUES
	(
		1,
		@SubmitterUserId,
		@SubmitterFacilityId,
		551,
		@TemplateId,
		@PatientId,
		1515,
		@SubmitterUserId,
		dbo.GetSutureSignDate(),
		1,
		@SubmitterFacilityId,
		@EffectiveDate,
		@StartOfCare,
		@Icd9CodeId
	);

	SET @RequestId = SCOPE_IDENTITY();
	UPDATE [$(SutureSignWeb)].dbo.Tasks
	SET FormId = @RequestId
	WHERE TaskId = @RequestId;

	-- RouteToQueue (Action: 553)
	INSERT INTO [$(SutureSignWeb)].dbo.Tasks
	(
		FormId,
		UserId,
		FacilityId,
		ActionId,
		TemplateId,
		PatientId,
		RuleId,
		SubmittedBy,
		CreateDate,
		Active,
		SubmittedByFacility,
		EffectiveDate,
		StartOfCare,
		ICD9CodeId
	)
	VALUES
	(
		@RequestId,
		@SignerUserId,
		@SignerFacilityId,
		553,
		@TemplateId,
		@PatientId,
		1515,
		@SubmitterUserId,
		dbo.GetSutureSignDate(),
		1,
		@SubmitterFacilityId,
		@EffectiveDate,
		@StartOfCare,
		@Icd9CodeId
	);
END