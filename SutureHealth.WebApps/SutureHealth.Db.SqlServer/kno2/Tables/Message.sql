CREATE TABLE [kno2].[Message]
(
    [Id] BIGINT NOT NULL IDENTITY,
    [ObfuscatedId] CHAR(40) NOT NULL,
    [ThreadId] BIGINT NULL, 
    [OrganizationId] CHAR(40) NULL, 
    [OriginalObjectId] CHAR(40) NULL, 
    [MessageDate] DATETIME2(2) NULL, 
    [CreatedDate] DATETIME2(2) NULL, 
    [UpdatedDate] DATETIME2(2) NULL, 
    [FromAddress] VARCHAR(320) NULL, 
    [ToAddress] VARCHAR(320) NULL, 
    [PatientName] NVARCHAR(100) NULL,
    [Properties] NVARCHAR(MAX) NULL,
    [Subject] NVARCHAR(1000) NULL, 
    [Body] NVARCHAR(4000) NULL,
    [PatientId] BIGINT NULL,
    [ReasonForDisclosure] NVARCHAR(1000) NULL, 
    [Origin] NVARCHAR(50) NULL, 
    [IsProcessed] BIT NULL, 
    [IsUrgent] BIT NULL, 
    [IsDraft] BIT NULL, 
    [Status] VARCHAR(10) NULL, -- 0 = None, 1 = Received, 2 = Pending, 3 = Signed, 4 = Suspended, 5 = Deleted, 6 = Removed, 7 = Forwarded, 8 = Replied, 9 = Processed, 10 = Uploaded, 11 = Triaged, 12 = Failed
    [ProcessedType] VARCHAR(20) NULL, -- 0 = None, 1 = StructuredExport, 2 = PDFExport, 4 = NativeExport, 8 = Printed, 16 = Saved, 32 = AwaitingEMRExport, 64 = EMRExported, 128 = ForceProcessed
    [ProcessTypes] VARCHAR(MAX) NULL, -- TODO: we hold this as json for now, when we have some data we can think how to optimize.
    [Priority] VARCHAR(10) NULL, -- 0 = NotUrgent, 1 = Urgent
    [ChannelId] VARCHAR(40) NULL, 
    [SourceType] VARCHAR(15) NULL, -- 0 = DirectMessage, 1 = RecordRequest, 2 = Triage, 3 = Fax, 4 = Carequality, 5 = NaviHealth, 6 = Ihde
    [UnprocessedNotificationSent] DATETIME2(2) NULL, 
    [Attachments2Pdf] BIT NULL, -- deprecated
    [Attachments2Cda] BIT NULL, -- deprecated
    [Attachments2HL7] BIT NULL, -- deprecated
    [AttachmentSendType] VARCHAR(10) NULL, -- 0 = Original, 1 = Pdf, 2 = Cda, 3 = Tif, 4 = PdfCda, 5 = TifCda, 6 = Hl7
    [ReleaseTypeId] VARCHAR(40) NULL, 
    [IsNew] BIT NULL,
    [ConversationId] BIGINT NULL,
    [Type] VARCHAR(10) NULL, -- 1 = Intake, 2 = Release
    [MessageType] VARCHAR(10) NULL,
    [ClassificationId] BIGINT NULL,
    [HispMessageIds] VARCHAR(MAX) NULL,  -- TODO: we hold this as json for now, when we have some data we can think how to optimize.
    CONSTRAINT [PK_Message] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Message_ObfuscatedId] UNIQUE ([ObfuscatedId]),
    CONSTRAINT [FK_Message_Patient] FOREIGN KEY ([PatientId]) REFERENCES [kno2].[Patient](Id),
    CONSTRAINT [FK_Message_Conversation] FOREIGN KEY ([ConversationId]) REFERENCES [kno2].[Conversation](Id),
    CONSTRAINT [FK_Message_Classification] FOREIGN KEY ([ClassificationId]) REFERENCES [kno2].[Classification](Id)
)
