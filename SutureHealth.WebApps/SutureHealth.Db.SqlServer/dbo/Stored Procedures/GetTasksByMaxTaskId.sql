CREATE PROCEDURE [dbo].[GetTasksByMaxTaskId]
	@MaxTaskId INT,
	@RequestId INT
AS
	
BEGIN	
	SET NOCOUNT ON;	  
	SELECT 
		TaskId,
		EffectiveDate,
		StartOfCare,
		ICD9CodeId
	FROM [$(SutureSignWeb)].[dbo].Tasks
	WHERE
	FormId = @RequestId	
	AND TaskId < @MaxTaskId
	ORDER BY TaskId DESC
END

