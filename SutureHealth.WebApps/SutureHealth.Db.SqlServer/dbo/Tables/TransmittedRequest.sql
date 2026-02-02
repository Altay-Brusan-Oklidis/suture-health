CREATE TABLE [dbo].[TransmittedRequest] (
    [TransmittedRequestId] bigint NOT NULL IDENTITY,
    [UniqueRequestId] uniqueidentifier NOT NULL,
    [ExternalRequestId] nvarchar(150) NULL,
    [Status] tinyint NOT NULL,
    [SubmittedAt] datetimeoffset NOT NULL,
    [LastUpdatedAt] datetimeoffset NOT NULL,
    [ApiVersion] nvarchar(32) NULL,
    [ApiRequest] nvarchar(max) NULL,
    [IntegratorId] int NOT NULL,
    [SenderId] int NOT NULL,
    [OrganizationId] int NOT NULL,
    [SutureSignRequestID] bigint NULL,
    [IsModified] bit DEFAULT(0),
    [RejectionReason] nvarchar(max) NULL,
    [DocumentStatusDate] datetimeoffset NULL,
    [StartOfCare] datetimeoffset NULL,
    [DiagnosisCode] int NOT NULL,
    CONSTRAINT [PK_TransmittedRequest] PRIMARY KEY ([TransmittedRequestId])
);
GO

CREATE INDEX [IX_TransmittedRequest_Status] ON [dbo].[TransmittedRequest] ([TransmittedRequestId],[Status],[SenderId],[OrganizationId]);
GO

CREATE UNIQUE INDEX [IX_TransmittedRequest_SutureSignRequestID] ON [dbo].[TransmittedRequest] ([SutureSignRequestID]) WHERE [SutureSignRequestID] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_TransmittedRequest_OrganizationExternalRequestId] ON [dbo].[TransmittedRequest] ([OrganizationId], [ExternalRequestId]) WHERE [ExternalRequestId] IS NOT NULL;
GO

CREATE TRIGGER [dbo].[TR_TransmittedRequest_IU] ON [dbo].[TransmittedRequest]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.TransmittedRequestStatusTransition
    (
        [TransmittedRequestId],
        [Status],
        [PreviousStatus],
        [TransitionedAt]
    )
    SELECT
        i.[TransmittedRequestId],
        i.[Status],
        d.[Status],
        GETUTCDATE()
    FROM
        inserted i
            LEFT JOIN deleted d ON i.TransmittedRequestId = d.TransmittedRequestId
    WHERE
        d.TransmittedRequestId IS NULL OR i.[Status] <> d.[Status];
END
GO