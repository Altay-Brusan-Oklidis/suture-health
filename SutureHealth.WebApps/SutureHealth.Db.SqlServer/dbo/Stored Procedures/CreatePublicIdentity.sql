CREATE PROCEDURE [dbo].[CreatePublicIdentity]
    @userId             INT,
    @identityType       VARCHAR(50),
    @expirationDate     DateTimeOffset,
    @effectiveDate      DateTimeOffset,
    @publicIdentityId   INT              OUTPUT,
    @publicIdentity     UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET @publicIdentity = NEWID();
    INSERT 
      INTO [$(SutureSignWeb)].dbo.PublicIdentity(Active, EffectiveDate, ExpirationDate, PID, [Type], UserId)
    VALUES (1, COALESCE(@effectiveDate, GETUTCDATE()), @expirationDate, @publicIdentity, @identityType, @userId);
    SET @publicIdentityId = SCOPE_IDENTITY();
END