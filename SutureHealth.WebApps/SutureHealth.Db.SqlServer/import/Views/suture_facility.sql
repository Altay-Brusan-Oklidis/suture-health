CREATE VIEW [import].suture_facility
AS
SELECT 
      TRY_CAST(FacilityNPI as bigint) as suture_facility_npi
      , COALESCE(Name, '') as suture_facility_name
      , COALESCE(Address1, '') as suture_facility_address1
      , COALESCE(Address2, '') as suture_facility_address2
      , COALESCE(City, '') as suture_facility_City
      , COALESCE(State, '') as suture_facility_State
      , COALESCE(Zip, '') as suture_facility_zip
      , COALESCE(Phone, '') as suture_facility_Phone
      , COALESCE(Fax, '') as suture_facility_Fax      
      , COALESCE(DateMod, '') as suture_facility_updated_at
      , COALESCE(CreateDate, '') as suture_facility_created_at
      , COALESCE(FacilityId, '') as suture_facility_id
      , COALESCE(FacilityTypeId, '') as suture_facility_type_id
      , COALESCE(CustomerType, 0) as suture_facility_customer_type
      , CAST(COALESCE(Active, 0) as bit) as suture_facility_is_active
      , CAST(COALESCE(IsFree, 0) as bit) as suture_facility_is_free
      , CAST(COALESCE(IsSender, 0) as bit) as suture_facility_is_sender
      , CAST(COALESCE(IsSigner, 0) as bit) as suture_facility_is_signer
      , CloseDate as suture_facility_close_date
      FROM (SELECT F.FacilityNPI,
                   F.DateMod, F.CreateDate, F.EffectiveDate,
                   F.FacilityId,
                   F.FacilityTypeId,
                   F.Name as 'Name',
                   L.Address1,
                   L.Address2,
                   L.City,
                   L.State,
                   L.Zip,
                   PhoneContact.Value as 'Phone',
                   FaxContact.Value as 'Fax',
                   F.Active,
                   fd.CustomerType,
                   fd.IsSigner,
                   fd.IsSender,
                   fd.IsFree,
                   F.CloseDate
              FROM [$(SutureSignWeb)].dbo.Facilities F WITH (NOLOCK)
              LEFT JOIN [$(SutureSignWeb)].dbo.vw_FacilityDetails fd WITH (NOLOCK) ON fd.FacilityId = F.FacilityId
             OUTER APPLY
                  (
                    SELECT TOP 1 Loc.Address1, Loc.Address2, Loc.City, Loc.State, Loc.Zip 
                    FROM [$(SutureSignWeb)].dbo.Locations Loc WITH (NOLOCK)
                    LEFT OUTER JOIN [$(SutureSignWeb)].dbo.Facilities_Locations FL WITH (NOLOCK) ON FL.FacilityId = F.FacilityId and FL.Active = 1 AND Loc.LocationId = FL.LocationId and Loc.Active = 1
                    WHERE F.FacilityId = FL.FacilityId AND FL.LocationId = Loc.LocationId
                    ORDER BY FL.CreateDate, FL.EffectiveDate
                  ) as L
                  OUTER APPLY
                  (
                    SELECT TOP 1 *
                    FROM [$(SutureSignWeb)].dbo.Facilities_Contacts PhoneContact WITH (NOLOCK)
                    WHERE PhoneContact.FacilityId = F.FacilityId and PhoneContact.[Primary] = 1 and PhoneContact.[Type] = 'Phone' and PhoneContact.Active = 1
                    ORDER BY PhoneContact.CreateDate, PhoneContact.EffectiveDate
                  ) as PhoneContact
                  OUTER APPLY
                  (
                    SELECT TOP 1 *
                    FROM [$(SutureSignWeb)].dbo.Facilities_Contacts FaxContact WITH (NOLOCK)
                    WHERE FaxContact.FacilityId = F.FacilityId and FaxContact.[Primary] = 1 and FaxContact.[Type] = 'Fax' and FaxContact.Active = 1
                    ORDER BY FaxContact.CreateDate, FaxContact.EffectiveDate
                  ) as FaxContact
          WHERE NOT COALESCE(F.FacilityTypeId, 0) IN (10004)
          ) as FOO
