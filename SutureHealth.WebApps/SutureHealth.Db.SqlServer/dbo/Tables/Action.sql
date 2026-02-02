CREATE TABLE [dbo].[Action]
(
    [ActionId]             INT IDENTITY (1, 1) NOT NULL,
    [Name]                 VARCHAR (127) NOT NULL,
    [San]                  VARCHAR (255) NOT NULL,
    [Description]          VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_Action] PRIMARY KEY CLUSTERED ([ActionId] ASC),
    CONSTRAINT [UQ_Action_San] UNIQUE ([San])
)
