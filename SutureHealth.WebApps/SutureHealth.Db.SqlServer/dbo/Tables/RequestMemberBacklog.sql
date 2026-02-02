CREATE TABLE [dbo].[RequestMemberBacklog]
(
	[RequestMemberBacklogId] INT NOT NULL IDENTITY(1, 1) CONSTRAINT PK_RequestMemberBacklog PRIMARY KEY CLUSTERED,
	[SutureSignRequestId] INT NOT NULL,
	[MemberId] INT NOT NULL,
	[ExpiresAt] DATETIME2(0) NOT NULL,
	[CreatedBy] INT NOT NULL,
	[CreatedAt] DATETIME2(0) NOT NULL CONSTRAINT DF_RequestMemberBacklog_CreatedAt DEFAULT (GETUTCDATE()),
	[ClosedBy] INT NULL,
	[ClosedAt] DATETIME2(0) NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_RequestMemberBacklog_SutureSignRequestId] ON [dbo].[RequestMemberBacklog]
(
	[SutureSignRequestId] ASC
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_RequestMemberBacklog_SutureSignRequestId_MemberId] ON [dbo].[RequestMemberBacklog]
(
	[SutureSignRequestId] ASC,
	[MemberId] ASC
) WHERE [ClosedAt] IS NULL;
GO