CREATE VIEW [dbo].[OrganizationMember]
AS 
SELECT uf.Id                                        OrganizationMemberId,
       uf.FacilityId                                OrganizationId,
       uf.UserId                                    MemberId, 
       CAST(COALESCE(uf.EditProfile, 0) AS BIT)     CanEditProfile,
       CAST(COALESCE(uf.CanSign, 0) AS BIT)         CanSign,
       CAST(COALESCE(uf.[Admin], 0) AS BIT)         IsAdministrator,
       CAST(COALESCE(uf.IsBillingAdmin, 0) AS BIT)  IsBillingAdministrator,
       CAST(COALESCE(uf.[Primary], 0) AS BIT)       IsPrimary,
       CAST(COALESCE(uf.Active, 0) AS BIT)          IsActive,
       CONVERT(DATETIME2, uf.CreateDate)            CreatedAt,
       CONVERT(DATETIME2, uf.EffectiveDate)         EffectiveAt,
       CASE
          WHEN uf.Active = 0 THEN uf.ActiveUpdate
          ELSE NULL
       END                                          DeactivatedAt,
       CASE
          WHEN EXISTS(SELECT TOP 1 1 FROM [$(SutureSignWeb)].dbo.Users u WHERE u.UserId = uf.UserId AND u.UserTypeId = 2017) THEN CONVERT(bit, 1)
          ELSE CONVERT(bit, 0)
       END                                          IsAutomatedUser
  FROM [$(SutureSignWeb)].dbo.Users_Facilities uf