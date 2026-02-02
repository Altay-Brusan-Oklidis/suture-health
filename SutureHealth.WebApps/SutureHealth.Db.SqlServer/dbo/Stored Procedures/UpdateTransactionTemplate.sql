CREATE PROCEDURE [dbo].[UpdateTransactionTemplate]
	@RequestId int,
	@TemplateId int
AS

BEGIN
	
	UPDATE [$(SutureSignWeb)].[dbo].[Transactions]
	SET TemplateId = @TemplateId
	WHERE RequestId = @RequestId;

	UPDATE [$(SutureSignWeb)].[dbo].[Requests]
	SET Template = @TemplateId
	WHERE Id = @RequestId;

END