CREATE TABLE [archive].[MedicareClaimSummaryByProviderAndFile] (
    [File]                      NVARCHAR (8)  NOT NULL,
    [ProviderId]                BIGINT        NOT NULL,
    [ProviderOrganizationNPI]   BIGINT        NOT NULL,
    [ClaimCount]                BIGINT        NOT NULL,
    [LatestClaimDate]           DATETIME2 (7) NOT NULL,
    CONSTRAINT [PK_provider_medicare_claims_by_provider_and_file] PRIMARY KEY CLUSTERED ([ProviderId] ASC, [File] ASC),
    CONSTRAINT [FK_provider_medicare_claims_by_provider_and_file_provider_provider_id] FOREIGN KEY ([ProviderId]) REFERENCES [archive].[Provider] ([ProviderId])
);


GO
CREATE NONCLUSTERED INDEX [IX_provider_medicare_claims_by_provider_and_file_provider_organization_npi_file]
    ON [archive].[MedicareClaimSummaryByProviderAndFile]([ProviderOrganizationNPI] ASC, [File] ASC);

