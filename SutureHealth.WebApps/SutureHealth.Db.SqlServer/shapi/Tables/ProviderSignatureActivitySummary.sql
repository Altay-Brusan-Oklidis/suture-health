CREATE TABLE [shapi].[ProviderSignatureActivitySummary] (
    [ProviderSenderId] BIGINT        NOT NULL,
    [ProviderSignerId] BIGINT        NOT NULL,
    [SutureSenderId]   INT        NOT NULL,
    [SutureSignerId]   INT        NOT NULL,
    [Count]              INT           NOT NULL,
    [LastInteraction]   DATETIME2 (7) NOT NULL,
    CONSTRAINT [PK_provider_signature_activity_summary] PRIMARY KEY CLUSTERED ([ProviderSenderId] ASC, [ProviderSignerId] ASC),
    CONSTRAINT [FK_provider_signature_activity_summary_provider_provider_sender_id] FOREIGN KEY ([ProviderSenderId]) REFERENCES [shapi].[Provider] ([ProviderId]),
    CONSTRAINT [FK_provider_signature_activity_summary_provider_provider_signer_id] FOREIGN KEY ([ProviderSignerId]) REFERENCES [shapi].[Provider] ([ProviderId])
);

GO
CREATE NONCLUSTERED INDEX [IX_provider_signature_activity_summary_provider_signer_id]
    ON [shapi].[ProviderSignatureActivitySummary]([ProviderSignerId] ASC);

