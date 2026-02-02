CREATE TABLE [archive].[MedicareClaimSummaryByProvider] (
    [ProviderId]                BIGINT        NOT NULL,
    [ProviderOrganizationNPI]   BIGINT        NOT NULL,
    [ClaimCount]                BIGINT        NOT NULL,
    [LatestClaimDate]           DATETIME2 (7) NOT NULL,
    CONSTRAINT [PK_provider_medicare_claims_by_provider] PRIMARY KEY CLUSTERED ([ProviderId] ASC),
    CONSTRAINT [FK_provider_medicare_claims_by_provider_provider_provider_id] FOREIGN KEY ([ProviderId]) REFERENCES [archive].[Provider] ([ProviderId])

);


GO
CREATE NONCLUSTERED INDEX [IX_provider_medicare_claims_by_provider_provider_organization_npi]
    ON [archive].[MedicareClaimSummaryByProvider]([ProviderOrganizationNPI] ASC);

