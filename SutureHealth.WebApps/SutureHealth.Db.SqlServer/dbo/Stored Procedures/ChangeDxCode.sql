CREATE PROCEDURE [dbo].[ChangeDxCode]
	@RequestId INT,
	@CurrentUserId INT,
	@OrganizationId INT,
	@DxCodeId INT,
	@ActionId INT
AS
	BEGIN
	SET NOCOUNT ON;
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
			@ActionId,
			r.Template,
			r.Patient,
			NULL,
			@CurrentUserId,
			dbo.GetSutureSignDate(),
			1,
			NULL,
			@OrganizationId,
			r.EffDate,
			r.StartOfCare,
			@DxCodeId
		FROM [$(SutureSignWeb)].dbo.Requests r
		WHERE r.Id = @RequestId;
END
