CREATE PROCEDURE [dbo].[SelectRequestMetadata]
	@LegacyRequestIds IntegerKey READONLY
AS
BEGIN
	SELECT
		result.[SutureSignRequestId],
		result.[Data]
	FROM
	(
		SELECT
			ids.Id		[SutureSignRequestId],
			(
				SELECT TOP 1 [Data]
				FROM [$(SutureSignWeb)].dbo.Tasks
				WHERE FormId = ids.Id AND ActionId = 527 AND [Active] = 1
				ORDER BY TaskId DESC
			)			[Data]
		FROM (SELECT DISTINCT Id FROM @LegacyRequestIds) ids
	) result
	WHERE result.[Data] IS NOT NULL;
END