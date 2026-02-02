CREATE TABLE [dbo].[RequestError] (
    [Id] bigint NOT NULL IDENTITY,
    [RequestId] bigint NOT NULL,
    [UniqueRequestId] uniqueidentifier NOT NULL,
    [ErrorMessage] nvarchar(max) NULL,
    CONSTRAINT [PK_RequestError] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RequestError_Request_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [TransmittedRequest] ([TransmittedRequestId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RequestError_RequestId] ON [dbo].[RequestError] ([RequestId]);
GO
