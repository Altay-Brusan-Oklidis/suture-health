CREATE TABLE [kno2].[Attachment]
(
    [Id] BIGINT NOT NULL IDENTITY,
    [ObfuscatedId] CHAR(40) NOT NULL,
    [MessageId] BIGINT NOT NULL,
    [ObfuscatedMessageId] CHAR(40) NOT NULL,
    [Key] VARCHAR(40) NULL, -- deprecated
    [DocumentType] VARCHAR(50) NULL, 
    [MimeType] VARCHAR(50) NULL, 
    [FileName] NVARCHAR(1000) NULL, 
    [SizeInBytes] BIGINT NULL,
    [IntegrationMeta] NVARCHAR(MAX) NULL, -- TODO: we hold this as json string - think how to normalize it once we have some data. There is the same column in AttachmentMeta and Patient - check if they are the same.
    [IsClone] BIT NULL,
    [IsPreviewAvailable] BIT NULL, 
    [IsRestorable] BIT NULL, 
    [PreviewKey] VARCHAR(48) NULL, -- deprecated
    [PreviewAvailable] VARCHAR(12) NULL,
    [Recipients] NVARCHAR(MAX) NULL, -- TODO: we hold this as json string - think how to normalize it once we have some data
    [Transforms] NVARCHAR(MAX) NULL, -- TODO: we hold this as json string - think how to normalize it once we have some data
    [TransformStatus] VARCHAR(10) NULL, -- 0 = None, 1 = Pending, 2 = Failed, 3 = Completed
    CONSTRAINT [PK_Attachment] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Attachment_ObfuscatedId] UNIQUE ([ObfuscatedId]),
    CONSTRAINT [FK_Attachment_Message] FOREIGN KEY ([MessageId]) REFERENCES [kno2].[Message](Id)
)
