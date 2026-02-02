CREATE PROCEDURE [dbo].[ForwardRequest]
	@SutureSignRequestId INT,
	@MemberId INT,
	@SigningMemberId INT,
	@OrganizationId INT,
	@CollaboratorMemberId INT,
	@AssistantMemberId INT
AS
BEGIN
	DECLARE @CurrentSigningMemberId INT,
			@CurrentOrganizationId INT,
			@CurrentCollaboratorMemberId INT,
			@CurrentAssistantMemberId INT,
			@MemberOrganizationId INT;

	SELECT @MemberOrganizationId = SignerOrgId
	FROM [$(SutureSignWeb)].dbo.Requests
	WHERE  Id=@SutureSignRequestId;

	SELECT
		@CollaboratorMemberId = COALESCE(@CollaboratorMemberId, -1),
		@AssistantMemberId = COALESCE(@AssistantMemberId, -1),
		@CurrentSigningMemberId = SignerId,
		@CurrentOrganizationId = SignerOrgId,
		@CurrentCollaboratorMemberId = COALESCE(CollaboratorId, -1),
		@CurrentAssistantMemberId = COALESCE(AssistantId, -1)
	FROM [$(SutureSignWeb)].dbo.Requests
	WHERE Id = @SutureSignRequestId;

	IF (@CurrentSigningMemberId <> @SigningMemberId OR @CurrentOrganizationId <> @OrganizationId)
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
			EffectiveDate,
			StartOfCare,
			ICD9CodeId
		)
		SELECT TOP 1
			FormId,
			@SigningMemberId,
			@OrganizationId,
			653,
			TemplateId,
			PatientId,
			RuleId,
			@MemberId,
			dbo.GetSutureSignDate(),
			Active,
			[Data],
			@MemberOrganizationId,
			EffectiveDate,
			StartOfCare,
			ICD9CodeId
		FROM [$(SutureSignWeb)].dbo.Tasks
		WHERE FormId = @SutureSignRequestId AND ActionId = 553
		ORDER BY TaskId DESC;
	END

	IF (@CurrentCollaboratorMemberId <> @CollaboratorMemberId)
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
			EffectiveDate,
			StartOfCare,
			ICD9CodeId
		)
		SELECT
			r.Id,
			@CollaboratorMemberId,
			@OrganizationId,
			575,
			r.Template,
			r.Patient,
			1515,
			@MemberId,
			dbo.GetSutureSignDate(),
			1,
			NULL,
			@MemberOrganizationId,
			dbo.GetSutureSignDate(),
			r.StartOfCare,
			r.ICD9CodeId
		FROM [$(SutureSignWeb)].dbo.Requests r
		WHERE r.Id = @SutureSignRequestId;
	END

	IF (@CurrentAssistantMemberId <> @AssistantMemberId)
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
			EffectiveDate,
			StartOfCare,
			ICD9CodeId
		)
		SELECT
			r.Id,
			@AssistantMemberId,
			@OrganizationId,
			526,
			r.Template,
			r.Patient,
			1515,
			@MemberId,
			dbo.GetSutureSignDate(),
			1,
			NULL,
			@MemberOrganizationId,
			dbo.GetSutureSignDate(),
			r.StartOfCare,
			r.ICD9CodeId
		FROM [$(SutureSignWeb)].dbo.Requests r
		WHERE r.Id = @SutureSignRequestId;

		UPDATE [$(SutureSignWeb)].dbo.Users_Managers
		SET LastUsed = CASE WHEN UserId = @AssistantMemberId AND Active = 1 THEN 1 ELSE 0 END
		WHERE ManagerId = @SigningMemberId;
	END
END