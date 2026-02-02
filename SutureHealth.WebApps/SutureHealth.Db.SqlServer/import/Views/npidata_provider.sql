
CREATE VIEW [import].[npidata_provider] 
AS
SELECT NPI AS npi, 
	    CASE 
		  WHEN [NPI Deactivation Date] IS NULL THEN 1
		  WHEN NOT [NPI Deactivation Date] IS NULL AND NOT [NPI Reactivation Date] IS NULL THEN 
				CASE 
					WHEN [NPI Reactivation Date] >= [NPI Deactivation Date] THEN 1
					ELSE 0
				END
		  ELSE 0
	    END AS [IsNPIActive],
	    CASE
				WHEN [Entity Type Code] = 1 THEN
					CASE
						WHEN [Provider Other Last Name Type Code] = 2 THEN CONCAT([Provider Other Last Name], ',', [Provider Other First Name], ' ', [Provider Other Middle Name])
						ELSE CONCAT([Provider Last Name (Legal Name)], ', ', [Provider First Name], ' ', [Provider Middle Name])
					END
				WHEN [Entity Type Code] = 2 THEN
					CASE 
						WHEN [Provider Other Organization Name Type Code] = 3 THEN [Provider Other Organization Name]
						ELSE [Provider Organization Name (Legal Business Name)]
					END
		 END AS [Name],
	    CASE 
				WHEN [Provider Other Last Name Type Code] = 2 THEN [Provider Other Last Name]
				ELSE [Provider Last Name (Legal Name)]
	    END AS LastName,
	    CASE 
				WHEN [Provider Other Last Name Type Code] = 2 THEN [Provider Other First Name]
				ELSE [Provider First Name]
		 END AS FirstName,
	    CASE 
				WHEN [Provider Other Last Name Type Code] = 2 THEN [Provider Other Middle Name]
				ELSE [Provider Middle Name]
	    END AS MiddleName,
	    CASE 
				WHEN [Provider Other Last Name Type Code] = 2 THEN [Provider Other Name Suffix Text]
				ELSE [Provider Name Suffix Text]
	    END AS suffix,
	    CASE 
				WHEN [Provider Other Last Name Type Code] = 2 THEN [Provider Other Credential Text]
				ELSE [Provider Credential Text]
	    END					  AS professional_suffix,
	    [Entity Type Code] AS ProviderType, 
	    [Last Update Date] AS updatedat,
	    [Provider First Line Business Practice Location Address]							AS AddressLine1,
	    [Provider Second Line Business Practice Location Address]							AS AddressLine2,
	    [Provider Business Practice Location Address City Name]								AS City,
	    [Provider Business Practice Location Address State Name]							AS [State],
	    [Provider Business Practice Location Address Postal Code]							AS PostalCode,
	    SUBSTRING([Provider Business Practice Location Address Postal Code], 1, 5)	AS PostalCodeArea,
	    CASE
			WHEN LEN([Provider Business Practice Location Address Postal Code]) = 9 THEN SUBSTRING([Provider Business Practice Location Address Postal Code], 6, 4)
			ELSE NULL
	    END AS zip4,
	    COALESCE([Provider Business Practice Location Address TelePhone Number], [Provider Business Mailing Address TelePhone Number])	AS Phone,
	    COALESCE([Provider Business Practice Location Address Fax Number], [Provider Business Mailing Address Fax Number])					AS Fax
  FROM [import].[npidata-pfile] n
 WHERE NOT [Entity Type Code] IS NULL