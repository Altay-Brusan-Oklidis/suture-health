CREATE VIEW [import].[suture_provider]
AS
select suture_facility_npi                      [NPI]
	  ,0                                        [IsNPIActive]
      ,2                                        [ProviderType]
      ,suture_facility_updated_at               [UpdatedAt]
      ,suture_facility_name                     [Name]
      ,null                                     [LastName]
      ,null                                     [FirstName]
      ,null                                     [MiddleName]
      ,null                                     [Suffix]
      ,null                                     [ProfessionalSuffix]
      ,suture_facility_address1                 [AddressLine1]
      ,suture_facility_address2                 [AddressLine2]
      ,suture_facility_City                     [City]
      ,suture_facility_State                    [State]
      ,suture_facility_zip                      [PostalCode]
      ,SUBSTRING(suture_facility_zip, 1, 5)     [PostalCodeArea]
      ,SUBSTRING(suture_facility_zip, 6, 4)     [PostalCodeRoute]
      ,suture_facility_Phone                    [Phone]
      ,suture_facility_Fax                      [Fax]
      ,null                                     [Email]
      ,suture_facility_close_date               [SutureCloseDate]
	  ,suture_facility_created_at               [SutureCreatedAt] 
	  ,suture_facility_customer_type            [SutureCustomerType] 
	  ,suture_facility_id                       [SutureFacilityId] 
	  ,suture_facility_type_id                  [SutureFacilityTypeId] 
	  ,NULL                                     [SutureUserId] 
	  ,NULL                                     [SutureUserTypeId] 
	  ,NULL                                     [SigningName]
	  ,NULL                                     [CanSign]
	  ,suture_facility_is_active                [IsActive] 
	  ,NULL                                     [IsCollaborator]
	  ,suture_facility_is_signer                [IsSigner] 
	  ,suture_facility_is_sender                [IsSender] 
	  ,NULL                                     [SuturePrimaryFacilityId] 
      ,NULL                                     [SuturePrimaryFacilityName]
  from [import].suture_facility
 union all
select suture_user_npi
	  , 0 
      , 1 
      , suture_user_updated_at 
      , suture_user_full_name 
      , suture_user_LastName 
      , suture_user_FirstName 
      , suture_user_MiddleName
      , suture_user_suffix 
      , suture_user_professional_suffix 
      , suture_user_address1 
      , suture_user_address2 
      , suture_user_City 
      , suture_user_State 
      , suture_user_zip 
      , SUBSTRING(suture_user_zip, 1, 5) 
      , SUBSTRING(suture_user_zip, 6, 4) 
      , suture_user_Phone 
      , suture_user_Fax 
      , suture_user_Email 
      , null
	  , suture_user_created_at 
	  , suture_user_customer_type 
	  , NULL 
	  , NULL 
	  , suture_user_id 
	  , suture_user_type_id 
	  , suture_user_signing_name 
	  , suture_user_can_sign 
	  , suture_user_is_active 
	  , suture_user_is_collaborator 
	  , suture_user_is_signer 
	  , suture_user_is_sender 
	  , suture_user_primary_facility_id 
      , suture_user_primary_facility_name 
  from [import].suture_user
