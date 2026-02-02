CREATE TABLE [Notification] (
    [NotificationId] bigint NOT NULL IDENTITY,
    [Id] uniqueidentifier NOT NULL,
    [Type] int NOT NULL,
    [DestinationUri] nvarchar(2048) NULL,
    [OriginationDate] datetime2 NOT NULL constraint DF_Notification_OriginationDate default (getutcdate()),
    [TerminationDate] datetime2 NOT NULL,
    [CallbackUrl] nvarchar(2048) NULL,
    [SourceUrl] nvarchar(2048) NULL,
    [SourceText] nvarchar(max) NULL,
    [SourceId] nvarchar(256) NULL,
    [StatusCode] nvarchar(256) NULL,
    [Success] bit NULL,
    [Complete] bit NULL,
    [Subject] nvarchar(max) NULL,
    [Message] Text NULL,
    [ProviderId] uniqueidentifier NULL,
    [ProviderType] nvarchar(max) NULL,
    [ProviderExternalKey] nvarchar(128) NULL,
    [NotificationDate] datetime2 NULL,
    [DesiredSendDateTime] datetime2 NULL,
    [SendDateTime] datetime2 NULL,
    [AdditionalOptionsJson] nvarchar(max) NULL,
    CONSTRAINT [PK_Notification] PRIMARY KEY ([NotificationId])
);
GO

CREATE NONCLUSTERED INDEX IX_Notification_OriginationDate
    ON [dbo].[Notification] ([OriginationDate]);
GO