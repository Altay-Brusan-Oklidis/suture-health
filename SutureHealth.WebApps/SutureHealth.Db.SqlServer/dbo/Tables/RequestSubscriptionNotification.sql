CREATE TABLE [dbo].[RequestSubscriptionNotification] (
    [RequestSubscriptionNotificationId] bigint NOT NULL IDENTITY,
    [RequestSubscriptionId] bigint NOT NULL,
    [Result] nvarchar(max) NULL,
    [TenantId] bigint NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [Success] bit NOT NULL,
    CONSTRAINT [PK_RequestSubscriptionNotification] PRIMARY KEY ([RequestSubscriptionNotificationId])
);
GO

CREATE INDEX [IX_RequestSubscriptionNotification_RequestSubscriptionId] ON [dbo].[RequestSubscriptionNotification] ([RequestSubscriptionId]);
GO