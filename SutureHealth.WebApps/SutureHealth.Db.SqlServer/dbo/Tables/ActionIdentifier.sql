CREATE TABLE [dbo].[ActionIdentifier] (
    [ActionIdentifierId]  INT           IDENTITY (1, 1) NOT NULL,
    [DomainObject]        VARCHAR (127) NOT NULL,
    [ActionName]          VARCHAR (127) NOT NULL,
    [ActionIdentifierKey] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_ActionIdentifier] PRIMARY KEY CLUSTERED ([ActionIdentifierId] ASC)
);

