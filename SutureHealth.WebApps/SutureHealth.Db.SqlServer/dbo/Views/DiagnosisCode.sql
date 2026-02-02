CREATE VIEW [dbo].[DiagnosisCode]
AS 
SELECT [ICD9Code]   Code 
      ,[Descr]      [Description]
      ,[CodeType]
      ,[Id]         DiagnosisCodeId
  FROM [$(SutureSignWeb)].dbo.ICD9Codes

