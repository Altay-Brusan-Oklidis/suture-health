CREATE TABLE [kno2].[AttachmentMeta]
(
    [AttachmentId] BIGINT NOT NULL, 
    [DocumentTitle] NVARCHAR(1000) NULL, 
    [DocumentType] NVARCHAR(50) NULL, 
    [DocumentDate] DATETIME2(2) NULL, 
    [DocumentDescription] NVARCHAR(1000) NULL, 
    [Confidentiality] VARCHAR(15) NULL, -- 0 = Normal, 1 = Restricted, 2 = VeryRestricted
    [Convert] BIT NULL, 
    [UsePriorityQueue] BIT NULL, 
    [IntegrationMeta] NVARCHAR(MAX) NULL, -- TODO: we hold this as json string - think how to normalize it once we have some data. There is the same column in Attachment and Patient - check if they are the same.
    [OrderIds] NVARCHAR(MAX) NULL, -- TODO: we hold this as json string - think how to normalize it once we have some data.
    [PatientId] BIGINT NULL,
    CONSTRAINT [UK_AttachmentMeta_AttachmentId] UNIQUE ([AttachmentId]),
    CONSTRAINT [FK_AttachmentMeta_Attachment] FOREIGN KEY ([AttachmentId]) REFERENCES [kno2].[Attachment](Id),
    CONSTRAINT [FK_AttachmentMeta_Patient] FOREIGN KEY ([PatientId]) REFERENCES [kno2].[Patient](Id)
)
