CREATE TABLE [dbo].[RequestPatientMatchLog]
(
	[RequestPatientMatchLogId] bigint NOT NULL IDENTITY,
	[RequestPatientId] bigint NOT NULL,
	[MatchDate] datetimeoffset NOT NULL,
	[MatchLog] nvarchar(MAX) NOT NULL,
	CONSTRAINT [PK_RequestPatientMatchLog] PRIMARY KEY ([RequestPatientMatchLogId])
);
GO

CREATE INDEX [IX_RequestPatientMatchLog_RequestPatientId] ON [dbo].[RequestPatientMatchLog] ([RequestPatientId]);
GO