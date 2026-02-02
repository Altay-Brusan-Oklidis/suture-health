CREATE TABLE [dbo].[MemberRole] (
    [MemberId] INT NOT NULL,
    [RoleId]   INT NOT NULL,
    CONSTRAINT [PK_MemberRole] PRIMARY KEY CLUSTERED ([MemberId], [RoleId]),
    CONSTRAINT [CHK_MemberRole_MemberId] CHECK ([dbo].[MemberIdExists] (MemberId) = 1), -- TODO: should be a foreign key to the Users
    CONSTRAINT [FK_MemberRole_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role]([RoleId]) ON DELETE CASCADE
);
