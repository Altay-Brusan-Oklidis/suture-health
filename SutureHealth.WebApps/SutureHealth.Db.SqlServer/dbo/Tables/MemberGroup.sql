CREATE TABLE [dbo].[MemberGroup]
(
    [MemberGroupId]          INT IDENTITY (1, 1) NOT NULL,
    [ParentMemberGroupId]    INT NULL,
    [Name]                   NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_MemberGroup] PRIMARY KEY CLUSTERED ([MemberGroupId] ASC),
    CONSTRAINT [FK_MemberGroup_MemberGroup_ParentMemberGroupId] FOREIGN KEY ([ParentMemberGroupId]) REFERENCES [MemberGroup]([MemberGroupId])
)
