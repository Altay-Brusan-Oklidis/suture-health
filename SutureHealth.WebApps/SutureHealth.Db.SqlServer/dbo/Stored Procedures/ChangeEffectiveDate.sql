CREATE PROCEDURE [dbo].[ChangeEffectiveDate]
	@RequestId INT,
	@CurrentUserId INT,
	@OrganizationId INT,
	@EffectiveDate DATETIME
AS
	BEGIN
		SET NOCOUNT ON;

		DECLARE @ClinicalDate VARCHAR(50)
		SELECT @ClinicalDate = ClinicalDate FROM [$(SutureSignWeb)].dbo.TemplateProperties tp
		INNER JOIN [$(SutureSignWeb)].dbo.Templates t ON t.TemplatePropertyId = tp.TemplatePropertyId
		INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON r.Template = t.TemplateId
		WHERE r.Id = @RequestId

		IF @ClinicalDate = 'Effective Date'
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
				r.SignerId,
				r.SignerOrgId,
				628,
				r.Template,
				r.Patient,
				NULL,
				@CurrentUserId,
				dbo.GetSutureSignDate(),
				1,
				NULL,
				@OrganizationId,
				@EffectiveDate,
				r.StartOfCare,
				r.ICD9CodeId
			FROM [$(SutureSignWeb)].dbo.Requests r
			WHERE r.Id = @RequestId;
		END
		IF @ClinicalDate = 'Start of Care'
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
				r.SignerId,
				r.SignerOrgId,
				628,
				r.Template,
				r.Patient,
				NULL,
				@CurrentUserId,
				dbo.GetSutureSignDate(),
				1,
				NULL,
				@OrganizationId,
				r.EffDate,
				@EffectiveDate,
				r.ICD9CodeId
			FROM [$(SutureSignWeb)].dbo.Requests r
			WHERE r.Id = @RequestId;
		END
END
