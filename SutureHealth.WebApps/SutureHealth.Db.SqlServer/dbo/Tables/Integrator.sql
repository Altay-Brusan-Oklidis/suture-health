CREATE TABLE [Integrator] (
    [IntegratorId] int NOT NULL IDENTITY,
    [ApiKey] nvarchar(50) NULL,
    [Name] nvarchar(250) NULL,
    [ContactName] nvarchar(250) NULL,
    [ContactTelephoneNumber] nvarchar(10) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] INT NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedBy] INT NOT NULL,
    CONSTRAINT [PK_Integrator] PRIMARY KEY ([IntegratorId])
);
GO