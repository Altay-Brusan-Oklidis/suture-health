CREATE TABLE [dbo].[RequestPatientIdentifier]
(
    [RequestPatientIdentifierId] bigint NOT NULL IDENTITY,
    [RequestPatientId] bigint NOT NULL,
    [Type] nvarchar(256) NULL,
    [Value] nvarchar(256) NULL,
    CONSTRAINT [PK_RequestPatientIdentifier] PRIMARY KEY ([RequestPatientIdentifierId]),
    CONSTRAINT [FK_RequestPatientIdentifier_RequestPatient_RequestPatientId] FOREIGN KEY ([RequestPatientId]) REFERENCES [RequestPatient] ([RequestPatientId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RequestPatientIdentifier_RequestId] ON [dbo].[RequestPatientIdentifier] ([RequestPatientId]);
GO
