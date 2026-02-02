CREATE PROCEDURE [dbo].[UpsertMember]
	@MemberId				INT					= NULL OUTPUT,

	@FirstName				VARCHAR(50)			= NULL,
	@LastName				VARCHAR(50)			= NULL,
	@Suffix					VARCHAR(5)			= NULL,
	@ProfessionalSuffix		VARCHAR(20)			= NULL,
	@Email					VARCHAR(250)		= NULL,
	@MobilePhone			VARCHAR(250)		= NULL,
	@OfficePhone			VARCHAR(250)		= NULL,
	@OfficePhoneExtension	VARCHAR(250)		= NULL,
	@UserName				VARCHAR(50)			= NULL,
	@MemberTypeId			INT					= NULL,
	@Npi					VARCHAR(10)			= NULL,
	@SigningName			VARCHAR(200)		= NULL,
	@CanSign				BIT					= NULL,

	@AccessFailedCount		INT 		 		= NULL,
	@EmailConfirmed			BIT 		 		= NULL,
	@LastLoggedInAt			DATETIMEOFFSET(7)	= '1900-01-01 00:00:00.0000000 +00:00',
	@LockoutEnd			    DATETIMEOFFSET(7)	= '1900-01-01 00:00:00.0000000 +00:00',
	@MobileNumberConfirmed	BIT 		 		= NULL,
	@MustChangePassword		BIT 		 		= NULL,
	@MustRegisterAccount	BIT 		 		= NULL,
	@PasswordHash			VARCHAR(MAX) 		= NULL,
	@SecurityStamp			VARCHAR(MAX)		= NULL,
	
	@RelatedMemberIds		IntegerKey					READONLY,
	@OrganizationMembers	OrganizationMemberUpsert	READONLY,
	
	@UpdatedByMemberId		INT
