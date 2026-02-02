CREATE VIEW [dbo].[FhirUsers]
AS
SELECT FhirId [FhirId],
       UserId [UserId]
FROM [$(SutureSignWeb)].dbo.FhirUsers
GO