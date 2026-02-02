CREATE VIEW [import].[suture_provider_service]
AS
SELECT s.ServiceId,
       sfs.FacilityId   [SutureFacilityId]
  FROM [$(SutureSignWeb)].dbo.[FacilitySettings] sfs WITH (NOLOCK)
 INNER JOIN dbo.service s ON sfs.ItemVarChar = s.code
 WHERE Setting = 'Services' AND Active = 1
