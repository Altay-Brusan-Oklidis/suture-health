CREATE PROCEDURE [dbo].[UpdateRequestTemplate]
	@RequestId int,
	@TemplateId int
AS

BEGIN

	UPDATE [$(SutureSignWeb)].[dbo].[Requests]
	SET Template = @TemplateId
	WHERE Id = @RequestId;

END
