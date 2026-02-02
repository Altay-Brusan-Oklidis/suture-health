CREATE VIEW [dbo].[MemberIdentity]
AS 
SELECT  u.UserId                                    MemberId,
        u.UserName,
        u.UserTypeId                                MemberTypeId,
        u.FirstName,
        u.MiddleName,
        u.MaidenName,
        u.LastName,
        u.Suffix,
        u.PrimCredential                            ProfessionalSuffix,
        u.IsCollaborator,
        u.CanSign,
        u.SigningName,
        u.UserNPI                                   NPI,
        u.CreatedDate                               CreatedAt,
        u.CreatedBy,
        CASE
            WHEN po.FacilityId IS NULL THEN CAST(0 AS BIT)
            ELSE u.Active                                    
        END                                         IsActive,
        u.SubmittedBy                               UpdatedBy,
        COALESCE(u.UpdatedDate, u.CreatedDate)      UpdatedAt,
        u.AccessFailedCount,
        u.ConcurrencyStamp,
        u.EulaEffDate                               EffectiveAt,
        u.Expiration                                ExpiredAt,
        u.LastLoggedInAt,
        COALESCE(u.LockoutEnabled, CAST(0 AS BIT))  LockoutEnabled,
        u.LockoutEnd,
        u.TwoFactorEnabled,
        u.MustRegisterAccount,
        u.MustChangePassword,
        u.NeedToReadEula                            MustReadEula,
        u.PasswordHash,
        u.SecurityStamp,
        u.[Suspend]                                 SuspendedAt,
        CASE
			WHEN EXISTS
			(
				SELECT TOP 1 1
				FROM [$(SutureSignWeb)].dbo.BillableEntities be WITH (NOLOCK)
					INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf WITH (NOLOCK) ON be.ObjectId = uf.FacilityId AND uf.UserId = u.UserId
					INNER JOIN [$(SutureSignWeb)].dbo.Facilities f WITH (NOLOCK) ON uf.FacilityId = f.FacilityId
				WHERE be.ObjectType = 'Facility' AND be.Active = 1
					AND uf.Active = 1 AND uf.EffectiveDate <= GETDATE() AND uf.[Primary] = 1 AND (uf.ExpirationDate IS NULL OR uf.ExpirationDate > GETDATE())
					AND f.Active = 1 AND f.EffectiveDate <= GETDATE()
				UNION ALL
				SELECT TOP 1 1
				FROM [$(SutureSignWeb)].dbo.BillableEntities be WITH (NOLOCK)
					INNER JOIN [$(SutureSignWeb)].dbo.Facilities f WITH (NOLOCK) ON be.ObjectId = f.CompanyId
					INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf WITH (NOLOCK) ON f.FacilityId = uf.FacilityId AND uf.UserId = u.UserId
				WHERE be.ObjectType = 'Company' AND be.Active = 1
					AND uf.Active = 1 AND uf.EffectiveDate <= GETDATE() AND uf.[Primary] = 1 AND (uf.ExpirationDate IS NULL OR uf.ExpirationDate > GETDATE())
					AND f.Active = 1 AND f.EffectiveDate <= GETDATE()
			) 
            THEN CAST(1 AS BIT)
			ELSE CAST(0 AS BIT)
		END AS IsPayingClient,
        CASE
            WHEN EXISTS
            (
                SELECT TOP 1 1
                  FROM [$(SutureSignWeb)].dbo.Users_Facilities uf
                 WHERE uf.UserId = u.UserId
                   AND (uf.EffectiveDate IS NULL OR uf.EffectiveDate < GETUTCDATE())
                   AND (uf.[Suspend] IS NULL OR uf.[Suspend] > GETUTCDATE())
                   AND uf.Active = 1
                   AND uf.[EditProfile] = 1
            ) 
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS CanEditOrganizationProfile,
        ec.Email,
        COALESCE(ec.Confirmed, CAST(0 AS BIT))  EmailConfirmed,
        mc.MobileNumber,
        COALESCE(mc.Confirmed, CAST(0 AS BIT))  MobileNumberConfirmed,
        oc.OfficeNumber,
        oec.OfficeExtension,
        COALESCE(po.FacilityId, CAST(0 AS BIT)) PrimaryOrganizationId

   FROM [$(SutureSignWeb)].dbo.Users u WITH (NOLOCK)
  OUTER APPLY (SELECT TOP 1 Value AS Email, Confirmed
                FROM [$(SutureSignWeb)].dbo.Users_Contacts uc WITH (NOLOCK) 
               WHERE uc.Type = 'Email' AND uc.Active = 1 AND u.UserId = uc.UserId
               ORDER BY uc.[Primary] DESC) ec 
  OUTER APPLY (SELECT TOP 1 Value AS MobileNumber, Confirmed
                FROM [$(SutureSignWeb)].dbo.Users_Contacts uc WITH (NOLOCK) 
               WHERE uc.Type = 'Mobile' AND uc.Active = 1 AND u.UserId = uc.UserId
               ORDER BY uc.[Primary] DESC) mc
  OUTER APPLY (SELECT TOP 1 Value AS OfficeNumber
                FROM [$(SutureSignWeb)].dbo.Users_Contacts uc WITH (NOLOCK) 
               WHERE uc.Type = 'OfficePhone' AND uc.Active = 1 AND u.UserId = uc.UserId
               ORDER BY uc.[Primary] DESC) oc
  OUTER APPLY (SELECT TOP 1 Value AS OfficeExtension
                FROM [$(SutureSignWeb)].dbo.Users_Contacts uc WITH (NOLOCK) 
               WHERE uc.Type = 'OfficePhoneExt' AND uc.Active = 1 AND u.UserId = uc.UserId
               ORDER BY uc.[Primary] DESC) oec
  OUTER APPLY (SELECT TOP 1 uf.FacilityId 
                FROM [$(SutureSignWeb)].dbo.Users_Facilities uf WITH (NOLOCK) 
               WHERE (uf.EffectiveDate IS NULL OR uf.EffectiveDate < GETUTCDATE()) AND
                     (uf.[Suspend] IS NULL OR uf.[Suspend] > GETUTCDATE()) AND
                     uf.Active = 1 AND u.UserId = uf.UserId
               ORDER BY uf.[Primary] DESC) po
