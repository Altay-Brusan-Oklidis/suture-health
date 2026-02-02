CREATE TABLE [dbo].[AnnotationOCRMapping]
(
	[AnnotationOCRMappingId] INT NOT NULL IDENTITY(1, 1) CONSTRAINT PK_AnnotationOCRMappingId PRIMARY KEY CLUSTERED,
	[TemplateConfigurationId] INT NOT NULL CONSTRAINT FK_AnnotationOCRMapping_TemplateConfigurationId FOREIGN KEY REFERENCES dbo.TemplateConfiguration(TemplateConfigurationId),
	[AnnotationFieldTypeId] INT NOT NULL CONSTRAINT FK_AnnotationOCRMapping_AnnotationFieldTypeId FOREIGN KEY REFERENCES dbo.AnnotationFieldType(AnnotationFieldTypeId),
	[SearchText] NVARCHAR(128) NOT NULL,
	[OffsetX] DECIMAL(9, 7) NOT NULL,
	[OffsetY] DECIMAL(9, 7) NOT NULL,
	[Width] DECIMAL(9, 7) NOT NULL,
	[Height] DECIMAL(9, 7) NOT NULL,
	[MatchAll] BIT NOT NULL CONSTRAINT DF_AnnotationOCRMapping_MatchAll DEFAULT (0)
)
