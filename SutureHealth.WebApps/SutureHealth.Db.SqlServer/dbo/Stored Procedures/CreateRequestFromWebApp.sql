CREATE PROCEDURE [dbo].[CreateRequestFromWebApp]

	@SubmitterUserId		INT,
	@SubmitterFacilityId	INT,
	@PhysicianId			INT,
	@PhysicianFacilityId	INT,
	@CollaboratorId			INT  = NULL,
	@AssistantId			INT  = NULL,
	@PatientId				INT,
	@EffectiveDate			DATE = NULL,
	@StartOfCare			DATE = NULL,
	@Icd9CodeId				INT  = NULL,
	@TemplateId				INT,
	@HasPreviewed			BIT = 0,
	@IsSurrogate			BIT = 0,
	@RequestId				INT OUTPUT

AS
BEGIN

	BEGIN TRANSACTION;

	DECLARE @FormId INT;
	DECLARE @CreateDate DATETIME2;
	DECLARE @SurrogateSubmitterOrgId INT;
	
	SET @CreateDate = dbo.GetSutureSignDate();
	SET @FormId = 1;
	SET @SurrogateSubmitterOrgId = CASE WHEN @IsSurrogate = 1 THEN @PhysicianFacilityId ELSE NULL END;

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
		SurrogateSubmitterOrgId,
		EffectiveDate,
		StartOfCare,
		ICD9CodeId
	)
	VALUES
	(
		@FormId,
		@SubmitterUserId,
		@SubmitterFacilityId,
		551,
		@TemplateId,
		@PatientId,
		1515,
		@SubmitterUserId,
		@CreateDate,
		1,
		@SubmitterFacilityId,
		@SurrogateSubmitterOrgId,
		@EffectiveDate,
		@StartOfCare,
		@Icd9CodeId
	);

	SET @RequestId = SCOPE_IDENTITY();
	UPDATE [$(SutureSignWeb)].dbo.Tasks
	   SET FormId = @RequestId
	 WHERE TaskId = @RequestId;

	IF @CollaboratorId IS NOT NULL
	BEGIN
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
			@CollaboratorId,
			@PhysicianFacilityId,
			575,
			@TemplateId,
			@PatientId,
			1515,
			@SubmitterUserId,
			@CreateDate,
			1,
			@SubmitterFacilityId,
			@EffectiveDate,
			@StartOfCare,
			@Icd9CodeId
		);
	END

	IF @HasPreviewed = 1
	BEGIN
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
			[Data],
			SubmittedByFacility,
			SurrogateSubmitterOrgId,
			EffectiveDate,
			StartOfCare,
			ICD9CodeId
		)
		VALUES
		(
			@RequestId,
			@SubmitterUserId,
			@SubmitterFacilityId,
			524,
			@TemplateId,
			@PatientId,
			1515,
			@SubmitterUserId,
			@CreateDate,
			1,
			CONCAT('Viewed on ', @CreateDate),
			@SubmitterFacilityId,
			@SurrogateSubmitterOrgId,
			@EffectiveDate,
			@StartOfCare,
			@Icd9CodeId
		);
	END

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
		SurrogateSubmitterOrgId,
		EffectiveDate,
		StartOfCare,
		ICD9CodeId
	)
	VALUES
	(
		@RequestId,
		@PhysicianId,
		@PhysicianFacilityId,
		553,
		@TemplateId,
		@PatientId,
		1515,
		@SubmitterUserId,
		@CreateDate,
		1,
		@SubmitterFacilityId,
		@SurrogateSubmitterOrgId,
		@EffectiveDate,
		@StartOfCare,
		@Icd9CodeId
	);

	IF (@AssistantId IS NOT NULL)
	BEGIN
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
			@AssistantId,
			@PhysicianFacilityId,
			526,
			@TemplateId,
			@PatientId,
			1515,
			@SubmitterUserId,
			@CreateDate,
			1,
			@SubmitterFacilityId,
			@EffectiveDate,
			@StartOfCare,
			@Icd9CodeId
		);
	END

	INSERT INTO [$(SutureSignWeb)].dbo.Facilities_Patients
	(
		PatientId,
		FacilityId,
		FacilityMRN,
		CompanyMRN,
		Active,
		CreateDate,
		ChangeDate,
		ChangeBy
	)
	SELECT TOP 1
		@PatientId,
		@SubmitterFacilityId,
		existing_p.FacilityMRN,
		existing_p.CompanyMRN,
		1,
		@CreateDate,
		@CreateDate,
		@SubmitterUserId
	FROM [$(SutureSignWeb)].dbo.Patients p
		LEFT JOIN [$(SutureSignWeb)].dbo.Facilities_Patients new_p ON p.PatientId = new_p.PatientId AND new_p.FacilityId = @SubmitterFacilityId AND new_p.Active = 1
		CROSS JOIN [$(SutureSignWeb)].dbo.Facilities submitter_f
		INNER JOIN [$(SutureSignWeb)].dbo.Facilities company_f ON submitter_f.CompanyId = company_f.CompanyId AND company_f.Active = 1
		INNER JOIN [$(SutureSignWeb)].dbo.Facilities_Patients existing_p ON p.PatientId = existing_p.PatientId AND company_f.FacilityId = existing_p.FacilityId AND existing_p.Active = 1
	WHERE p.PatientId = @PatientId AND new_p.PatientId IS NULL AND submitter_f.FacilityId = @SubmitterFacilityId
	ORDER BY existing_p.ChangeDate DESC, existing_p.CreateDate DESC, existing_p.FacilityId DESC;

	COMMIT;

END