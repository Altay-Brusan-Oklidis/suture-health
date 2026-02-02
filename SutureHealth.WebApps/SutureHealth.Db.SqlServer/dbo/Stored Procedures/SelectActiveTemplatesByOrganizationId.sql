CREATE PROCEDURE [dbo].[SelectActiveTemplatesByOrganizationId]
	@OrganizationId INT
AS
BEGIN
	DECLARE @Templates TABLE
	(
		TemplateId INT,
		TemplateName VARCHAR(512),
		Category VARCHAR(512),
		FacilityId INT
	);

	-- Get Org/Company Templates
	INSERT INTO @Templates
	EXEC [$(SutureSignWeb)].dbo.spGetTemplatesByFacilityId @FacilityId = @OrganizationId, @ContextId = @OrganizationId;

	-- Get SutureSign Standard Templates
	INSERT INTO @Templates
	EXEC [$(SutureSignWeb)].dbo.spGetTemplatesByFacilityId @FacilityId = 0, @ContextId = @OrganizationId;

	SELECT t.*
	FROM Template t
		INNER JOIN @Templates result_t ON t.TemplateId = result_t.TemplateId;
END