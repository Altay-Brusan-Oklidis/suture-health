DROP TRIGGER IF EXISTS dbo.TR_MemberAuditEvent_UnSignDocuments_I;

IF (DB_NAME() = 'SutureSignApi-Demo')
BEGIN
	EXEC('CREATE TRIGGER dbo.TR_MemberAuditEvent_UnSignDocuments_I
ON dbo.MemberAuditEvent
AFTER INSERT
AS BEGIN
	DECLARE @MemberId INT = NULL;

	SELECT TOP 1
		@MemberId = MemberId
	FROM inserted
	WHERE AuditEventID = 1;

	IF (@MemberId IS NOT NULL AND DB_NAME() = ''SutureSignApi-Demo'')
	BEGIN
		EXEC [$(SutureSignWeb)].dbo.spUnSignDocuments @UserId = @MemberId;
	END
END');
END
