CREATE VIEW [dbo].[PatientMatchOutcome]
AS 
	SELECT [MatchPatientOutcomeID]
          ,[MatchPatientLogID]
          ,[PatientID]
          ,[MatchScore]
          ,[MatchRejected]
          ,[PatientCreated]
          ,[CreateDate] 
	  FROM [$(SutureSignWeb)].[dbo].MatchPatientOutcome
