CREATE TABLE [stage].[ProviderRelationship] (
    [ProviderRelationshipId]        INT           NOT NULL,
    [SutureUserId]                  INT           NOT NULL,
    [SutureFacilityId]              INT           NOT NULL,
    [Primary]                       BIT           NULL,
    [CreatedAt]                     DATETIME2 (7) NULL,
    [EffectiveDate]                 DATETIME2 (7) NULL,
    [SubmittedById]                 INT           NULL,
    [Admin]                         BIT           NULL,
    [EditProfile]                   BIT           NULL,
    [ActiveUpdate]                  DATETIME2 (7) NULL,
    [CanSign]                       BIT           NULL,
    [UserProviderId]                BIGINT        NOT NULL,
    [FacilityProviderId]            BIGINT        NOT NULL,
    CONSTRAINT [PK_provider_user_facility] PRIMARY KEY CLUSTERED ([ProviderRelationshipId] ASC),
    CONSTRAINT [FK_provider_user_facility_provider_facility_provider_id] FOREIGN KEY ([FacilityProviderId]) REFERENCES [stage].[Provider] ([ProviderId]),
    CONSTRAINT [FK_provider_user_facility_provider_user_provider_id] FOREIGN KEY ([UserProviderId]) REFERENCES [stage].[Provider] ([ProviderId])
);


GO
CREATE NONCLUSTERED INDEX [IX_provider_user_facility_facility_provider_id]
    ON [stage].[ProviderRelationship]([FacilityProviderId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_provider_user_facility_user_provider_id]
    ON [stage].[ProviderRelationship]([UserProviderId] ASC);

