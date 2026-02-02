CREATE TABLE [dbo].[AnnotationFieldType]
(
	[AnnotationFieldTypeId] INT NOT NULL IDENTITY(1, 1) CONSTRAINT PK_AnnotationFieldTypeId PRIMARY KEY CLUSTERED,
	[Description] VARCHAR(64) NOT NULL
)
