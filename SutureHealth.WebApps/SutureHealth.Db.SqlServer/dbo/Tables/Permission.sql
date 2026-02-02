CREATE TABLE [dbo].[Permission]
(
    [PermissionId]           INT IDENTITY (1, 1) NOT NULL,
    [ActionId]               INT NOT NULL,
    [ResourceIdentifierId]   INT NOT NULL,
    [Effect]                 TINYINT NOT NULL,
    CONSTRAINT [PK_Permission] PRIMARY KEY CLUSTERED ([PermissionId] ASC),
    CONSTRAINT [FK_Permission_Action_ActionId] FOREIGN KEY ([ActionId]) REFERENCES [Action]([ActionId])
    ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Permission_ResourceIdentifier_ResourceIdentifierId] FOREIGN KEY ([ResourceIdentifierId]) REFERENCES [ResourceIdentifier]([ResourceIdentifierId])
    ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [UQ_Permission_ActionIdResourceIdentifierIdEffect] UNIQUE ([ActionId], [ResourceIdentifierId], [Effect])
)
