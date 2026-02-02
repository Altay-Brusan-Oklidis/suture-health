CREATE TABLE [archive].[ProviderService] (
    [ProviderServiceId] BIGINT IDENTITY (1, 1) NOT NULL,
    [ServiceId]          INT    NOT NULL,
    [ProviderId]         BIGINT NOT NULL,
    CONSTRAINT [PK_provider_service] PRIMARY KEY CLUSTERED ([ProviderServiceId] ASC),
    CONSTRAINT [FK_provider_service_provider_provider_id] FOREIGN KEY ([ProviderId]) REFERENCES [archive].[Provider] ([ProviderId])
);


GO
CREATE NONCLUSTERED INDEX [IX_provider_service_provider_id]
    ON [archive].[ProviderService]([ProviderId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_provider_service_service_id]
    ON [archive].[ProviderService]([ServiceId] ASC);

