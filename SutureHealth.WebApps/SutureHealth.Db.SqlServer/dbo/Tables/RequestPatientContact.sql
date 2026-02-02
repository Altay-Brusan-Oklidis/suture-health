CREATE TABLE [dbo].[RequestPatientContact]
(
    [RequestPatientContactId] bigint NOT NULL IDENTITY,
    [RequestPatientId] bigint NOT NULL,
    [Type] INT NOT NULL,
    [Value] nvarchar(254) NULL,
    CONSTRAINT [PK_RequestPatientContact] PRIMARY KEY ([RequestPatientContactId]),
    CONSTRAINT [FK_RequestPatientContact_RequestPatient_RequestPatientId] FOREIGN KEY ([RequestPatientId]) REFERENCES [RequestPatient] ([RequestPatientId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RequestPatientContact_RequestId] ON [dbo].[RequestPatientContact] ([RequestPatientId]);
GO
