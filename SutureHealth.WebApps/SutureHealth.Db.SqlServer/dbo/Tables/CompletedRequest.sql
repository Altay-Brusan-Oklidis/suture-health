CREATE TABLE [dbo].[CompletedRequest] (
    [CompletedRequestId]    bigint not null IDENTITY,
    [UniqueRequestId]       nvarchar(36) not null,
    [ClientState]           nvarchar(max) not null,
    [Status]                nvarchar(max) null,
    [Resource]              nvarchar(max) null,
    [SubscriptionId]        nvarchar(max) not null,
    [TenantId]              bigint null
);
GO