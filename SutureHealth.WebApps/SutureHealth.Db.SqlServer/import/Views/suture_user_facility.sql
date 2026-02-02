CREATE VIEW [import].suture_user_facility
AS
SELECT Id                               as SutureUserFacilityId
      ,UserId                           as SutureUserFacilityUserId
      ,FacilityId                       as SutureUserFacilityFacilityId
      ,[Primary]                        as [Primary]
      ,COALESCE(CreateDate, '')         as CreatedAt
      ,COALESCE(EffectiveDate, '')      as EffectiveAt
      ,TRY_CAST(SubmittedBy as bigint)  as SubmittedByUserId
      ,[Admin]                          as [Admin]
      ,EditProfile                      as EditProfile
      ,COALESCE(ActiveUpdate, '')       as ActiveUpdate
      ,CanSign                          as CanSign
 FROM [$(SutureSignWeb)].dbo.Users_Facilities WITH (NOLOCK)
WHERE Active = 1