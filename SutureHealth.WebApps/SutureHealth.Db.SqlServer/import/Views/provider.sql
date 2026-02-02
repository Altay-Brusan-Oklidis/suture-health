CREATE VIEW [import].[Provider]
AS

WITH merged
AS
(
   SELECT NEWID()                                                       [ProviderId]
        , COALESCE(ss.npi, npi.npi)                                     [NPI]
        , COALESCE(ss.ProviderType, npi.ProviderType)                   [ProviderType] 
        , COALESCE(ss.[Name], npi.[Name])                               [Name] 
        , COALESCE(ss.AddressLine1, npi.AddressLine1)                   [AddressLine1] 
        , COALESCE(ss.AddressLine2, npi.AddressLine2)                   [AddressLine2] 
        , COALESCE(ss.City, npi.City)                                   [City] 
        , COALESCE(ss.[State], npi.[State])                             [State] 
        , COALESCE(ss.PostalCode, npi.PostalCode)                       [PostalCode] 
        , COALESCE(ss.PostalCodeArea, npi.PostalCodeArea)               [PostalCodeArea]
        , COALESCE(ss.PostalCodeRoute, npi.zip4)                        [PostalCodeRoute]
        , COALESCE(ss.FirstName, npi.FirstName)                         [FirstName] 
        , COALESCE(ss.MiddleName, npi.MiddleName)                       [MiddleName]
        , COALESCE(ss.LastName, npi.LastName)                           [LastName] 
        , COALESCE(ss.suffix, npi.suffix)                               [suffix] 
        , COALESCE(ss.ProfessionalSuffix, npi.professional_suffix)      [ProfessionalSuffix] 
        , COALESCE(ss.[IsNPIActive], npi.[IsNPIActive])                 [IsNPIActive]
        , COALESCE(ss.[CanSign], 0)                                     [CanSign]
        , COALESCE(ss.[IsActive], 0)                                    [IsActive]
        , COALESCE(ss.[IsCollaborator], 0)                              [IsCollaborator]
        , COALESCE(ss.[IsSigner], 0)                                    [IsSigner]
        , COALESCE(ss.[IsSender], 0)                                    [IsSender]
        , COALESCE(ss.[SutureCustomerType], 0)                          [SutureCustomerType]
        , COALESCE(npi.service_count, 0)                                [ServiceCount]
        , ss.[SutureCreatedAt]
        , ss.[SigningName]                           
        , ss.[SutureFacilityId] 
        , ss.[SutureFacilityTypeId] 
        , ss.[SuturePrimaryFacilityId]
        , ss.[SuturePrimaryFacilityName]
        , ss.[SutureUserId] 
        , ss.[SutureUserTypeId]
        , ss.[SutureCloseDate]

        , CASE db_name()
             WHEN 'SutureSignApi-Prod' THEN COALESCE(ss.Phone, npi.Phone) 
             ELSE '2055551212' 
          END AS [Phone]
        , CASE db_name()
             WHEN 'SutureSignApi-Prod' THEN COALESCE(ss.Fax, npi.Fax) 
             ELSE '2057194210' 
          END AS [Fax]
        , CASE db_name()
             WHEN 'SutureSignApi-Prod' THEN ss.Email
             ELSE 'testuser@suturehealth.com' 
          END  AS [Email]
        , CASE 
             WHEN ss.SutureFacilityId IS NOT NULL OR ss.SutureUserId IS NOT NULL OR claims_provider = 1 OR COALESCE(mc.medicare_claims_count, 0) > 0 THEN 1
             ELSE 0
          END AS [IsNetworkProvider]
        , ss.[UpdatedAt] 

     FROM [import].suture_provider ss
     FULL OUTER JOIN (SELECT npp.*, 
                             nppes.service_count,
                             CASE 
                                 WHEN non_claim_services > 0 THEN 1
                                 ELSE 0
                             END AS claims_provider
                        FROM (SELECT npi, 
                                     COUNT(*) as service_count,
                                     SUM(CASE
                                             WHEN npps.ServiceId BETWEEN 120 AND 121 THEN 1
                                             ELSE 0
                                         END) AS pharmacy_service_count,
                                     SUM(CASE
                                             WHEN s.description IN ('Assisted Living Facility', 'Nursing Home') THEN 1
                                             ELSE 0
                                         END) AS non_claim_services
                                FROM [import].npidata_providerservice npps 
                               INNER JOIN [dbo].service s ON npps.ServiceId = s.ServiceId
                               GROUP BY NPI) nppes
                        LEFT JOIN [import].npidata_provider npp ON npp.npi = nppes.NPI 
                       WHERE nppes.pharmacy_service_count = 0) npi ON ss.npi = npi.npi
    LEFT JOIN (SELECT pmc.[provider_organization_npi] npi, 
                      count(*) medicare_claims_count
                 FROM import.[medicare_claims_by_provider] pmc
                GROUP BY pmc.[provider_organization_npi]) mc ON mc.npi = npi.npi
)
SELECT p.*
     , COALESCE(zs.latitude, zg.Latitude)       [Latitude]
     , COALESCE(zs.longitude, zg.longitude)     [Longitude]
     , null                                     [ProviderPrimaryFacilityId] 
 FROM merged p
 LEFT JOIN merged pp ON p.[SuturePrimaryFacilityId] = pp.[SutureFacilityId]
 LEFT JOIN import.ZipCode zs ON p.PostalCodeArea = zs.PostalCodeArea AND NOT p.PostalCodeRoute is null AND CAST(zs.[BeginingRoute] AS smallint) <= CAST(p.PostalCodeRoute AS smallint) AND CAST(p.PostalCodeRoute AS smallint) <= CAST(zs.[EndingRoute] AS smallint)
 LEFT JOIN import.ZipCode zg ON p.PostalCodeArea = zg.PostalCodeArea AND zg.[BeginingRoute] is null AND zg.[EndingRoute] is null
 WHERE p.SutureCloseDate is null
