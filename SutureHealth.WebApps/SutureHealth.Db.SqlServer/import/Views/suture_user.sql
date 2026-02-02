CREATE VIEW [import].suture_user 
AS
SELECT
       UserId as suture_user_id
	 , COALESCE(UserTypeId, '') as suture_user_type_id
	 , CAST(COALESCE(IsCollaborator, 0) AS BIT) as suture_user_is_collaborator
	 , CAST(COALESCE(CanSign, 0) AS BIT) as suture_user_can_sign
	 , CAST(COALESCE(IsSigner, 1) AS BIT) as suture_user_is_signer
	 , CAST(COALESCE(IsSender, 0) AS BIT) as suture_user_is_sender
     , CAST(COALESCE(Active, '') AS BIT) as suture_user_is_active
	 , CAST(COALESCE(IsFree, 0) as bit) as suture_user_is_free
     , COALESCE(CustomerType, 0) as suture_user_customer_type
     , COALESCE(CreatedDate, '') as suture_user_created_at
     , COALESCE(UpdatedDate, '') as suture_user_updated_at
	 , FacilityId as suture_user_primary_facility_id
     , COALESCE(FacilityName, '') as suture_user_primary_facility_name
     , TRY_CAST(UserNPI as bigint) as suture_user_npi
     , COALESCE(FirstName, '') as suture_user_FirstName
     , COALESCE(MiddleName, '') as suture_user_MiddleName
     , COALESCE(LastName, '') as suture_user_LastName
     , COALESCE(Suffix, '') as suture_user_suffix
     , COALESCE(PrimCredential, '') as suture_user_professional_suffix
     , COALESCE(LastName, '') + ', ' + COALESCE(FirstName, '') + ' ' + COALESCE(MiddleName, '') as suture_user_full_name
     , COALESCE(SigningName, '') as suture_user_signing_name
     , COALESCE(Address1, '') as suture_user_address1
	 , COALESCE(Address2, '') as suture_user_address2
     , COALESCE(City, '') as suture_user_City
     , COALESCE([State], '') as suture_user_State
	 , COALESCE(Zip, '') as suture_user_zip
	 , COALESCE(Phone, '') as suture_user_Phone
     , COALESCE(Fax, '') as suture_user_Fax
     , COALESCE(Email, '') as suture_user_Email
	   FROM
	    (
	    SELECT
	    	U.CreatedDate, 
			U.UpdatedDate, 
			U.UserId, 
			U.UserTypeId, 
			U.IsCollaborator,
			U.CanSign,
			U.Active,
			1 as 'IsSigner',
			0 as 'IsSender',
			F.FacilityId,
			pf.Name as FacilityName,
			F.IsFree,
			F.CustomerType,
			U.UserNPI, 
			U.FirstName, 
			U.MiddleName,
	    	U.LastName, 
			U.Suffix, 
			U.PrimCredential, 
			U.SigningName, 
			L.Address1, 
			L.Address2, 
			L.City, 
			L.State, 
			L.Zip,
	    	PhoneContact.Value  as 'Phone',
	    	FaxContact.Value    as 'Fax',
	    	EmailContact.Value  as 'Email'
	    FROM [$(SutureSignWeb)].dbo.Users U WITH (NOLOCK) 
	    LEFT JOIN [$(SutureSignWeb)].dbo.Users_Facilities UF WITH (NOLOCK) ON U.UserId = UF.UserId AND UF.[Primary] = 1 AND UF.Active = 1
	    LEFT JOIN [$(SutureSignWeb)].dbo.Facilities pf WITH (NOLOCK) ON UF.FacilityId = pf.FacilityId
	    LEFT JOIN [$(SutureSignWeb)].dbo.vw_FacilityDetails F WITH (NOLOCK) ON UF.FacilityId = F.FacilityId
	    OUTER APPLY 
	    (
	    	SELECT TOP 1 Loc.Address1, Loc.Address2, Loc.City, Loc.State, Loc.Zip
	    	  FROM [$(SutureSignWeb)].dbo.Locations Loc WITH (NOLOCK)
	    	  LEFT OUTER JOIN [$(SutureSignWeb)].dbo.Facilities_Locations FL WITH (NOLOCK) ON F.FacilityId = FL.FacilityId AND FL.[Primary] = 1
	    	 WHERE F.FacilityId = FL.FacilityId AND FL.LocationId = Loc.LocationId
	    ) as L
	    OUTER APPLY
	    (
	    	SELECT TOP 1 *
	    	  FROM [$(SutureSignWeb)].dbo.Users_Contacts PhoneContact WITH (NOLOCK)
	    	 WHERE PhoneContact.UserId = U.UserId AND PhoneContact.[Primary] = 1 AND PhoneContact.[Type] = 'Phone' AND PhoneContact.Active = 1
	    	 ORDER BY PhoneContact.CreateDate, PhoneContact.EffectiveDate
	    ) as PhoneContact
	    OUTER APPLY
	    (
	    	SELECT TOP 1 *
	    	  FROM [$(SutureSignWeb)].dbo.Users_Contacts FaxContact WITH (NOLOCK)
	    	 WHERE FaxContact.UserId = U.UserId AND FaxContact.[Primary] = 1 AND FaxContact.[Type] = 'Fax' AND FaxContact.Active = 1
	    	 ORDER BY FaxContact.CreateDate, FaxContact.EffectiveDate
	    ) as FaxContact
	    OUTER APPLY
	    (
	    	SELECT TOP 1 *
	    	  FROM [$(SutureSignWeb)].dbo.Users_Contacts EmailContact WITH (NOLOCK)
	    	 WHERE EmailContact.UserId = U.UserId AND EmailContact.[Primary] = 1 AND EmailContact.[Type] = 'Email' AND EmailContact.Active = 1
	    	 ORDER BY EmailContact.CreateDate, EmailContact.EffectiveDate
	    ) as EmailContact
	    WHERE u.UserTypeId IN (2000, 2001, 2008) 
		   OR u.CANSIGN = 1
    ) AS FOO