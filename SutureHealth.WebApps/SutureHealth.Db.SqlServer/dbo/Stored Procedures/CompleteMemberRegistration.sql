CREATE PROCEDURE [dbo].[CompleteMemberRegistration]
	@UserName						nvarchar(50),
	@FirstName						nvarchar(50),
	@LastName						nvarchar(50),
	@Suffix							nvarchar(4),
	@PrimCredential					nvarchar(20),
	@Password						nvarchar(50),
	@SigningName					nvarchar(200),
	@OriginalUserName				nvarchar(50),
	@SubmittedBy					int,
	@PublicId						uniqueidentifier	= null,
	@AssistantTypeId				int					= -1,
	@PreferredEmailAddress			nvarchar(250),
	@OriginalPreferredEmailAddress	nvarchar(350),
	@ErrorMessage					nvarchar(2000)		= ''	OUTPUT
AS
BEGIN
	DECLARE @RC int
	DECLARE @SendWelcomeEmail bit

	EXECUTE @RC = [$(SutureSignWeb)].[dbo].[spUpdateUser] 
		   @userName = @UserName
		  ,@firstName = @FirstName
		  ,@lastName = @LastName
		  ,@suffix = @Suffix
		  ,@primCredential = @PrimCredential
		  ,@password = @Password
		  ,@signingName = @SigningName
		  ,@originalUserName = @OriginalUserName
		  ,@middleName = ''
		  ,@SubmittedBy = @SubmittedBy
		  ,@contactId = -1
		  ,@pid = @PublicId
		  ,@assistantTypeId = @AssistantTypeId
		  ,@preferredEmailAddress = @PreferredEmailAddress
		  ,@originalPreferredEmailAddress = @OriginalPreferredEmailAddress
		  ,@SendWelcomeEmail =  @SendWelcomeEmail OUTPUT
		  ,@ErrorMessage = @ErrorMessage OUTPUT

END