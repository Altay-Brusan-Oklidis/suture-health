CREATE TABLE [kno2].[Tab]
(
    [Id] BIGINT NOT NULL IDENTITY, 
    [ObfuscatedId] CHAR(40) NOT NULL,
    [SignerId] BIGINT NOT NULL, 
    [Value] NVARCHAR(50) NULL, 
    [Label] NVARCHAR(50) NULL,
    CONSTRAINT [PK_Tab] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tab_Signer] FOREIGN KEY ([SignerId]) REFERENCES [kno2].[Signer](Id)
)
