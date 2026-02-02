CREATE TYPE [dbo].[TemplateAnnotation] AS TABLE
(
	[AnnotationFieldTypeId] INT NOT NULL,
	[PageNumber] INT NOT NULL,
	[Top] INT NOT NULL,
	[Right] INT NOT NULL,
	[Bottom] INT NOT NULL,
	[Left] INT NOT NULL,
	[PageHeight] INT NOT NULL,
	[Value] VARCHAR(2800) NULL
)
