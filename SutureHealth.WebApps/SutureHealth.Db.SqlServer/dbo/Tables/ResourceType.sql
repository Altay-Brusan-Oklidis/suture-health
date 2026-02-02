CREATE TABLE [dbo].[ResourceType]
(
    [ResourceTypeId] INT IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_ResourceType] PRIMARY KEY CLUSTERED ([ResourceTypeId] ASC),
    CONSTRAINT [UQ_ResourceType_Name] UNIQUE NONCLUSTERED ([Name])
)
