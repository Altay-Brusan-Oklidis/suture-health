CREATE TABLE [dbo].[ResourceIdentifier]
(
    [ResourceIdentifierId] INT IDENTITY (1, 1) NOT NULL,
    [ResourceTypeId]       INT NOT NULL,
    [Srn]                  NVARCHAR(848) NOT NULL,
    CONSTRAINT [PK_ResourceIdentifier] PRIMARY KEY CLUSTERED ([ResourceIdentifierId] ASC),
    CONSTRAINT [FK_ResourceIdentifier_ResourceType_ResourceTypeId] FOREIGN KEY ([ResourceTypeId]) REFERENCES [ResourceType]([ResourceTypeId])
    ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [UQ_ResourceIdentifier_ResourceTypeIdSrn] UNIQUE NONCLUSTERED ([ResourceTypeId], [Srn])
)
