CREATE TABLE [dbo].[TransmittedRequestStatusTransition]
(
	[RequestStatusTransitionId] BIGINT NOT NULL IDENTITY(1, 1) CONSTRAINT PK_RequestStatusTransition PRIMARY KEY CLUSTERED,
	[TransmittedRequestId] BIGINT NOT NULL,
	[Status] TINYINT NOT NULL,
	[PreviousStatus] TINYINT NULL,
	[TransitionedAt] DATETIMEOFFSET(7) NOT NULL
);
GO

CREATE INDEX IX_RequestStatusTransition_RequestId ON [dbo].[TransmittedRequestStatusTransition] ([TransmittedRequestId]);
GO