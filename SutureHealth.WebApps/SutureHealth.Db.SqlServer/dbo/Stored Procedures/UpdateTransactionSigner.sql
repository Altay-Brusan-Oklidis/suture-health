CREATE PROCEDURE [dbo].[UpdateTransactionSigner]
	@Id int,
	@SignerId int,
	@SignerFacilityId int
AS

BEGIN
	UPDATE [$(SutureSignWeb)].[dbo].[Transactions] 
	SET SignerId = @SignerId, SignerFacilityId = @SignerFacilityId
	WHERE Id = @Id;
END
