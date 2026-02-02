CREATE TABLE [kno2].[Signer]
(
    [Id] BIGINT NOT NULL IDENTITY, 
    [ObfuscatedId] CHAR(40) NOT NULL,
    [MessageId] BIGINT NOT NULL,
    [Email] VARCHAR(320) NULL, 
    [Name] NVARCHAR(100) NULL, 
    [HostName] VARCHAR(253) NULL, 
    [HostEmail] VARCHAR(320) NULL, 
    [PhoneNumber] VARCHAR(31) NULL, 
    [SignerType] VARCHAR(15) NULL, -- 0 = Signer, 1 = CarbonCopy, 2 = InPersonSigner
    [Order] INT NULL, 
    [Role] VARCHAR(50) NULL, 
    [CreatedDate] DATETIME2(2) NULL, 
    [UpdatedDate] DATETIME2(2) NULL,
    CONSTRAINT [PK_Signer] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Signer_ObfuscatedId] UNIQUE ([ObfuscatedId]),
    CONSTRAINT [FK_Signer_Message] FOREIGN KEY ([MessageId]) REFERENCES [kno2].[Message](Id)
)
