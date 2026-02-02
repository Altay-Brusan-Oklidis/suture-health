CREATE PROCEDURE [dbo].[UpdateOrganization]
	@OrganizationId INT,
	@Name VARCHAR(250) = NULL,
	@OtherDesignation VARCHAR(250) = NULL,
	@AddressLine1 VARCHAR(150) = NULL,
	@AddressLine2 VARCHAR(150) = NULL,
	@City VARCHAR(150) = NULL,
	@StateOrProvince VARCHAR(2) = NULL,
	@PostalCode VARCHAR(9) = NULL,
	@Phone VARCHAR(250) = NULL,
	@Fax VARCHAR(250) = NULL,
	@NPI VARCHAR(10) = NULL,
	@MedicareNumber VARCHAR(10) = NULL,
	@OrganizationTypeId INT = NULL,
	@SetCompanyId BIT = NULL,
	@CompanyId INT = NULL,
	@SetDateClosed BIT = NULL,
	@DateClosed DATETIME = NULL,
	@UpdatedByMemberId INT
AS
BEGIN
	DECLARE @AddressUpdated BIT,
			@PhoneUpdated BIT,
			@FaxUpdated BIT;

	SELECT
		@Name = COALESCE(@Name, f.[Name]),
		@OtherDesignation = COALESCE(@OtherDesignation, f.NickName),
		@AddressLine1 = COALESCE(@AddressLine1, l.Address1),
		@AddressLine2 = COALESCE(@AddressLine2, l.Address2),
		@City = COALESCE(@City, l.City),
		@StateOrProvince = COALESCE(@StateOrProvince, l.[State]),
		@PostalCode = COALESCE(@PostalCode, l.Zip),
		@Phone = COALESCE(@Phone, fcp.[Value]),
		@Fax = COALESCE(@Fax, fcf.[Value]),
		@NPI = COALESCE(@NPI, f.FacilityNPI),
		@MedicareNumber = COALESCE(@MedicareNumber, f.MedicareNumber),
		@OrganizationTypeId = COALESCE(@OrganizationTypeId, f.FacilityTypeId),
		@AddressUpdated = CASE WHEN @AddressLine1 IS NOT NULL OR @AddressLine2 IS NOT NULL OR @City IS NOT NULL OR @StateOrProvince IS NOT NULL OR @PostalCode IS NOT NULL THEN 1 ELSE 0 END,
		@PhoneUpdated = CASE WHEN @Phone IS NOT NULL THEN 1 ELSE 0 END,
		@FaxUpdated = CASE WHEN @Fax IS NOT NULL THEN 1 ELSE 0 END,
		@SetCompanyId = COALESCE(@SetCompanyId, 0),
		@SetDateClosed = COALESCE(@SetDateClosed, 0)
	FROM
		[$(SutureSignWeb)].dbo.Facilities f
			LEFT JOIN [$(SutureSignWeb)].dbo.Facilities_Contacts fcp ON f.FacilityId = fcp.FacilityId AND fcp.[Primary] = 1 AND fcp.[Active] = 1 AND fcp.[Type] = 'Phone'
			LEFT JOIN [$(SutureSignWeb)].dbo.Facilities_Contacts fcf ON f.FacilityId = fcf.FacilityId AND fcf.[Primary] = 1 AND fcp.[Active] = 1 AND fcf.[Type] = 'Fax'
			LEFT JOIN [$(SutureSignWeb)].dbo.Facilities_Locations fl ON f.FacilityId = fl.FacilityId AND fl.[Primary] = 1 AND fl.Active = 1
			LEFT JOIN [$(SutureSignWeb)].dbo.Locations l ON fl.LocationId = l.LocationId AND l.Active = 1
	WHERE
		f.FacilityId = @OrganizationId;

	UPDATE f
	SET f.[Name] = @Name,
		f.NickName = @OtherDesignation,
		f.FacilityNPI = @NPI,
		f.MedicareNumber = @MedicareNumber,
		f.UpdatedBy = @UpdatedByMemberId,
		f.FacilityTypeId = @OrganizationTypeId,
		f.CompanyId = CASE WHEN @SetCompanyId = 1 THEN COALESCE(@CompanyId, f.FacilityId) ELSE COALESCE(f.CompanyId, f.FacilityId) END,
		f.CloseDate = CASE WHEN @SetDateClosed = 1 THEN @DateClosed ELSE f.CloseDate END,
		f.DateMod = GETDATE(),
		f.EffectiveDate = GETDATE()
	FROM [$(SutureSignWeb)].dbo.Facilities f
	WHERE f.FacilityId = @OrganizationId;

	IF (@PhoneUpdated = 1)
	BEGIN
		IF EXISTS (SELECT 0 FROM [$(SutureSignWeb)].dbo.Facilities_Contacts WHERE FacilityId = @OrganizationId AND [Primary] = 1 AND [Active] = 1 AND [Type] = 'Phone')
		BEGIN
			UPDATE [$(SutureSignWeb)].dbo.Facilities_Contacts
			SET [Value] = @Phone,
				DateMod = GETDATE(),
				EffectiveDate = GETDATE(),
				UpdatedBy = @UpdatedByMemberId
			WHERE FacilityId = @OrganizationId AND [Primary] = 1 AND [Active] = 1 AND [Type] = 'Phone'
		END
		ELSE
		BEGIN
			INSERT INTO [$(SutureSignWeb)].dbo.Facilities_Contacts (FacilityId, [Type], [Value], Active, [Primary], CreateDate, EffectiveDate, SubmittedBy, DateMod, UpdatedBy)
			VALUES (@OrganizationId, 'Phone', @Phone, 1, 1, GETDATE(), GETDATE(), @UpdatedByMemberId, GETDATE(), @UpdatedByMemberId);
		END
	END

	IF (@FaxUpdated = 1)
	BEGIN
		IF EXISTS (SELECT 0 FROM [$(SutureSignWeb)].dbo.Facilities_Contacts WHERE FacilityId = @OrganizationId AND [Primary] = 1 AND [Active] = 1 AND [Type] = 'Fax')
		BEGIN
			UPDATE [$(SutureSignWeb)].dbo.Facilities_Contacts
			SET [Value] = @Fax,
				DateMod = GETDATE(),
				EffectiveDate = GETDATE(),
				UpdatedBy = @UpdatedByMemberId
			WHERE FacilityId = @OrganizationId AND [Primary] = 1 AND [Active] = 1 AND [Type] = 'Fax'
		END
		ELSE
		BEGIN
			INSERT INTO [$(SutureSignWeb)].dbo.Facilities_Contacts (FacilityId, [Type], [Value], Active, [Primary], CreateDate, EffectiveDate, SubmittedBy, DateMod, UpdatedBy)
			VALUES (@OrganizationId, 'Fax', @Fax, 1, 1, GETDATE(), GETDATE(), @UpdatedByMemberId, GETDATE(), @UpdatedByMemberId);
		END
	END

	IF (@AddressUpdated = 1)
	BEGIN
		IF EXISTS (SELECT 0 FROM [$(SutureSignWeb)].dbo.Facilities_Locations fl INNER JOIN [$(SutureSignWeb)].dbo.Locations l ON fl.LocationId = l.LocationId WHERE fl.FacilityId = @OrganizationId)
		BEGIN
			UPDATE [$(SutureSignWeb)].dbo.Facilities_Locations
			SET [Primary] = 1,
				Active = 1,
				EffectiveDate = GETDATE()
			WHERE FacilityId = @OrganizationId;

			UPDATE l
			SET Address1 = @AddressLine1,
				Address2 = @AddressLine2,
				City = @City,
				[State] = @StateOrProvince,
				Zip = @PostalCode,
				Country = 'USA',
				DateMod = GETDATE(),
				UpdatedBy = @UpdatedByMemberId,
				EffectiveDate = GETDATE(),
				Active = 1
			FROM
				[$(SutureSignWeb)].dbo.Locations l
					INNER JOIN [$(SutureSignWeb)].dbo.Facilities_Locations fl ON l.LocationId = fl.LocationId
			WHERE
				fl.FacilityId = @OrganizationId;
		END
		ELSE
		BEGIN
			DECLARE @LocationId INT;

			DELETE FROM [$(SutureSignWeb)].dbo.Facilities_Locations
			WHERE FacilityId = @OrganizationId;

			INSERT INTO [$(SutureSignWeb)].dbo.Locations ([Type], Address1, Address2, City, [State], Zip, Country, [Active], DateMod, UpdatedBy, EffectiveDate)
			VALUES ('Practice Location', @AddressLine1, @AddressLine2, @City, @StateOrProvince, @PostalCode, 'USA', 1, GETDATE(), @UpdatedByMemberId, GETDATE());

			SET @LocationId = SCOPE_IDENTITY();

			INSERT INTO [$(SutureSignWeb)].dbo.Facilities_Locations (FacilityId, LocationId, [Primary], [Active], CreateDate, EffectiveDate, SubmittedBy)
			VALUES (@OrganizationId, @LocationId, 1, 1, GETDATE(), GETDATE(), @UpdatedByMemberId);
		END
	END
END