CREATE TABLE [archive].[MedicareClaimSummaryByProviderAndFileAndPhysician] (
    [File]                     NVARCHAR (8)  NOT NULL,
    [PhysicianNPI]             BIGINT        NOT NULL,
    [ProviderId]               BIGINT        NOT NULL,
    [ProviderOrganizationNPI]  BIGINT        NOT NULL,
    [ClaimCount]               BIGINT        NOT NULL,
    [LatestClaimDate]          DATETIME2 (7) NOT NULL,
    CONSTRAINT [PK_provider_medicare_claims_by_provider_and_file_and_physician] PRIMARY KEY CLUSTERED ([ProviderId] ASC, [File] ASC, [PhysicianNPI] ASC),
    CONSTRAINT [FK_provider_medicare_claims_by_provider_and_file_and_physician_provider_provider_id] FOREIGN KEY ([ProviderId]) REFERENCES [archive].[Provider] ([ProviderId])
);


GO
CREATE NONCLUSTERED INDEX [IX_provider_medicare_claims_by_provider_and_file_and_physician_provider_organization_npi_file_physician_npi]
    ON [archive].[MedicareClaimSummaryByProviderAndFileAndPhysician]([ProviderOrganizationNPI] ASC, [File] ASC, [PhysicianNPI] ASC);

