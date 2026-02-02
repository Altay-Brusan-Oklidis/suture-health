CREATE PROCEDURE [dbo].[SetPublicIdentityActive]
    @publicIdentityId   INT,
    @active             BIT
AS
    UPDATE [$(SutureSignWeb)].dbo.PublicIdentity
       SET Active = @active
     WHERE ID = @publicIdentityId;
