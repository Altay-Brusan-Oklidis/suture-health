CREATE PROCEDURE ResetPatientPayerMixFlags 	
	@PatientId    INT , 
	@ChangeBy	  INT
	
AS
BEGIN
		EXEC [$(SutureSignWeb)].[dbo].[spResetPatientPayerMixFlags]
		 @PatientId     = @PatientId,
		 @ChangeBy		= @ChangeBy
END
GO