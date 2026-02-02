CREATE VIEW [dbo].[MemberPublicIdentity]
AS 
SELECT [ID]                 MemberPublicIdentityId
      ,[PID]                PublicIdentity
      ,[UserId]             MemberId
      ,[ExpirationDate]
      ,[EffectiveDate]
      ,[Type]               [UseType]
      ,[Active]
  FROM [$(SutureSignWeb)].dbo.PublicIdentity pi WITH (NOLOCK)
