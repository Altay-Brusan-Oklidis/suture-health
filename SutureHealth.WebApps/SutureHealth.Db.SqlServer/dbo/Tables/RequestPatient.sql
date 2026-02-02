CREATE TABLE [dbo].[RequestPatient] (
    [RequestPatientId] bigint IDENTITY NOT NULL,
    [UniquePatientId] uniqueidentifier NOT NULL,
    [RequestId] bigint NULL,
    [SutureSignPatientId] int NULL,
    [FirstName] nvarchar(50) NULL,
    [MiddleName] nvarchar(50) NULL,
    [LastName] nvarchar(50) NULL,
    [Suffix] nvarchar(4) NULL,
    [Gender] TINYINT NOT NULL,
    [Birthdate] datetime2 NOT NULL,
    CONSTRAINT [PK_RequestPatient] PRIMARY KEY ([RequestPatientId]),
    CONSTRAINT [FK_RequestPatient_Request_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [TransmittedRequest] ([TransmittedRequestId]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_RequestPatient_RequestId] ON [dbo].[RequestPatient] ([RequestId]);
GO
