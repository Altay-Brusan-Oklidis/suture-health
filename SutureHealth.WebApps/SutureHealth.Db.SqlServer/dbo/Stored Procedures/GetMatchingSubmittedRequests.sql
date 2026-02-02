CREATE PROCEDURE [dbo].[GetMatchingSubmittedRequests]
	@SubmitterFacilityId	INT,
	@PatientId				INT,
	@PhysicianId			INT,
	@TemplateID				INT,
	@ClinicalDate			DATETIME,
	@AlreadyExists			INT = 0 OUT
AS
BEGIN
	DECLARE @ParentTemplateId INT;

	SET @AlreadyExists = 0;
	SELECT
		@ParentTemplateId = COALESCE(ParentTemplateId, TemplateId)
	FROM
		[$(SutureSignWeb)].dbo.Templates WITH (NOLOCK)
	WHERE
		TemplateId = @TemplateId;

	SELECT
		@AlreadyExists = 1
	FROM
		[$(SutureSignWeb)].dbo.Requests r WITH (NOLOCK)
			INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities signer_uf WITH (NOLOCK) ON r.Signer = signer_uf.Id
			INNER JOIN [$(SutureSignWeb)].dbo.Templates t WITH (NOLOCK) ON r.Template = t.TemplateId
			INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities submitter_uf WITH (NOLOCK) ON r.Submitter = submitter_uf.Id
	WHERE
		r.Patient = @PatientId AND
		t.ParentTemplateId = @ParentTemplateId AND
		signer_uf.UserId = @PhysicianId AND
		submitter_uf.FacilityId = @SubmitterFacilityId AND
		COALESCE(R.EffDate, R.StartOfCare) = CAST(@ClinicalDate AS DATE);
END