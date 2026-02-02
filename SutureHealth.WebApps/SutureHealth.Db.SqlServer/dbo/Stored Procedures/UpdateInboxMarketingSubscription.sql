CREATE PROCEDURE [dbo].[UpdateInboxMarketingSubscription]
	@OrganizationId INT,
	@Active BIT
AS
	DECLARE @BillableEntityId AS INT, @SysServiceInboxMarketingId AS INT
	SET @BillableEntityId = ISNULL((SELECT TOP 1 [Id] FROM [$(SutureSignWeb)].dbo.BillableEntities WHERE ObjectId = @OrganizationId), 0)
	SET @SysServiceInboxMarketingId = (SELECT TOP 1 [Id] FROM [$(SutureSignWeb)].dbo.SysServices WHERE [Name] = 'Inbox Marketing')

	IF (@BillableEntityId = 0)
		INSERT INTO [$(SutureSignWeb)].dbo.BillableEntities (ObjectId, ObjectType, Active) VALUES (@OrganizationId, 'Facility', 1)
		SET @BillableEntityId = (SELECT TOP 1 [Id] FROM [$(SutureSignWeb)].dbo.BillableEntities WHERE ObjectId = @OrganizationId)

	MERGE INTO [$(SutureSignWeb)].dbo.BillableEntity_SysServices AS targetTable
		USING (VALUES (@BillableEntityId, @SysServiceInboxMarketingId, @Active)) AS source (BillableEntityId, InboxMarketingId, Active)
		ON (targetTable.BillableEntityId = source.BillableEntityId AND targetTable.SysServiceId = source.InboxMarketingId)
		WHEN MATCHED THEN
			UPDATE SET Active = source.Active
		WHEN NOT MATCHED THEN
			INSERT (BillableEntityId, SysServiceId, Active, CreateDate) 
			VALUES (source.BillableEntityId, source.InboxMarketingId, source.Active, GETDATE());

	SELECT CAST(1 AS BIT) AS Success;