AS
BEGIN
	DECLARE @IsNew BIT,
			@UserActive BIT = 0,
			@IsCollaborator BIT,
			@OriginalMemberTypeId INT = NULL,
			@DefaultOrganizationId INT;
	DECLARE @OrganizationIds TABLE (OrganizationId INT NOT NULL);

	INSERT INTO @OrganizationIds
	SELECT DISTINCT OrganizationId
	  FROM (SELECT FacilityId AS OrganizationId FROM [$(SutureSignWeb)].dbo.Users_Facilities WHERE UserId = @MemberId
			 UNION ALL
			SELECT OrganizationId FROM @OrganizationMembers) joined;

	SET @IsNew = CASE WHEN @MemberId IS NULL THEN 1 ELSE 0 END;
	IF (@IsNew = 0)
		BEGIN
			SELECT @OriginalMemberTypeId = UserTypeId
			  FROM [$(SutureSignWeb)].dbo.Users
			 WHERE UserId = @MemberId;
		END

	SET @OriginalMemberTypeId = COALESCE(@OriginalMemberTypeId, @MemberTypeId);
	SET @IsCollaborator = CASE WHEN COALESCE(@MemberTypeId, @OriginalMemberTypeId) IN (2001, 2008) THEN 1 ELSE 0 END;

	SELECT @UserActive = 1
	  FROM @OrganizationIds orgs LEFT JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON orgs.OrganizationId = uf.FacilityId AND uf.UserId = @MemberId
	  LEFT JOIN @OrganizationMembers om ON orgs.OrganizationId = om.OrganizationId
	 WHERE om.IsActive = 1 OR (om.OrganizationId IS NULL AND uf.Active = 1);

	SELECT TOP 1 @DefaultOrganizationId = COALESCE(om.OrganizationId, uf.FacilityId)
	  FROM @OrganizationIds orgs LEFT JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON orgs.OrganizationId = uf.FacilityId AND uf.UserId = @MemberId
	  LEFT JOIN @OrganizationMembers om ON orgs.OrganizationId = om.OrganizationId
	 WHERE om.IsActive = 1 OR (om.OrganizationId IS NULL AND uf.Active = 1)
	 ORDER BY CASE WHEN om.IsPrimary = 1 THEN 2 
				   ELSE uf.[Primary] 
			  END DESC;

	IF (@IsNew = 1)
	BEGIN
		INSERT INTO [$(SutureSignWeb)].dbo.Users
		(
			UserName,
			UserTypeId,
			FirstName,
			LastName,
			Suffix,
			PrimCredential,
			SigningName,
			UserNPI,
			Active,
			MustRegisterAccount,
			MustChangePassword,
			EulaEffDate,
			ResetPwd,
			TempPassword,
			CreatedDate,
			UpdatedDate,
			CreatedBy,
			SubmittedBy,
			CanSign,
			IsCollaborator
		)
		VALUES
		(
			@UserName,
			@MemberTypeId,
			@FirstName,
			@LastName,
			@Suffix,
			@ProfessionalSuffix,
			@SigningName,
			CASE WHEN TRIM(@Npi) = '' THEN NULL ELSE @Npi END,
			@UserActive,
			1,
			1,
			GETDATE(),
			0,
			NULL,
			GETDATE(),
			GETDATE(),
			@UpdatedByMemberId,
			@UpdatedByMemberId,
			CASE WHEN COALESCE(@MemberTypeId, @OriginalMemberTypeId) = 2000 THEN 1 ELSE COALESCE(@CanSign, 0) END,
			@IsCollaborator
		);

		SET @MemberId = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE [$(SutureSignWeb)].dbo.Users
		SET UserName = COALESCE(@UserName, UserName),
			UserTypeId = COALESCE(@MemberTypeId, UserTypeId),
			FirstName = COALESCE(@FirstName, FirstName),
			LastName = COALESCE(@LastName, LastName),
			Suffix = COALESCE(@Suffix, Suffix),
			PrimCredential = COALESCE(@ProfessionalSuffix, PrimCredential),
			SigningName = COALESCE(@SigningName, SigningName),
			UserNPI = CASE WHEN TRIM(@Npi) = '' THEN NULL ELSE COALESCE(@Npi, UserNPI) END,
			Active = @UserActive,
			ActiveUpdate = CASE WHEN Active != @UserActive THEN GETDATE() ELSE ActiveUpdate END,
			UpdatedDate = GETDATE(),
			SubmittedBy = @UpdatedByMemberId,
			CanSign = CASE WHEN COALESCE(@MemberTypeId, @OriginalMemberTypeId) = 2000 THEN 1 ELSE COALESCE(@CanSign, CanSign, 0) END,
			IsCollaborator = @IsCollaborator,

			AccessFailedCount = COALESCE(@AccessFailedCount, AccessFailedCount),
			LastLoggedInAt = CASE
								WHEN @LastLoggedInAt = '1900-01-01 00:00:00.0000000 +00:00' THEN LastLoggedInAt
								ELSE @LastLoggedInAt
							 END,
			LockoutEnd = CASE
							WHEN @LockoutEnd = '1900-01-01 00:00:00.0000000 +00:00' THEN LockoutEnd
							ELSE @LockoutEnd
						 END,
			MustChangePassword = COALESCE(@MustChangePassword, MustChangePassword),
			MustRegisterAccount = COALESCE(@MustRegisterAccount, MustRegisterAccount),
			EulaEffDate = CASE 
							WHEN @MustRegisterAccount = 1 THEN GETUTCDATE()
							ELSE EulaEffDate
						  END,
			PasswordHash = COALESCE(@PasswordHash, PasswordHash),
			SecurityStamp = COALESCE(@SecurityStamp, SecurityStamp)
		WHERE UserId = @MemberId;
	END

	--IF (@TemporaryPassword IS NOT NULL)
	--BEGIN
	--	EXEC [$(SutureSignWeb)].dbo.spSetUserTemporaryPassword
	--		@UserId = @MemberId,
	--		@Password = @TemporaryPassword;
	--END

	IF (@Email IS NOT NULL)
	BEGIN
		IF EXISTS (SELECT 0 FROM [$(SutureSignWeb)].dbo.Users_Contacts WHERE UserId = @MemberId AND [Primary] = 1 AND [Active] = 1 AND [Type] = 'Email')
		BEGIN
			UPDATE [$(SutureSignWeb)].dbo.Users_Contacts
			SET [Value] = @Email,
				EffectiveDate = GETDATE(),
				Confirmed = CASE 
								WHEN @Email <> [Value] THEN COALESCE(@EmailConfirmed, 0)
								ELSE COALESCE(@EmailConfirmed, Confirmed)
							END,
				SubmittedBy = @UpdatedByMemberId
			WHERE UserId = @MemberId AND [Primary] = 1 AND [Active] = 1 AND [Type] = 'Email'
		END
		ELSE IF (TRIM(@Email) != '')
		BEGIN
			INSERT INTO [$(SutureSignWeb)].dbo.Users_Contacts (UserId, [Type], [Value], Active, [Primary], CreateDate, EffectiveDate, SubmittedBy)
			VALUES (@MemberId, 'Email', @Email, 1, 1, GETDATE(), GETDATE(), @UpdatedByMemberId);
		END
	END

	IF (@MobilePhone IS NOT NULL)
	BEGIN
		IF EXISTS (SELECT 0 FROM [$(SutureSignWeb)].dbo.Users_Contacts WHERE UserId = @MemberId AND [Active] = 1 AND [Type] = 'Mobile')
		BEGIN
			UPDATE [$(SutureSignWeb)].dbo.Users_Contacts
			SET [Value] = @MobilePhone,
				[Primary] = 1,
				Confirmed = CASE 
								WHEN @MobilePhone <> [Value] THEN COALESCE(@MobileNumberConfirmed, 0)
								ELSE COALESCE(@MobileNumberConfirmed, Confirmed)
							END,
				EffectiveDate = GETDATE(),
				SubmittedBy = @UpdatedByMemberId
			WHERE UserId = @MemberId AND [Active] = 1 AND [Type] = 'Mobile'
		END
		ELSE IF (TRIM(@MobilePhone) != '')
		BEGIN
			INSERT INTO [$(SutureSignWeb)].dbo.Users_Contacts (UserId, [Type], [Value], Active, [Primary], CreateDate, EffectiveDate, SubmittedBy)
			VALUES (@MemberId, 'Mobile', @MobilePhone, 1, 1, GETDATE(), GETDATE(), @UpdatedByMemberId);
		END
	END

	IF (@OfficePhone IS NOT NULL)
	BEGIN
		IF EXISTS (SELECT 0 FROM [$(SutureSignWeb)].dbo.Users_Contacts WHERE UserId = @MemberId AND [Active] = 1 AND [Type] = 'OfficePhone')
		BEGIN
			UPDATE [$(SutureSignWeb)].dbo.Users_Contacts
			SET [Value] = @OfficePhone,
				[Primary] = 1,
				EffectiveDate = GETDATE(),
				SubmittedBy = @UpdatedByMemberId
			WHERE UserId = @MemberId AND [Active] = 1 AND [Type] = 'OfficePhone'
		END
		ELSE IF (TRIM(@OfficePhone) != '')
		BEGIN
			INSERT INTO [$(SutureSignWeb)].dbo.Users_Contacts (UserId, [Type], [Value], Active, [Primary], CreateDate, EffectiveDate, SubmittedBy)
			VALUES (@MemberId, 'OfficePhone', @OfficePhone, 1, 1, GETDATE(), GETDATE(), @UpdatedByMemberId);
		END

		IF EXISTS (SELECT 0 FROM [$(SutureSignWeb)].dbo.Users_Contacts WHERE UserId = @MemberId AND [Active] = 1 AND [Type] = 'OfficePhoneExt')
		BEGIN
			UPDATE [$(SutureSignWeb)].dbo.Users_Contacts
			SET [Value] = COALESCE(@OfficePhoneExtension, ''),
				[Primary] = 1,
				EffectiveDate = GETDATE(),
				SubmittedBy = @UpdatedByMemberId
			WHERE UserId = @MemberId AND [Active] = 1 AND [Type] = 'OfficePhoneExt'
		END
		ELSE IF (TRIM(@OfficePhoneExtension) != '')
		BEGIN
			INSERT INTO [$(SutureSignWeb)].dbo.Users_Contacts (UserId, [Type], [Value], Active, [Primary], CreateDate, EffectiveDate, SubmittedBy)
			VALUES (@MemberId, 'OfficePhoneExt', COALESCE(@OfficePhoneExtension, ''), 1, 1, GETDATE(), GETDATE(), @UpdatedByMemberId);
		END
	END

	--IF (@TemporaryPassword IS NOT NULL AND @UserActive = 1)
	--BEGIN
	--	DECLARE @SutureSignUrl VARCHAR(200);

	--	SELECT
	--		@UserName = u.UserName,
	--		@Email = uc.[Value],
	--		@SutureSignUrl = CASE DB_NAME()
	--							 WHEN 'SutureSignApi-CI' THEN 'https://ci.suturesign.com'
	--							 WHEN 'SutureSignApi-QA' THEN 'https://qa.suturesign.com'
	--							 WHEN 'SutureSignApi-Demo' THEN 'https://demo.suturesign.com'
	--							 WHEN 'SutureSignApi-Stage' THEN 'https://stage.suturesign.com'
	--							 ELSE 'https://secure.suturesign.com'
	--						 END
	--	FROM [$(SutureSignWeb)].dbo.Users u WITH (NOLOCK)
	--		LEFT JOIN [$(SutureSignWeb)].dbo.Users_Contacts uc WITH (NOLOCK) ON u.UserId = uc.UserId AND uc.Active = 1 AND uc.[Type] = 'Email'
	--	WHERE u.UserId = @MemberId;

	--	IF (@Email IS NOT NULL)
	--	BEGIN
	--		IF (@IsNew = 1)
	--		BEGIN
	--			DECLARE @WelcomeEmailType VARCHAR(100),
	--					@Output BIT;

	--			SET @WelcomeEmailType = CASE COALESCE(@MemberTypeId, @OriginalMemberTypeId)
	--										WHEN 2000 THEN 'NewSignerWelcomeEmail'
	--										WHEN 2014 THEN 'NewAssistantWelcomeEmail'
	--										ELSE 'NewSenderWelcomeEmail'
	--									END;

	--			EXEC [$(SutureSignWeb)].dbo.spAddEmailTask @UserID = @MemberId,
	--													   @PhysicianId = @MemberId,
	--													   @EmailToId = @MemberId,
	--													   @RequestorId = @MemberId,
	--													   @Assistantid =  @MemberId,
	--													   @SutureSignUrl = @SutureSignUrl,
	--													   @TemporaryPassword = @TemporaryPassword,
	--													   @EmailType = @WelcomeEmailType,
	--													   @SendWelcomeEmail = @Output;
	--		END
	--		ELSE
	--		BEGIN
	--			EXEC [$(SutureSignWeb)].dbo.spChangePasswordEmailNotification @UserName = @UserName,
	--																		  @NewPassword = @TemporaryPassword,
	--																		  @EmailAddress = @Email,
	--																		  @SutureSignUrl = @SutureSignUrl,
	--																		  @ReturnMessage = NULL;
	--		END
	--	END
	--END

	UPDATE uf
	SET uf.[Primary] = CASE om.OrganizationId WHEN @DefaultOrganizationId THEN 1 ELSE 0 END,
		uf.[Active] = om.IsActive,
		uf.EffectiveDate = GETDATE(),
		uf.SubmittedBy = @UpdatedByMemberId,
		uf.[Admin] = om.IsAdministrator,
		uf.ActiveUpdate = CASE WHEN uf.[Active] != om.IsActive THEN GETDATE() ELSE uf.ActiveUpdate END,
		uf.IsBillingAdmin = om.IsBillingAdministrator,
		uf.CanSign = CASE WHEN COALESCE(@MemberTypeId, @OriginalMemberTypeId) = 2000 THEN 1 ELSE COALESCE(@CanSign, uf.CanSign, 0) END
	FROM @OrganizationMembers om
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON om.OrganizationId = uf.FacilityId AND uf.UserId = @MemberId;

	INSERT INTO [$(SutureSignWeb)].dbo.Users_Facilities (UserId, FacilityId, [Primary], Active, CreateDate, EffectiveDate, SubmittedBy, [Admin], CanSign, IsBillingAdmin)
	SELECT
		@MemberId,
		om.OrganizationId,
		CASE om.OrganizationId WHEN @DefaultOrganizationId THEN 1 ELSE 0 END,
		1,
		GETDATE(),
		GETDATE(),
		@UpdatedByMemberId,
		om.IsAdministrator,
		CASE WHEN COALESCE(@MemberTypeId, @OriginalMemberTypeId) = 2000 THEN 1 ELSE COALESCE(@CanSign, 0) END,
		om.IsBillingAdministrator
	FROM @OrganizationMembers om
		LEFT JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON om.OrganizationId = uf.FacilityId AND uf.UserId = @MemberId
	WHERE om.IsActive = 1 AND uf.Id IS NULL;

	UPDATE uf
	SET uf.[Primary] = CASE uf.FacilityId WHEN @DefaultOrganizationId THEN 1 ELSE 0 END,
		uf.EffectiveDate = GETDATE(),
		uf.SubmittedBy = @UpdatedByMemberId
	FROM [$(SutureSignWeb)].dbo.Users_Facilities uf
		LEFT JOIN @OrganizationMembers om ON uf.FacilityId = om.OrganizationId
	WHERE uf.UserId = @MemberId AND om.OrganizationId IS NULL;

	IF (@OriginalMemberTypeId != COALESCE(@MemberTypeId, @OriginalMemberTypeId))
	BEGIN
		DELETE FROM [$(SutureSignWeb)].dbo.Users_Managers WHERE ManagerId = @MemberId OR UserId = @MemberId;
	END

	UPDATE um
	   SET um.Active = CASE WHEN related.Id IS NOT NULL THEN 1 ELSE 0 END,
		   um.DateMod = GETDATE(),
		   um.ChangedBy = @UpdatedByMemberId
	  FROM [$(SutureSignWeb)].dbo.Users_Managers um
	 INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON um.ManagerId = uf.UserId OR um.UserId = uf.UserId
	 INNER JOIN @OrganizationMembers om ON uf.FacilityId = om.OrganizationId
	  LEFT JOIN @RelatedMemberIds related ON um.ManagerId = related.Id OR um.UserId = related.Id
	 WHERE (um.ManagerId = @MemberId OR um.UserId = @MemberId) AND um.Active != CASE WHEN related.Id IS NOT NULL THEN 1 ELSE 0 END;

	INSERT INTO [$(SutureSignWeb)].dbo.Users_Managers (UserId, ManagerId, Active, DateMod, ChangedBy, PrimaryManager)
	SELECT
		related.Id,
		@MemberId,
		1,
		GETDATE(),
		@UpdatedByMemberId,
		0
	FROM @RelatedMemberIds related
		INNER JOIN
		(
			SELECT DISTINCT uf.UserId
			FROM [$(SutureSignWeb)].dbo.Users_Facilities uf WITH (NOLOCK)
				INNER JOIN @OrganizationMembers om ON uf.FacilityId = om.OrganizationId
		) org_members ON related.Id = org_members.UserId
		INNER JOIN [$(SutureSignWeb)].dbo.Users u WITH (NOLOCK) ON related.Id = u.UserId
		LEFT JOIN [$(SutureSignWeb)].dbo.Users_Managers um WITH (NOLOCK) ON related.Id = um.UserId AND um.ManagerId = @MemberId
	WHERE (COALESCE(@MemberTypeId, @OriginalMemberTypeId) = 2000 OR (@IsCollaborator = 1 AND u.UserTypeId != 2000)) AND um.UserId IS NULL
	UNION ALL
	SELECT
		@MemberId,
		related.Id,
		1,
		GETDATE(),
		@UpdatedByMemberId,
		0
	FROM @RelatedMemberIds related
		INNER JOIN
		(
			SELECT DISTINCT uf.UserId
			FROM [$(SutureSignWeb)].dbo.Users_Facilities uf WITH (NOLOCK)
				INNER JOIN @OrganizationMembers om ON uf.FacilityId = om.OrganizationId
		) org_members ON related.Id = org_members.UserId
		INNER JOIN [$(SutureSignWeb)].dbo.Users u WITH (NOLOCK) ON related.Id = u.UserId
		LEFT JOIN [$(SutureSignWeb)].dbo.Users_Managers um WITH (NOLOCK) ON related.Id = um.ManagerId AND um.UserId = @MemberId
	WHERE (u.UserTypeId = 2000 OR (u.UserTypeId IN (2001, 2008) AND COALESCE(@MemberTypeId, @OriginalMemberTypeId) != 2000)) AND um.ManagerId IS NULL;
	
	SELECT @MemberId;
END
