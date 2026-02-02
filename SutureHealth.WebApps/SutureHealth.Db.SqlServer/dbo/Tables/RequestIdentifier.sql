CREATE TABLE [dbo].[RequestIdentifier] (
    [RequestIdentifierId] bigint NOT NULL IDENTITY,
    [RequestId] bigint NOT NULL,
    [Type] nvarchar(256) NULL,
    [Value] nvarchar(256) NULL,
    CONSTRAINT [PK_RequestIdentifier] PRIMARY KEY ([RequestIdentifierId]),
    CONSTRAINT [FK_RequestIdentifier_Request_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [TransmittedRequest] ([TransmittedRequestId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RequestIdentifier_RequestId] ON [dbo].[RequestIdentifier] ([RequestId]);
GO
