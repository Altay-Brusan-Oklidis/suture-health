CREATE TABLE [kno2].[AdditionalNotification]
(
    [Id] BIGINT NOT NULL IDENTITY,
    [ObfuscatedId] CHAR(40) NOT NULL,
    [SignerId] BIGINT NOT NULL, 
    [NotificationType] VARCHAR(10) NULL, -- 0 = Email, 1 = SMS
    [Value] NVARCHAR(1000) NULL,
    CONSTRAINT [PK_AdditionalNotification] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_AdditionalNotification_ObfuscatedId] UNIQUE ([ObfuscatedId]),
    CONSTRAINT [FK_AdditionalNotification_Signer] FOREIGN KEY ([SignerId]) REFERENCES [kno2].[Signer](Id)
)
