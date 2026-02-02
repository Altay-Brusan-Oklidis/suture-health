CREATE TABLE [dbo].[TemplateConfiguration]
(
	[TemplateConfigurationId] INT NOT NULL IDENTITY(1, 1) CONSTRAINT PK_TemplateConfigurationId PRIMARY KEY CLUSTERED,
	[TemplateId] INT NOT NULL CONSTRAINT IX_TemplateConfiguration_TemplateId UNIQUE NONCLUSTERED,
	[DocumentTypeKey] VARCHAR(256) NOT NULL CONSTRAINT DF_TemplateConfiguration_DocumentTypeKey DEFAULT(''),
	[TemplateProcessingModeId] INT NOT NULL CONSTRAINT FK_TemplateConfiguration_TemplateProcessingModeId FOREIGN KEY REFERENCES dbo.TemplateProcessingMode(TemplateProcessingModeId),
	[OCRDocumentId] INT NULL CONSTRAINT FK_TemplateConfiguration_OCRDocumentId FOREIGN KEY REFERENCES dbo.OCRDocument(OCRDocumentID),
	[DateCreated] DATETIME2(0) NOT NULL CONSTRAINT DF_TemplateConfiguration_DateCreated DEFAULT(GETUTCDATE()),
	[CreatedBy] VARCHAR(128) NOT NULL,
	[DateModified] DATETIME2(0) NULL,
	[ModifiedBy] VARCHAR(128) NULL,
	[Enabled] BIT NOT NULL CONSTRAINT DF_TemplateConfiguration_Enabled DEFAULT (0)
)
