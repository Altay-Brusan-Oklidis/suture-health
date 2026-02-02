CREATE TABLE [IntegratorOrganization] (
    [IntegratorOrganizationId] int NOT NULL IDENTITY,
    [IntegratorId] int NOT NULL,
    [OrganizationId] int NOT NULL,
    [ApiKey] nvarchar(64) NULL,
    [IsActive] bit NOT NULL,
    [EffectiveDate] datetimeoffset NULL,
    [ExpirationDate] datetimeoffset NULL,
    CONSTRAINT [PK_IntegratorOrganization] PRIMARY KEY ([IntegratorOrganizationId]),
    CONSTRAINT [FK_IntegratorOrganization_Integrators_IntegratorId] FOREIGN KEY ([IntegratorId]) REFERENCES [Integrator] ([IntegratorId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_IntegratorOrganization_IntegratorId] ON [IntegratorOrganization] ([IntegratorId]);
GO

CREATE INDEX [IX_IntegratorOrganization_OrganizationId] ON [IntegratorOrganization] ([OrganizationId]);
GO
