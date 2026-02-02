CREATE VIEW [dbo].[TemplateAnnotation]
AS
SELECT
	TemplateCoordinateId			[TemplateAnnotationId],
	TemplateId						[TemplateId],
	CASE
		WHEN [Placeholder] IN
			(
				'VisibleSignature',
				'DateSigned',
				'CheckBox',
				'TextArea'
			) THEN [Placeholder]
		ELSE 'Unknown'
	END								[AnnotationType],
	coordLeft						[PdfCoordinateLeft],
	coordBottom						[PdfCoordinateBottom],
	coordRight						[PdfCoordinateRight],
	coordTop						[PdfCoordinateTop],
	PageNumber						[PageNumber],
	[Value]							[Value],
	htmlCoordinateLeft				[HtmlCoordinateLeft],
	htmlCoordinateBottom			[HtmlCoordinateBottom],
	htmlCoordinateRight				[HtmlCoordinateRight],
	htmlCoordinateTop				[HtmlCoordinateTop],
	htmlPageHeight					[PageHeight]
FROM
	[$(SutureSignWeb)].dbo.TemplateCoordinates