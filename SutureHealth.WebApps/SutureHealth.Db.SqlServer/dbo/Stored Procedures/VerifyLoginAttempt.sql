CREATE PROCEDURE [dbo].[VerifyLoginAttempt]
	@UserName		varchar(50),
	@Password		varchar(50),
	@IPAddress		varchar(50),
	@URL			varchar(50),
	@UserAgent		varchar(300),
	@ReturnMessage	varchar(500) OUTPUT
AS
BEGIN

	  EXEC [$(SutureSignWeb)].[dbo].[spUserLogin_New] @UserName			= @UserName,
													  @Password			= @Password,
													  @IPAddress		= @IPAddress,
													  @URL				= @URL,
													  @Browser			= @UserAgent,
													  @BrowserVersion	= '--',
													  @ReturnMessage	= @ReturnMessage OUTPUT;

		
END