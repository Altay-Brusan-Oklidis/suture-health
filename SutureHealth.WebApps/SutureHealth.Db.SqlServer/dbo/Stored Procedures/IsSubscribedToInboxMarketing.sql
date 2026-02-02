CREATE PROCEDURE [dbo].[IsSubscribedToInboxMarketing]
	@OrganizationId INT
AS
	DECLARE @BillableEntityId AS INT, @SysServiceInboxMarketingId AS INT
	SET @BillableEntityId = ISNULL((SELECT TOP 1 [Id] FROM [$(SutureSignWeb)].dbo.BillableEntities WHERE ObjectId = @OrganizationId), 0)
	SET @SysServiceInboxMarketingId = (SELECT TOP 1 [Id] FROM [$(SutureSignWeb)].dbo.SysServices WHERE [Name] = 'Inbox Marketing')

	IF (@BillableEntityId = 0)
		SELECT CAST(0 AS BIT) AS IsSubscribed;

	DECLARE @ActiveSubscriptions AS BIT
	SELECT @ActiveSubscriptions = COUNT(*) FROM [$(SutureSignWeb)].dbo.BillableEntity_SysServices
		WHERE BillableEntityId = @BillableEntityId AND
		SysServiceId = @SysServiceInboxMarketingId AND
		Active = 1

	SELECT CAST(@ActiveSubscriptions AS BIT) AS IsSubscribed;
