CREATE TABLE [dbo].[RequestSigner] (
    [RequestSignerId] bigint NOT NULL IDENTITY,
    [NPI] bigint NOT NULL,
    [FirstName] nvarchar(50) NULL,
    [LastName] nvarchar(50) NULL,
    [ProfessionalSuffix] nvarchar(20) NULL,
    [RequestId] bigint NOT NULL,
    [SutureSignUserFacilityId] int NULL,
    [RequestOrganizationId] int NULL,
    [ParentOrganizationNPI] bigint NULL,
    [ParentOrganizationName] nvarchar(250) NULL,
    [ParentOrganizationLine1] nvarchar(100) NULL,
    [ParentOrganizationLine2] nvarchar(100) NULL,
    [ParentOrganizationCity] nvarchar(60) NULL,
    [ParentOrganizationCounty] nvarchar(60) NULL,
    [ParentOrganizationStateOrProvince] nvarchar(2) NULL,
    [ParentOrganizationCountryOrRegion] nvarchar(2) NULL,
    [ParentOrganizationTelephoneNumber] nvarchar(15) NULL,
    [ParentOrganizationPostalCode] nvarchar(16) NULL,
    CONSTRAINT [PK_RequestSigner] PRIMARY KEY ([RequestSignerId]),
    CONSTRAINT [FK_RequestSigner_Request_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [TransmittedRequest] ([TransmittedRequestId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RequestSigner_RequestId] ON [dbo].[RequestSigner] ([RequestId]);
GO
