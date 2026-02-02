CREATE PROCEDURE [dbo].[CreateOrganization]
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
	@CompanyId INT = NULL,
	@CreatedByMemberId INT
AS
BEGIN
	DECLARE @OrganizationId INT,
			@LocationId INT;

	INSERT INTO [$(SutureSignWeb)].dbo.Facilities
	(
		[Name],
		NickName,
		FacilityNPI,
		MedicareNumber,
		FacilityTypeId,
		CompanyId,
		Active,
		DateMod,
		EffectiveDate,
		SubmittedBy,
		UpdatedBy
	)
	VALUES
	(
		@Name,
		@OtherDesignation,
		@NPI,
		@MedicareNumber,
		@OrganizationTypeId,
		@CompanyId,
		1,
		GETDATE(),
		GETDATE(),
		@CreatedByMemberId,
		@CreatedByMemberId
	);

	SET @OrganizationId = SCOPE_IDENTITY();

	UPDATE [$(SutureSignWeb)].dbo.Facilities
	SET FacilityId = @OrganizationId,
		CompanyId = COALESCE(@CompanyId, @OrganizationId)
	WHERE Id = @OrganizationId;

	IF (@Phone IS NOT NULL AND LEN(TRIM(@Phone)) <> 0)
	BEGIN
		INSERT INTO [$(SutureSignWeb)].dbo.Facilities_Contacts (FacilityId, [Type], [Value], Active, [Primary], CreateDate, EffectiveDate, SubmittedBy, DateMod, UpdatedBy)
		VALUES (@OrganizationId, 'Phone', @Phone, 1, 1, GETDATE(), GETDATE(), @CreatedByMemberId, GETDATE(), @CreatedByMemberId);
	END

	IF (@Fax IS NOT NULL AND LEN(TRIM(@Fax)) <> 0)
	BEGIN
		INSERT INTO [$(SutureSignWeb)].dbo.Facilities_Contacts (FacilityId, [Type], [Value], Active, [Primary], CreateDate, EffectiveDate, SubmittedBy, DateMod, UpdatedBy)
		VALUES (@OrganizationId, 'Fax', @Fax, 1, 1, GETDATE(), GETDATE(), @CreatedByMemberId, GETDATE(), @CreatedByMemberId);
	END

	INSERT INTO [$(SutureSignWeb)].dbo.Locations ([Type], Address1, Address2, City, [State], Zip, Country, [Active], DateMod, UpdatedBy, EffectiveDate)
	VALUES ('Practice Location', @AddressLine1, @AddressLine2, @City, @StateOrProvince, @PostalCode, 'USA', 1, GETDATE(), @CreatedByMemberId, GETDATE());

	SET @LocationId = SCOPE_IDENTITY();

	INSERT INTO [$(SutureSignWeb)].dbo.Facilities_Locations (FacilityId, LocationId, [Primary], [Active], CreateDate, EffectiveDate, SubmittedBy)
	VALUES (@OrganizationId, @LocationId, 1, 1, GETDATE(), GETDATE(), @CreatedByMemberId);

	SELECT @OrganizationId AS OrganizationId
END