CREATE TABLE [dbo].[Service] (
    [ServiceId]         INT            IDENTITY (1, 1) NOT NULL,
    [Code]               NVARCHAR (64)  NULL,
    [Source]             NVARCHAR (64)  NULL,
    [Description]        NVARCHAR (128) NULL,
    [SourceDescription] NVARCHAR (128) NULL,
    CONSTRAINT [PK_service] PRIMARY KEY CLUSTERED ([ServiceId] ASC)
);

