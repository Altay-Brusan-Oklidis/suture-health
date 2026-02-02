CREATE PROCEDURE [dbo].[VerifyUserPassword]
	@MemberId		INT,
	@Password		VARCHAR(50),
	@IsValid		BIT = 0  OUTPUT 
AS
BEGIN
	  EXEC [$(SutureSignWeb)].[dbo].[spVerifyUserPassword] @MemberId = @MemberId,
														   @Password = @Password,
														   @IsValid = @IsValid OUTPUT;
	
END