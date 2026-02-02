CREATE TABLE [dbo].[MemberAuditEvent]
(
	[MemberAuditEventId]	BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[MemberId]				INT NOT NULL,
	[AuditDate]				DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
	[AuditEventID]			INT NOT NULL,
	[AuditEventName]		VARCHAR(255) NOT NULL,
	[ServiceProvider]		VARCHAR(1024),
	[IpAddress]				VARCHAR(50),
	[Succeeded]				BIT NOT NULL DEFAULT 0
)
GO

CREATE INDEX [IC_MemberAuditEvent_MemberId] ON [dbo].[MemberAuditEvent] ([MemberId])
