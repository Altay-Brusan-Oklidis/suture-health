CREATE VIEW [dbo].[MemberContact]
AS 
SELECT uc.Id                                    MemberContactId,
       uc.UserId                                MemberId,
       CASE
            WHEN uc.[Type] IN ('Phone', 'Fax', 'Mobile', 'Email', 'Url', 'OfficePhone', 'OfficePhoneExt') THEN uc.[Type]
            ELSE 'OfficePhone'
       END                                      [Type],
       uc.[Value],
       CAST(COALESCE(uc.Active, 0) AS BIT)      IsActive,
       CAST(COALESCE(uc.[Confirmed], 0) AS BIT) IsConfirmed,
       CAST(COALESCE(uc.[Primary], 0) AS BIT)   IsPrimary,
       uc.CreateDate                            CreatedAt,
       uc.SubmittedBy                           CreatedBy,
       uc.EffectiveDate                         EffectiveAt
  FROM [$(SutureSignWeb)].dbo.Users_Contacts uc WITH (NOLOCK)