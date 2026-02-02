CREATE TABLE [kno2].[Conversation]
(
    [Id] BIGINT NOT NULL IDENTITY, 
    [ObfuscatedId] CHAR(40) NOT NULL, 
    [ConversationStatus] VARCHAR(20) NULL, -- 0 = Received, 1 = Accepted, 2 = Declined, 3 = Completed, 4 = Cancelled, 5 = Scheduled, 6 = Rescheduled, 7 = Draft, 8 = Pending, 9 = ResponseReceived
    [Type] NVARCHAR(50) NULL, 
    CONSTRAINT [PK_Conversation] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Conversation_ObfuscatedId] UNIQUE ([ObfuscatedId])
)
