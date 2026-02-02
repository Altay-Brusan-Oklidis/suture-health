CREATE VIEW [import].suture_facility_service
AS
SELECT s.ServiceId,
       sfs.FacilityId
  FROM [$(SutureSignWeb)].dbo.[FacilitySettings] sfs WITH (NOLOCK) 
 INNER JOIN dbo.service s WITH (NOLOCK) ON sfs.ItemVarChar = s.code
 WHERE Setting = 'Services'
   AND Active = 1