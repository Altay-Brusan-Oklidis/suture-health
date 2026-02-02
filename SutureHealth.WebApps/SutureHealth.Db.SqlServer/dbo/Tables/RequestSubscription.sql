CREATE TABLE [dbo].[RequestSubscription] (
    [RequestSubscriptionId] bigint NOT NULL IDENTITY,
    [RequestId] bigint NOT NULL,
    [SubscriptionId] uniqueidentifier NOT NULL,
    [NotificationUrl] nvarchar(2048) NULL,
    [ClientState] nvarchar(256) NULL,
    CONSTRAINT [PK_RequestSubscription] PRIMARY KEY ([RequestSubscriptionId]),
    CONSTRAINT [FK_RequestSubscription_Request_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [TransmittedRequest] ([TransmittedRequestId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RequestSubscription_RequestId] ON [dbo].[RequestSubscription] ([RequestId]);
GO
