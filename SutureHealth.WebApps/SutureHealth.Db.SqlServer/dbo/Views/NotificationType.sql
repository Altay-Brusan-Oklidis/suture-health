CREATE VIEW [dbo].[NotificationType]
AS
	SELECT
		NotificationTypeId,
		NotificationDescription				[Description]
	FROM [$(SutureSignWeb)].dbo.NotificationTypes