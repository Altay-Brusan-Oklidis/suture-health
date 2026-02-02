CREATE TABLE [import].[medicare_claims_by_provider_and_file] (
    [provider_organization_npi] BIGINT   NULL,
    [file]                      CHAR (3) NULL,
    [claim_count]               BIGINT   NULL,
    [max_claim_date]            DATE     NULL
);

