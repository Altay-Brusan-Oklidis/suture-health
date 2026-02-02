CREATE VIEW [dbo].[Member]
AS 
 SELECT u.UserId                                MemberId,
        u.UserName,
        u.UserTypeId                            MemberTypeId,
        u.FirstName,
        u.MiddleName,
        u.MaidenName,
        u.LastName,
        u.Suffix,
        u.PrimCredential                        ProfessionalSuffix,
        u.SigningName,
        u.UserNPI                               NPI,
        u.CreatedDate                           CreatedAt,
        u.CreatedBy                             CreatedBy,
        CASE
            WHEN po.FacilityId IS NULL THEN CAST(0 AS BIT)
            ELSE u.Active                                    
        END                                     IsActive,
        u.SubmittedBy                           UpdatedBy,
        COALESCE(u.UpdatedDate, u.CreatedDate)  UpdatedAt,
        u.Expiration                            ExpiredAt,
        u.Locked                                LockedAt,
        u.[Suspend]                             SuspendedAt,
        u.NeedToReadEula                        MustReadEula,
        u.ResetPwd                              MustRegisterAccount,
        u.MustChangePassword,
        u.IsCollaborator,
        u.LastLoggedInAt,
        u.CanSign,        
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
        COALESCE(po.FacilityId, CAST(0 AS BIT)) PrimaryOrganizationId,
        (
            SELECT TOP 1 [Value]
              FROM [$(SutureSignWeb)].dbo.Users_Contacts uc WITH (NOLOCK) 
             WHERE uc.[Type] = 'Email' AND uc.Active = 1 AND u.UserId = uc.UserId
             ORDER BY uc.[Primary] DESC
        ) Email,
        (
            SELECT TOP 1 [Value] AS MobileNumber
              FROM [$(SutureSignWeb)].dbo.Users_Contacts uc WITH (NOLOCK) 
             WHERE uc.[Type] = 'Mobile' AND uc.Active = 1 AND u.UserId = uc.UserId
             ORDER BY uc.[Primary] DESC
        ) MobileNumber
   FROM [$(SutureSignWeb)].dbo.Users u WITH (NOLOCK)
  OUTER APPLY (SELECT TOP 1 uf.FacilityId 
                 FROM [$(SutureSignWeb)].dbo.Users_Facilities uf WITH (NOLOCK) 
                WHERE (uf.EffectiveDate IS NULL OR uf.EffectiveDate < GETUTCDATE()) AND
                      (uf.[Suspend] IS NULL OR uf.[Suspend] > GETUTCDATE()) AND
                      uf.Active = 1 AND u.UserId = uf.UserId
                ORDER BY uf.[Primary] DESC) po