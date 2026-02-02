CREATE TABLE [dbo].[ProviderInvitation] (
    [InvitationId]            BIGINT            IDENTITY (1, 1) NOT NULL,
    [SalesforceInvitationId]  NVARCHAR (32)     NULL,
    [QueueKey]                NVARCHAR (64)     NULL,
    [InviterUserId]           INT               NOT NULL,
    [SubmitterUserId]         INT               NULL,
    [InviteeNPI]              BIGINT            NULL,
    [InviteeFacilityId]       INT               NULL,
    [InviteeUserId]           INT               NULL,
    [CreatedAt]               DATETIME2 (7)     NOT NULL,
    [SalesStatus]             INT               NOT NULL,
    [ProcessingStatus]        INT               NOT NULL,
    [ProcessingMessage]       NVARCHAR (MAX)    NULL,
    [ReceiveDocumentTime]     INT               DEFAULT (CONVERT([int],(0))) NOT NULL,
	[UsageRequirement]        INT               DEFAULT (CONVERT([int],(0))) NOT NULL,
    CONSTRAINT [PK_provider_invitation] PRIMARY KEY CLUSTERED ([InvitationId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_provider_invitations_inviter_user_id]
    ON [dbo].[ProviderInvitation]([InviterUserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_provider_invitations_invitee_npi_invitee_facility_id_invitee_user_id]
    ON [dbo].[ProviderInvitation]([InviteeNPI] ASC, [InviteeFacilityId] ASC, [InviteeUserId] ASC);

