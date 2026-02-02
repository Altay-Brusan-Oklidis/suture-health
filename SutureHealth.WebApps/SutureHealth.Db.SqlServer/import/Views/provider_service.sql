CREATE VIEW [import].[ProviderService]
AS

SELECT imp.ServiceId
	  ,p.ProviderId		   
  FROM [import].npidata_providerservice imp
 INNER JOIN [stage].[Provider] p on imp.NPI = p.npi
 UNION ALL
SELECT ps.ServiceId
      ,p.ProviderId
  FROM [import].suture_provider_service ps
 INNER JOIN [stage].[Provider] p ON ps.suturefacilityid = p.suturefacilityid