



CREATE VIEW [import].[npidata_providerservice] 
AS

select NPI, s.ServiceId
  from (
select NPI, [Provider], service_code
  from import.[npidata-pfile]
  UNPIVOT(service_code FOR [Provider] in ([Healthcare Provider Taxonomy Code_1]
      ,[Healthcare Provider Taxonomy Code_2]
      ,[Healthcare Provider Taxonomy Code_3]
      ,[Healthcare Provider Taxonomy Code_4]
      ,[Healthcare Provider Taxonomy Code_5]
      ,[Healthcare Provider Taxonomy Code_6]
      ,[Healthcare Provider Taxonomy Code_7]
      ,[Healthcare Provider Taxonomy Code_8]
      ,[Healthcare Provider Taxonomy Code_9]
      ,[Healthcare Provider Taxonomy Code_10]
      ,[Healthcare Provider Taxonomy Code_11]
      ,[Healthcare Provider Taxonomy Code_12]
      ,[Healthcare Provider Taxonomy Code_13]
      ,[Healthcare Provider Taxonomy Code_14]
      ,[Healthcare Provider Taxonomy Code_15])) AS provider_services) ps
 inner join dbo.[Service] s ON ps.service_code = s.code AND s.source = 'NPI'
 
