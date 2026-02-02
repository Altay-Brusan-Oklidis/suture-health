CREATE TABLE [dbo].[TemplateProcessingMode]
(
	[TemplateProcessingModeId] INT NOT NULL IDENTITY(1, 1) CONSTRAINT PK_TemplateProcessingModeId PRIMARY KEY CLUSTERED,
	[Description] VARCHAR(64) NOT NULL
)
