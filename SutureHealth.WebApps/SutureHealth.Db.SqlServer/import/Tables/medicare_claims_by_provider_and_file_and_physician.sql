CREATE TABLE [import].[medicare_claims_by_provider_and_file_and_physician] (
    [provider_organization_npi] BIGINT   NULL,
    [file]                      CHAR (3) NULL,
    [physician_npi]             BIGINT   NULL,
    [claim_count]               BIGINT   NULL,
    [max_claim_date]            DATE     NULL
);

