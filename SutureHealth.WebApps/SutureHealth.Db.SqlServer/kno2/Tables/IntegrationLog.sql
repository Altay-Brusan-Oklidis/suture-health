CREATE TABLE [kno2].[IntegrationLog]
(
    [MessageId] BIGINT NOT NULL,
    [Date] DATETIME2(2) NULL, 
    [ReferenceId] BIGINT NULL, 
    [Message] NVARCHAR(4000) NULL,
    CONSTRAINT [FK_IntegrationLog_Message] FOREIGN KEY ([MessageId]) REFERENCES [kno2].[Message](Id)
)
