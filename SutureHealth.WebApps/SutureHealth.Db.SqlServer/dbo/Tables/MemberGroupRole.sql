CREATE TABLE [dbo].[MemberGroupRole]
(
    [MemberGroupId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    CONSTRAINT [PK_MemberGroupRole] PRIMARY KEY CLUSTERED ([MemberGroupId], [RoleId]),
    CONSTRAINT [FK_MemberGroupRole_MemberGroup_Id] FOREIGN KEY ([MemberGroupId]) REFERENCES [MemberGroup]([MemberGroupId]) ON DELETE CASCADE,
    CONSTRAINT [FK_MemberGroupRole_Role_Id] FOREIGN KEY ([RoleId]) REFERENCES [Role]([RoleId]) ON DELETE CASCADE
)
