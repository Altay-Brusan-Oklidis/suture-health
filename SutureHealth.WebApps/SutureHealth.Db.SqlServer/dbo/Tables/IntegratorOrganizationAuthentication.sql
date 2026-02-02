CREATE TABLE [dbo].[IntegratorOrganizationAuthentication]
(
    [IntegratorOrganizationAuthenticationId] INT NOT NULL PRIMARY KEY,
    [IntegratorOrganizationId] INT NOT NULL,
    [UserName] NVARCHAR(128) NOT NULL,
    [IsActive] bit NOT NULL,
    [EffectiveDate] datetimeoffset NULL,
    [ExpirationDate] datetimeoffset NULL,
)
