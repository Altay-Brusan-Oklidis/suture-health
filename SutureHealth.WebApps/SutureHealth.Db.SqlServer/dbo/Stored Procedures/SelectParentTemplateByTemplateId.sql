CREATE PROCEDURE [dbo].[SelectParentTemplateByTemplateId]
	@TemplateId INT
AS
BEGIN
	-- TODO: Model remaining columns/properties
	SELECT
		pt.TemplateId
	FROM
		[$(SutureSignWeb)].dbo.Templates t
			INNER JOIN [$(SutureSignWeb)].dbo.Templates pt ON t.ParentTemplateId = pt.TemplateId
	WHERE
		t.TemplateId = @TemplateId;
END