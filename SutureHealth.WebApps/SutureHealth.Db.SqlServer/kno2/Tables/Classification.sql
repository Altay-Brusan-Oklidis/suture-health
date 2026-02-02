-- TODO: most probably the Classification can be normalized. Check once we have some data.
CREATE TABLE [kno2].[Classification]
(
    [Id] BIGINT NOT NULL IDENTITY, 
    [Code] VARCHAR(50) NULL, 
    [Name] VARCHAR(50) NULL, 
    [Scheme] VARCHAR(50) NULL, 
    CONSTRAINT [PK_Classification] PRIMARY KEY ([Id])
)
