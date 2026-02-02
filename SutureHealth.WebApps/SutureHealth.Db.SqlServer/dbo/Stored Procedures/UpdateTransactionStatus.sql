CREATE PROCEDURE [dbo].[UpdateTransactionStatus]
	@RequestId int,
	@Status int
AS

BEGIN

	UPDATE [$(SutureSignWeb)].[dbo].[Transactions] SET [Status] = @Status WHERE [RequestId] = @RequestId;

END
