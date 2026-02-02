CREATE TABLE [stage].[Provider] (
    [ProviderId]                    BIGINT         IDENTITY (1, 1) NOT NULL,
    [ProviderType]                  INT            NOT NULL,
    [AddressLine1]                  NVARCHAR (256) NULL,
    [AddressLine2]                  NVARCHAR (256) NULL,
    [City]                          NVARCHAR (150) NULL,
    [State]                         NVARCHAR (50)  NULL,
    [PostalCode]                    NVARCHAR (50)  NULL,
    [PostalCodeArea]                NVARCHAR (5)   NULL,
    [EmailAddress]                  NVARCHAR (250) NULL,
    [TelephoneNumber]               NVARCHAR (250) NULL,
    [FaxNumber]                     NVARCHAR (250) NULL,
    [FirstName]                     NVARCHAR (50)  NULL,
    [MiddleName]                    NVARCHAR (50)  NULL,
    [LastName]                      NVARCHAR (50)  NULL,
    [Suffix]                        NVARCHAR (5)   NULL,
    [ProfessionalSuffix]            NVARCHAR (50)  NULL,
    [Name]                          NVARCHAR (256) NULL,
    [SigningName]                   NVARCHAR (256) NULL,
    [Latitude]                      DECIMAL (9, 6) NULL,
    [Longitude]                     DECIMAL (9, 6) NULL,
    [ServiceCount]                  INT            NOT NULL,
    [NPI]                           BIGINT         NULL,
    [IsActiveInSutureSign]          BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [IsCollaborator]                BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [IsNetworkProvider]             BIT            DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [IsNPIActive]                   BIT            NULL,
    [IsSender]                      BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [IsSigner]                      BIT            NOT NULL,
    [CanSign]                       BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [SutureCreatedAt]               DATETIME2 (7)  NULL,
    [SutureCloseDate]               DATETIME2 (7)  NULL,
    [SutureUpdatedAt]               DATETIME2 (7)  NULL,
    [SutureCustomerType]            TINYINT        DEFAULT (CONVERT([tinyint],(0))) NOT NULL,
    [SutureFacilityId]              INT            NULL,
    [SutureFacilityTypeId]          INT            NULL,
    [SuturePrimaryFacilityId]       INT            NULL,
    [SuturePrimaryFacilityName]     NVARCHAR (256) NULL,
    [SutureUserId]                  INT            NULL,
    [SutureUserTypeId]              INT            NULL,
    CONSTRAINT [PK_provider] PRIMARY KEY CLUSTERED ([ProviderId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_provider_npi]
    ON [stage].[Provider]([NPI] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_provider_suture_facility_id]
    ON [stage].[Provider]([SutureFacilityId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_provider_provider_primary_facility_id]
    ON [stage].[Provider]([SuturePrimaryFacilityId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_provider_suture_user_id]
    ON [stage].[Provider]([SutureUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_network_provider_default_index]
    ON [stage].[Provider]([SutureCreatedAt]) INCLUDE ([Latitude],[Longitude],[SutureCustomerType],[IsNetworkProvider],[SutureCloseDate]);
    


GO