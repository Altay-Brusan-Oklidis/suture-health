CREATE TABLE [dbo].[RequestDocument] (
    [Id] bigint NOT NULL IDENTITY,
    [RequestId] bigint NOT NULL,
    [TemplateId] int NULL,
    [FileName] nvarchar(256) NULL,
    [StorageContainer] nvarchar(64) NULL,
    [StorageKey] nvarchar(1024) NULL,
    [DocumentDate] datetimeoffset NOT NULL,
    [DocumentTypeKey] nvarchar(256) NULL,
    CONSTRAINT [PK_RequestDocument] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RequestDocument_Request_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [TransmittedRequest] ([TransmittedRequestId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RequestDocument_RequestId] ON [dbo].[RequestDocument] ([RequestId]);
GO
